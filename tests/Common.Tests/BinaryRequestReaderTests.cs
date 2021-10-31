using System;
using System.Linq;
using System.Text;
using Dms.Common.Binary;
using Storage.Tests.RandomGeneration;
using Xunit;

namespace Common.Tests;

public class BinaryRequestReaderTests
{
    [Fact]
    public void ReadGuid()
    {
        var guid = Guid.NewGuid();
        var disposableBuffer = CreateBufferWithIdAndType(guid);
        var binaryRequestReader = new BinaryRequestReader(disposableBuffer);

        Assert.Equal(guid, binaryRequestReader.Id);
    }
    
    [Fact]
    public void ReadType()
    {
        var type = RequestTypes.StringGet;
        var disposableBuffer = CreateBufferWithIdAndType(null, (short)type);
        var binaryRequestReader = new BinaryRequestReader(disposableBuffer);
        
        Assert.Equal((short)type, binaryRequestReader.Type);
    }

    [Fact]
    public void ReadString()
    {
        var randomLength = 42;
        var randomString = RandomDataGenerator.RandomString(randomLength);
        var disposableBuffer = CreateBufferWithIdAndType();
        var currentOffset = Protocol.RequestTypeSize + Protocol.GuidSize;
        var binaryRequestReader = new BinaryRequestReader(disposableBuffer);

        BitConverter.TryWriteBytes(disposableBuffer.Memory.Span.Slice(currentOffset), randomLength);

        currentOffset += Protocol.KeySize;
        
        Encoding.UTF8.GetBytes(randomString, disposableBuffer.Memory.Span.Slice(currentOffset));
        
        Assert.Equal(randomString, binaryRequestReader.ReadNextString());
    }

    [Fact]
    public void ReadDateTime()
    {
        var randomDateTime = DateTime.Now;
        var disposableBuffer = CreateBufferWithIdAndType();
        var currentOffset = Protocol.RequestTypeSize + Protocol.GuidSize;
        var binaryRequestReader = new BinaryRequestReader(disposableBuffer);

        BitConverter.TryWriteBytes(disposableBuffer.Memory.Span.Slice(currentOffset), randomDateTime.ToBinary());
        
        Assert.Equal(randomDateTime, binaryRequestReader.ReadNextDateTime());
    }

    [Fact]
    public void ReadBinary()
    {
        var randomLength = 42;
        var randomBinaryArray = RandomDataGenerator.RandomBinary(randomLength);
        var disposableBuffer = CreateBufferWithIdAndType();
        var currentOffset = Protocol.RequestTypeSize + Protocol.GuidSize;
        var binaryRequestReader = new BinaryRequestReader(disposableBuffer);

        BitConverter.TryWriteBytes(disposableBuffer.Memory.Span.Slice(currentOffset), randomLength);

        currentOffset += Protocol.KeySize;
        
        randomBinaryArray.CopyTo(disposableBuffer.Memory.Slice(currentOffset));
        
        Assert.Equal(randomBinaryArray.ToArray(), binaryRequestReader.ReadNextBinary().ToArray());
    }

    private DisposableBuffer CreateBufferWithIdAndType(Guid? id = null, short? type = null)
    {
        var disposableBuffer = new DisposableBuffer(1024);

        id ??= Guid.NewGuid();
        type ??= 0;
        
        BitConverter.TryWriteBytes(disposableBuffer.Memory.Span.Slice(0, Protocol.RequestTypeSize), type.Value);
        id.Value.TryWriteBytes(disposableBuffer.Memory.Span.Slice(Protocol.RequestTypeSize, Protocol.GuidSize));

        return disposableBuffer;
    }
}