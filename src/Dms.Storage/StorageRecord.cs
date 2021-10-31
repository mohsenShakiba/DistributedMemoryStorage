using System;

namespace Dms.Storage;

public class StorageRecord
{
    public long Offset { get; init; }
    public Memory<byte> Value { get; set; }
}