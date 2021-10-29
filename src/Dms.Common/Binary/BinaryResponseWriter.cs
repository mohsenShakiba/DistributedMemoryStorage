using System;
using System.Diagnostics;
using System.Text;

namespace Dms.Common.Binary
{
    /// <summary>
    /// Used to write binary data to a buffer
    /// </summary>
    public class BinaryResponseWriter: IDisposable
    {
        // if set will make sure enough bytes are available in buffer 
        // and will grow the buffer if necessary
        private readonly bool _assertBytesAvailability;
        
        // the inner buffer used for write data to
        private DisposableBuffer _buffer;
        
        // current offset at which data should be written
        private int _offset;
        
        private Span<byte> PayloadBuffer => _buffer.Memory.Span.Slice(_offset);

        public Memory<byte> Buffer => _buffer.Memory;

        public BinaryResponseWriter(int sizeRequired)
        {
            _buffer = new DisposableBuffer(sizeRequired);
        }

        public BinaryResponseWriter(): this(1024)
        {
            _assertBytesAvailability = true;
        }

        public void WriteType(ResponseTypes type)
        {
            AssertEnoughBytesAreAvailable(Protocol.RequestTypeSize);
            BitConverter.TryWriteBytes(PayloadBuffer, (short)type);
            _offset += Protocol.RequestTypeSize;
        }
        

        public void WriteGuid(Guid guid)
        {
            AssertEnoughBytesAreAvailable(Protocol.GuidSize);
            guid.TryWriteBytes(PayloadBuffer);
            _offset += Protocol.GuidSize;
        }
        
        public void WriteMemory(Memory<byte> mem)
        {
            AssertEnoughBytesAreAvailable(mem.Length);
            mem.Span.CopyTo(PayloadBuffer);
            _offset += mem.Length;
        }

        public void WriteString(string str)
        {
            var size = Encoding.UTF8.GetByteCount(str);
            AssertEnoughBytesAreAvailable(size);
            Encoding.UTF8.GetBytes(str, PayloadBuffer);
            _offset += size;
        }

        private void AssertEnoughBytesAreAvailable(int size)
        {
            if (_assertBytesAvailability)
            {
                if (PayloadBuffer.Length < size)
                {
                    var newBuffer = new DisposableBuffer(_buffer.Length * 2);
                    _buffer.Memory.CopyTo(newBuffer.Memory);
                    _buffer = newBuffer;
                }
            }
            Debug.Assert(PayloadBuffer.Length >= size);
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }
    }
}