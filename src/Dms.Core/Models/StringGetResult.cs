using System;

namespace Dms.Core.Models
{
    public class StringGetResult
    {
        public Guid CommandId { get; init; }
        public Memory<byte> Value { get; init; }
    }
}