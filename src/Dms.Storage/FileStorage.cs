using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dms.Common.Configurations;
using Dms.Common.Logging;
using Microsoft.Extensions.Logging;

namespace Dms.Storage;

public class FileStorage : IStorage
{
    private readonly StorageConfig _config;
    private readonly ILogger<FileStorage> _logger;
    private readonly byte[] _sharedHeaderBuff;
    private readonly byte[] _sharedKeyBuffer;
    private readonly Dictionary<string, StorageRecord> _store;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _timer;
    private FileStream _fileStream;

    private long _currentOffset;
    private bool _isInitialized;

    private const int KeySize = 4;
    private const int ValueSize = 8;
    private const int DeleteFlagSize = 1;

    public StorageConfig Config => _config;

    public FileStorage(StorageConfig config)
    {
        _config = config;
        _semaphore = new(1, 1);
        _logger = LogProvider.GetLogger<FileStorage>();
        _store = new();
        _sharedHeaderBuff = new byte[KeySize + ValueSize + DeleteFlagSize];
        _sharedKeyBuffer = new byte[1024];

        _fileStream = File.Open(_config.DbFilePath, FileMode.OpenOrCreate);

        var timeSpan = TimeSpan.FromMinutes(config.VacuumPeriodInMinutes);
        _timer = new Timer(OnVacuumTimer, null, timeSpan, timeSpan);
    }


    public ValueTask InitializeAsync()
    {
        _logger.LogInformation("Beginning parsing file content");

        try
        {
            ParseFileContent();
            _isInitialized = true;
        }
        catch
        {
            _logger.LogInformation($"Exception while trying to parse the content of path {_config.DbFilePath}");
            throw;
        }

        _logger.LogInformation("Completed parsing file content");

        return ValueTask.CompletedTask;
    }

    public ValueTask<Memory<byte>> ReadAsync(string key)
    {
        AssertInitialized();

        try
        {
            _semaphore.WaitAsync();

            if (_store.TryGetValue(key, out var result))
            {
                return ValueTask.FromResult(result.Value);
            }

            return ValueTask.FromResult(Memory<byte>.Empty);
        }
        finally

        {
            _semaphore.Release();
        }
    }


