using System;

namespace Dms.Core.Models
{
    public class Nack
    {
        public Guid CommandId { get; init; }
        public string Error { get; init; }
    }
}