using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dms.Common.Buffers;

namespace Dms.Tcp
{
    public class Packet: IDisposable
    {
        public DisposableBuffer Payload { get; init; }
        public bool IsMalformed { get; init; }
        
        public Memory<byte> Type => Payload.Memory.Slice(0, 6);
        public Guid Id => new Guid(Payload.Memory.Slice(7, 17).Span);

        public static async ValueTask<Packet> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            using var headerDisposableBuffer = new DisposableBuffer(4);

            // read 4 bytes from payload size
            var headerSizeReceived = await stream.ReadAsync(headerDisposableBuffer.Memory, cancellationToken);

            // if header value is malformed return a malformed packet
            if (headerSizeReceived != 4)
            {
                return new Packet { IsMalformed = true };
            }

            // convert header to int32
            var payloadSize = BitConverter.ToInt32(headerDisposableBuffer.Memory.Span);
            
            var payloadDisposableBuffer = new DisposableBuffer(payloadSize);

            // read exactly payload length from stream
            await stream.ReadAsync(payloadDisposableBuffer.Memory, cancellationToken);

            return new Packet
            {
                Payload = payloadDisposableBuffer
            };
        }

        public void Dispose()
        {
            Payload?.Dispose();
        }
    }
}