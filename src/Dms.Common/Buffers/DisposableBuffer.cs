using System;
using System.Buffers;

namespace Dms.Common.Buffers
{
    /// <summary>
    /// Represents a block of memory that can be disposed, this object makes it easier to
    /// implement reusable memory pattern without introducing too many try/finally blocks
    /// </summary>
    /// <remarks>This object uses ArrayPool internally to reuse memory blocks</remarks>
    public class DisposableBuffer : IDisposable
    {
        private readonly byte[] _buff;
        public Memory<byte> Memory => _buff.AsMemory(0, Length);
        public int Length { get; }

        private bool _disposed;

        public DisposableBuffer(int buffLength)
        {
            _buff = ArrayPool<byte>.Shared.Rent(buffLength);
            Length = buffLength;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DisposableBuffer));
            }

            _disposed = true;
            ArrayPool<byte>.Shared.Return(_buff);
        }
    }
}