    public async ValueTask WriteAsync(string key, Memory<byte> value)
    {
        AssertInitialized();

        try
        {
            await _semaphore.WaitAsync();

            var binaryValue = new StorageRecord
            {
                Offset = _currentOffset,
                Value = value
            };

            _store[key] = binaryValue;

            // seek to current position
            _fileStream.Seek(_currentOffset, SeekOrigin.Begin);

            // write record to file stream
            var recordSize = await WriteRecordAsync(_fileStream, key, value);

            // update offset
            _currentOffset += recordSize;

            _fileStream.Flush();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DeleteAsync(string key)
    {
        AssertInitialized();

        try
        {
            await _semaphore.WaitAsync();

            if (_store.Remove(key, out var result))
            {
                // seek to header info position
                _fileStream.Seek(result.Offset, SeekOrigin.Begin);

                // only set the flag to deleted
                BitConverter.TryWriteBytes(_sharedHeaderBuff.AsSpan(KeySize + ValueSize, DeleteFlagSize), true);

                // write header
                await _fileStream.WriteAsync(_sharedHeaderBuff);

                _fileStream.Flush();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask<VacuumStat> VacuumStatAsync()
    {
        AssertInitialized();

        try
        {
            await _semaphore.WaitAsync();

            var headerInfoLength = KeySize + ValueSize + DeleteFlagSize;
            var totalNumberOfRecords = 0;
            var numberOfActiveRecords = 0;

            var currentOffset = 0L;

            _fileStream.Seek(0, SeekOrigin.Begin);

            while (currentOffset < _fileStream.Length)
            {
                _fileStream.Seek(currentOffset, SeekOrigin.Begin);

                _fileStream.Read(_sharedHeaderBuff);

                // read header info
                var (keySize, valueSize, deleted) = ReadHeader(_sharedHeaderBuff);

                // update offset
                currentOffset += headerInfoLength + keySize + valueSize;

                totalNumberOfRecords += 1;

                if (!deleted)
                {
                    numberOfActiveRecords += 1;
                }
            }

            return new VacuumStat { TotalNumberOfRecords = totalNumberOfRecords, NumberOfActiveRecords = numberOfActiveRecords };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask VacuumAsync()
    {
        AssertInitialized();

        try
        {
            await _semaphore.WaitAsync();

            var newFilePath = _config.DbFilePath + "_v";

            await using var fileStream = File.Open(newFilePath, FileMode.OpenOrCreate);

            foreach (var (key, binaryValue) in _store)
            {
                await WriteRecordAsync(fileStream, key, binaryValue.Value);
            }

            _logger.LogInformation($"Size reduced from {_fileStream.Length} to {fileStream.Length}");

            fileStream.Flush();

            await _fileStream.DisposeAsync();
            await fileStream.DisposeAsync();

            File.Move(newFilePath, _config.DbFilePath, true);

            _fileStream = File.Open(_config.DbFilePath, FileMode.OpenOrCreate);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void ParseFileContent()
    {
        var headerInfoLength = KeySize + ValueSize + DeleteFlagSize;

        var offset = 0L;

        while (offset < _fileStream.Length)
        {
            // seek to position where next record is
            // the reason we seek is for records that are deleted 
            // the key and value will not be read and the position of
            // file stream must be set manually
            _fileStream.Seek(offset, SeekOrigin.Begin);

            // read the header info
            _fileStream.Read(_sharedHeaderBuff);

            // read key size, value size and deleted
            var (keySize, valueSize, deleted) = ReadHeader(_sharedHeaderBuff);

            // if the object isn't deleted
            if (!deleted)
            {
                // get span with size of key
                var keyBufferSpan = _sharedKeyBuffer.AsSpan(0, keySize);

                // read the data of span
                _fileStream.Read(keyBufferSpan);

                // get string for key
                var key = Encoding.UTF8.GetString(keyBufferSpan);

                // create a buffer for value with the size
                var valueBuffer = new byte[valueSize];

                _fileStream.Read(valueBuffer);

                _store[key] = new StorageRecord
                {
                    Offset = offset,
                    Value = valueBuffer
                };
            }

            // add the header info length, key size and value size to offset
            offset += headerInfoLength;
            offset += keySize;
            offset += valueSize;
        }

        _currentOffset = offset;
    }

    private void AssertInitialized()
    {
        if (!_isInitialized)
        {
            throw new Exception("The storage hasn't been initialized yet");
        }
    }

    private (int KeySize, long ValueSize, bool deleted) ReadHeader(byte[] buffer)
    {
        // read key size, value size and deleted
        var keySize = BitConverter.ToInt32(buffer.AsSpan(0, KeySize));
        var valueSize = BitConverter.ToInt64(buffer.AsSpan(KeySize, ValueSize));
        var deleted = BitConverter.ToBoolean(buffer.AsSpan(KeySize + ValueSize, DeleteFlagSize));

        return (keySize, valueSize, deleted);
    }

    private async ValueTask<long> WriteRecordAsync(FileStream fileStream, string key, Memory<byte> value)
    {
        var headerInfoLength = KeySize + ValueSize + DeleteFlagSize;

        BitConverter.TryWriteBytes(_sharedHeaderBuff.AsSpan(0, KeySize), key.Length);
        BitConverter.TryWriteBytes(_sharedHeaderBuff.AsSpan(KeySize, ValueSize), value.Length);
        BitConverter.TryWriteBytes(_sharedHeaderBuff.AsSpan(KeySize + ValueSize, DeleteFlagSize), false);

        // write the key to shared key buffer
        Encoding.UTF8.GetBytes(key, _sharedKeyBuffer);

        // write header
        await fileStream.WriteAsync(_sharedHeaderBuff);

        // write key
        await fileStream.WriteAsync(_sharedKeyBuffer.AsMemory(0, key.Length));

        // write value
        await fileStream.WriteAsync(value);

        return headerInfoLength + key.Length + value.Length;
    }

    private async void OnVacuumTimer(object _)
    {
        var vacuumStat = await VacuumStatAsync();

        var rationOfActiveRecords = vacuumStat.NumberOfActiveRecords / (float)vacuumStat.TotalNumberOfRecords;

        if ((1 - rationOfActiveRecords) > _config.VacuumThreshold)
        {
            await VacuumAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _fileStream.Flush();
        await _timer.DisposeAsync();
        await _fileStream.DisposeAsync();
    }
}