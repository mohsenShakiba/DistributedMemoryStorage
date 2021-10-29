using System;
using System.Text;
using Dms.Common.Binary;
using Storage.Tests.RandomGeneration;
using Xunit;

namespace Common.Tests;

public class BinaryResponseWriteTests
{
    [Fact]
    public void WriteType()
    {
        var type = ResponseTypes.StringGet;
        var binaryResponseWriter = CreateResponseWriteWithTypeAndId(null, type);
        var typeFromWriter = BitConverter.ToInt16(binaryResponseWriter.Buffer.Span.Slice(0, Protocol.ResponseTypeSize));
        
        Assert.Equal((short)type, typeFromWriter);
    }

    [Fact]
    public void WriteGuid()
    {
        var guid = Guid.NewGuid();
        var binaryResponseWriter = CreateResponseWriteWithTypeAndId(guid);
        var currentOffset = Protocol.ResponseTypeSize;
        var guidFromWriter = new Guid(binaryResponseWriter.Buffer.Span.Slice(currentOffset, Protocol.GuidSize));

        Assert.Equal(guid, guidFromWriter);
    }

    [Fact]
    public void WriteString()
    {
        var randomSize = 42;
        var randomString = RandomDataGenerator.RandomString(randomSize);
        var binaryResponseWriter = CreateResponseWriteWithTypeAndId();
        var currentOffset = Protocol.ResponseTypeSize + Protocol.GuidSize;
        
        binaryResponseWriter.WriteString(randomString);

        var stringDataFromWriter = binaryResponseWriter.Buffer.Span.Slice(currentOffset, randomSize);
        var stringFromWriter = Encoding.UTF8.GetString(stringDataFromWriter);
        
        Assert.Equal(randomString, stringFromWriter);

    }

    [Fact]
    public void WriteBinary()
    {
        var randomSize = 42;
        var randomBinary = RandomDataGenerator.RandomBinary(randomSize);
        var binaryResponseWriter = CreateResponseWriteWithTypeAndId();
        var currentOffset = Protocol.ResponseTypeSize + Protocol.GuidSize;
        
        binaryResponseWriter.WriteMemory(randomBinary);

        var binaryFromWriter = binaryResponseWriter.Buffer.Span.Slice(currentOffset, randomSize);
        
        Assert.Equal(randomBinary, binaryFromWriter.ToArray());
    }

    private BinaryResponseWriter CreateResponseWriteWithTypeAndId(Guid? id = null, ResponseTypes? type = null)
    {
        var responseWrite = new BinaryResponseWriter();
        responseWrite.WriteType(type ?? ResponseTypes.Ack);
        responseWrite.WriteGuid(id ?? Guid.NewGuid());
        return responseWrite;
    }
}