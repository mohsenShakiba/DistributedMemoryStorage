using System;

namespace Tcp.Tests.Packets;

public class InvalidPacket
{

    public byte[] Buffer;
        
    public InvalidPacket()
    {
        Buffer = new Byte[3];
    }
}