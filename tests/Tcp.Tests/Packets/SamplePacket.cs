using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tcp.Tests.Packets
{
    public class SamplePacket
    {
        private static readonly Random random = new ();
        public byte[] Buffer { get; }
        public string Payload { get; }

        public SamplePacket(int size)
        {
            Buffer = new byte[size + 4];

            BitConverter.TryWriteBytes(Buffer, size);

            Payload = RandomString(size);
            
            Encoding.UTF8.GetBytes(Payload).CopyTo(Buffer, 4);
        }
        
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
    }
}