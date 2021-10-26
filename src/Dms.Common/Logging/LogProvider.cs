using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dms.Common.Logging
{
    public static class LogProvider
    {

        private static ILoggerFactory _loggerFactory;
        
        static LogProvider()
        {
            _loggerFactory = new NullLoggerFactory();
        }

        public static void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        
        public static ILogger<T> GetLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }
    }
}