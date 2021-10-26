namespace Dms.Common.Configurations
{
    public static class ConfigurationProvider
    {
        private static TcpConfig _tcpConfig;

        public static TcpConfig GetTcpConfig()
        {
            return _tcpConfig;
        }

        public static void SetTcpConfig(TcpConfig tcpConfig)
        {
            _tcpConfig = tcpConfig;
        }
    }
}