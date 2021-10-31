namespace Dms.Common.Configurations
{
    public static class ConfigurationProvider
    {
        private static TcpConfig _tcpConfig;
        private static StorageConfig _storageConfig;

        public static TcpConfig GetTcpConfig()
        {
            return _tcpConfig;
        }

        public static void SetTcpConfig(TcpConfig tcpConfig)
        {
            _tcpConfig = tcpConfig;
        }

        public static StorageConfig GetStorageConfig()
        {
            return _storageConfig;
        }

        public static void SetStorageConfig(StorageConfig config)
        {
            _storageConfig = config;
        }
    }
}