using System;
using System.Buffers;
using System.Diagnostics;
using Dms.Common.Buffers;
using Dms.Common.ProtocolSpec;

namespace Dms.Core.Commands
{
    public class CommandInput : IDisposable
    {
        public Memory<Byte> PayloadData => DisposableBuffer.Memory[Protocol.CommandTypeSize..];
        public Guid Id => new(PayloadData.Slice(Protocol.GuidSize).Span);
        public DisposableBuffer DisposableBuffer { get; }

        private int _offset;
        private readonly byte[] _sharedKeyBuffer;

        public CommandInput(DisposableBuffer disposableBuffer)
        {
            DisposableBuffer = disposableBuffer;
            _sharedKeyBuffer = ArrayPool<Byte>.Shared.Rent(8);
            _offset = Protocol.GuidSize;
        }


        public string ReadNextString()
        {
            Debug.Assert(PayloadData.Slice(_offset).Length >= Protocol.KeySize);

            var lengthToRead = BitConverter.ToInt32(PayloadData.Slice(_offset, Protocol.KeySize).Span);
            
            Debug.Assert(PayloadData.Slice(_offset).Length);
        }

        public DateTime ReadNextDateTime()
        {
        }

        public byte[] ReadNextBinary()
        {
        }

        public void Dispose()
        {
            DisposableBuffer?.Dispose();
            ArrayPool<Byte>.Shared.Return(_sharedKeyBuffer);
        }
    }
}