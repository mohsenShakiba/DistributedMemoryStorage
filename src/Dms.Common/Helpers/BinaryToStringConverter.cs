using System;
using System.Collections.Generic;
using System.Text;

namespace Dms.Common.Helpers
{
    /// <summary>
    /// Will transform binary to string in an efficient way without creating new strings
    /// </summary>
    public class BinaryToStringConverter
    {
        private static BinaryToStringConverter _shared;
        
        public static BinaryToStringConverter Shared
        {
            get
            {
                if (_shared is not null)
                    return _shared;

                _shared = new BinaryToStringConverter();

                return _shared;
            }
        }

        private readonly Dictionary<int, string> _store = new();

        public string GetStringForBytes(Span<byte> data)
        {
            var hashCode = ComputeHash(data);

            if (_store.TryGetValue(hashCode, out var s)) return s;

            s = Encoding.ASCII.GetString(data);

            _store[hashCode] = s;

            return s;
        }

        private int ComputeHash(Span<byte> data)
        {
            unchecked
            {
                const int p = 16777619;
                var hash = (int) 2166136261;

                for (var i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
    }
}