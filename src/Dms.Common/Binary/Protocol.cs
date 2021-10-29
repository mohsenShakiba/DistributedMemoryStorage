namespace Dms.Common.Binary
{
    /// <summary>
    /// Contains the length for each data type that is used in binary serialization
    /// </summary>
    public class Protocol
    {
        public const int RequestTypeSize = 2;
        public const int ResponseTypeSize = 2;
        public const int GuidSize = 16;
        public const int KeySize = 4;
        public const int DateTimeSize = 8;
    }
}