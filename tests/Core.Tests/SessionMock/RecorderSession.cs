using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Tcp;

namespace Core.Tests.SessionMock;

public class RecorderSession: ISession
{
    public List<DisposableBuffer> RecordedData = new();


    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void HandlePackets()
    {
    }

    public ValueTask SendAsync(Memory<byte> payload)
    {
        var disposableBuffer = new DisposableBuffer(payload.Length);
        payload.CopyTo(disposableBuffer.Memory);
        RecordedData.Add(disposableBuffer);
        return ValueTask.CompletedTask;
    }
}