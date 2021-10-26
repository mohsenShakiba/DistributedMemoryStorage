using System;
using System.Text;

namespace Tcp.Tests.Packets
{
    public class AuthenticationPacket
    {
        public const string SecurityKey = "SECURITY_KEY";
        public byte[] Payload { get; }
        
        public AuthenticationPacket()
        {
            Payload = new Byte[4 + SecurityKey.Length];
            BitConverter.TryWriteBytes(Payload, SecurityKey.Length);
            Encoding.UTF8.GetBytes(SecurityKey).CopyTo(Payload, 4);
        }
    }
}