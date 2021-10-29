using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace Dms.Common.Binary
{
    /// <summary>
    /// Used for reading data from a binary request
    /// </summary>
    public class BinaryRequestReader : IDisposable
    {
        public Memory<byte> PayloadData => DisposableBuffer.Memory;
        public RequestTypes Type => (RequestTypes)BitConverter.ToInt16(PayloadData.Span.Slice(0, Protocol.RequestTypeSize));
        public Guid Id => new(PayloadData.Slice(Protocol.RequestTypeSize, Protocol.GuidSize).Span);
        public DisposableBuffer DisposableBuffer { get; }

        private int _offset;
        private readonly byte[] _sharedKeyBuffer;

        public BinaryRequestReader(DisposableBuffer disposableBuffer)
        {
            DisposableBuffer = disposableBuffer;
            _sharedKeyBuffer = ArrayPool<Byte>.Shared.Rent(8);
            _offset = Protocol.GuidSize + Protocol.RequestTypeSize;
        }


        public string ReadNextString()
        {
            var sizeToRead = ReadSize();
            
            Debug.Assert(PayloadData.Slice(_offset).Length >= sizeToRead);

            _offset += Protocol.KeySize;

            return Encoding.UTF8.GetString(PayloadData.Slice(_offset, sizeToRead).Span);
        }

        public DateTime ReadNextDateTime()
        {
            Debug.Assert(PayloadData.Slice(_offset).Length >= Protocol.DateTimeSize);

            var dateTimeLong = BitConverter.ToInt64(PayloadData.Slice(_offset, Protocol.DateTimeSize).Span);
            
            _offset += Protocol.DateTimeSize;
    
            return DateTime.FromBinary(dateTimeLong);
        }

        public Memory<byte> ReadNextBinary()
        {
            var sizeToRead = ReadSize();
            
            Debug.Assert(PayloadData.Slice(_offset).Length >= sizeToRead);

            _offset += Protocol.KeySize;

            return PayloadData.Slice(_offset, sizeToRead);
        }

        private int ReadSize()
        {
            Debug.Assert(PayloadData.Slice(_offset).Length >= Protocol.KeySize);
            return BitConverter.ToInt32(PayloadData.Slice(_offset, Protocol.KeySize).Span);
        }

        public void Dispose()
        {
            DisposableBuffer?.Dispose();
            ArrayPool<Byte>.Shared.Return(_sharedKeyBuffer);
        }
    }
}