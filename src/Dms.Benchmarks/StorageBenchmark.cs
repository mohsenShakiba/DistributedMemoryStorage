using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Dms.Common.Configurations;
using Dms.Storage;

namespace Dms.Benchmarks
{
    public class StorageBenchmark
    {
        private readonly FileStorage _storage;
        private readonly string _sampleKey;
        private readonly byte[] _sampleValue;

        public StorageBenchmark()
        {
            var config = new StorageConfig
            {
                VacuumThreshold = 0.9f,
                DbFilePath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString()),
                VacuumPeriodInMinutes = 1
            };

            _sampleKey = Guid.NewGuid().ToString();
            _sampleValue = Guid.NewGuid().ToByteArray();

            _storage = new FileStorage(config);

            _storage.InitializeAsync().GetAwaiter().GetResult();
        }

        [Benchmark]
        public async Task WriteNormal()
        {
            await _storage.WriteAsync(_sampleKey, _sampleValue);
        }


        [Benchmark]
        public async Task Read()
        {
            await _storage.ReadAsync(_sampleKey);
        }
    }
}