using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Tests.SessionMock;
using Dms.Common.Binary;
using Dms.Core;
using Dms.Storage;
using Dms.Tcp;
using Moq;
using Storage.Tests.RandomGeneration;
using Xunit;

namespace Core.Tests;

public class StringCommandTests
{
    [Fact]
    public async Task StringGet_InvalidInput_NackIsReceived()
    {
        // create data
        var validKey = RandomDataGenerator.RandomString(8);
        var invalidKey = RandomDataGenerator.RandomString(8);
        var randomValue = RandomDataGenerator.RandomBinary(128);
        var commandId = Guid.NewGuid();

        // create objects
        var router = new CommandRouter();
        var storageMock = new Mock<IStorage>();
        var sessionMock = new RecorderSession();
        var engine = new CommandEngine(storageMock.Object, router);

        // create input
        var responseWriter = new BinaryResponseWriter();
        responseWriter.WriteType(RequestTypes.StringGet);
        responseWriter.WriteGuid(commandId);
        responseWriter.WriteString(invalidKey);

        // create packet
        var packet = new Packet
        {
            Payload = responseWriter.DisposableBuffer
        };

        // setup repository
        storageMock.Setup(s => s.ReadAsync(validKey)).ReturnsAsync(randomValue);
        storageMock.Setup(s => s.ReadAsync(invalidKey)).ReturnsAsync(Memory<byte>.Empty);

        // run the command
        await engine.ExecuteAsync(sessionMock, packet);

        // create reader from session recorded data
        var requestReader = new BinaryRequestReader(sessionMock.RecordedData.First());

        Assert.Equal(ResponseTypes.Nack, (ResponseTypes)requestReader.Type);
    }

    [Fact]
    public async Task StringGet_ValidInput_ValueIsReturned()
    {
        // create data
        var randomKey = RandomDataGenerator.RandomString(8);
        var randomValue = RandomDataGenerator.RandomBinary(128);
        var commandId = Guid.NewGuid();

        // create objects
        var router = new CommandRouter();
        var storageMock = new Mock<IStorage>();
        var sessionMock = new RecorderSession();
        var engine = new CommandEngine(storageMock.Object, router);

        // create input
        var responseWriter = new BinaryResponseWriter();
        responseWriter.WriteType(RequestTypes.StringGet);
        responseWriter.WriteGuid(commandId);
        responseWriter.WriteString(randomKey);

        // create packet
        var packet = new Packet
        {
            Payload = responseWriter.DisposableBuffer
        };

        // setup repository
        storageMock.Setup(s => s.ReadAsync(randomKey)).ReturnsAsync(randomValue);

        // run the command
        await engine.ExecuteAsync(sessionMock, packet);

        // create reader from session recorded data
        var requestReader = new BinaryRequestReader(sessionMock.RecordedData.First());

        Assert.Equal(ResponseTypes.StringGet, (ResponseTypes)requestReader.Type);
        Assert.Equal(randomValue, requestReader.ReadNextBinary().ToArray());
    }

    [Fact]
    public async Task StringSet_InvalidInput_NackIsReceived()
    {
        // create data
        var randomKey = RandomDataGenerator.RandomString(8);
        var commandId = Guid.NewGuid();

        // create objects
        var router = new CommandRouter();
        var storageMock = new Mock<IStorage>();
        var sessionMock = new RecorderSession();
        var engine = new CommandEngine(storageMock.Object, router);

        // create input
        var responseWriter = new BinaryResponseWriter();
        responseWriter.WriteType(RequestTypes.StringSet);
        responseWriter.WriteGuid(commandId);
        responseWriter.WriteString(randomKey);

        // create packet
        var packet = new Packet
        {
            Payload = responseWriter.DisposableBuffer
        };

        // run the command
        await engine.ExecuteAsync(sessionMock, packet);

        // create reader from session recorded data
        var requestReader = new BinaryRequestReader(sessionMock.RecordedData.First());

        Assert.Equal(ResponseTypes.Nack, (ResponseTypes)requestReader.Type);
    }

    [Fact]
    public async Task StringSet_ValidInput_AckIsReceived()
    {
        // create data
        var randomKey = RandomDataGenerator.RandomString(8);
        var randomValue = RandomDataGenerator.RandomBinary(128);
        var commandId = Guid.NewGuid();

        // create objects
        var router = new CommandRouter();
        var storageMock = new Mock<IStorage>();
        var sessionMock = new RecorderSession();
        var engine = new CommandEngine(storageMock.Object, router);

        // create input
        var responseWriter = new BinaryResponseWriter();
        responseWriter.WriteType(RequestTypes.StringSet);
        responseWriter.WriteGuid(commandId);
        responseWriter.WriteString(randomKey);
        responseWriter.WriteMemory(randomValue);

        // create packet
        var packet = new Packet
        {
            Payload = responseWriter.DisposableBuffer
        };

        // run the command
        await engine.ExecuteAsync(sessionMock, packet);

        // create reader from session recorded data
        var requestReader = new BinaryRequestReader(sessionMock.RecordedData.First());

        Assert.Equal(ResponseTypes.Ack, (ResponseTypes)requestReader.Type);
        storageMock.Verify(s => s.WriteAsync(randomKey, randomValue));
    }

    [Fact]
    public async Task StringDelete_ValidInput_AckIsReceived()
    {
        // create data
        var randomKey = RandomDataGenerator.RandomString(8);
        var commandId = Guid.NewGuid();

        // create objects
        var router = new CommandRouter();
        var storageMock = new Mock<IStorage>();
        var sessionMock = new RecorderSession();
        var engine = new CommandEngine(storageMock.Object, router);

        // create input
        var responseWriter = new BinaryResponseWriter();
        responseWriter.WriteType(RequestTypes.StringDelete);
        responseWriter.WriteGuid(commandId);
        responseWriter.WriteString(randomKey);

        // create packet
        var packet = new Packet
        {
            Payload = responseWriter.DisposableBuffer
        };

        // run the command
        await engine.ExecuteAsync(sessionMock, packet);

        // create reader from session recorded data
        var requestReader = new BinaryRequestReader(sessionMock.RecordedData.First());

        Assert.Equal(ResponseTypes.Ack, (ResponseTypes)requestReader.Type);
        storageMock.Verify(s => s.DeleteAsync(randomKey));
    }
}