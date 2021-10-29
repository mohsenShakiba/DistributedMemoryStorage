namespace Dms.Common.Binary
{
    public enum ResponseTypes: short
    {
        Ack = 1,
        Nack = 2,
        Ping = 3,
        
        StringGet = 10
    }
}