using System;

namespace Dms.Storage
{
    public class BinaryValue
    {
        public long Offset { get; init; }
        public Memory<byte> Value { get; set; }
        public bool Deleted { get; private set; }

        public void MarkAsDeleted()
        {
            Deleted = true;
        }
    }
}