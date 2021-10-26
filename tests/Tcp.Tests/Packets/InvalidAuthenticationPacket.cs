using System;
using System.Text;

namespace Tcp.Tests.Packets
{
    public class InvalidAuthenticationPacket
    {
        public const string SecurityKey = "SECURITY_KEY_1";
        public byte[] Payload { get; }
        
        public InvalidAuthenticationPacket()
        {
            Payload = new Byte[4 + SecurityKey.Length];
            BitConverter.TryWriteBytes(Payload, SecurityKey.Length);
            Encoding.UTF8.GetBytes(SecurityKey).CopyTo(Payload, 4);
        }
    }
}