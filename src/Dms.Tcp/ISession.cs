using System;
using System.Threading.Tasks;

namespace Dms.Tcp;

public interface ISession: IAsyncDisposable
{
    void HandlePackets();
    ValueTask SendAsync(Memory<byte> payload);
}