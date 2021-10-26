using System.Net;

namespace Dms.Common.Configurations
{
    public class TcpConfig
    {
        public IPEndPoint EndPoint { get; }
        public bool EnableAuthentication { get; }
        public ISessionCredentialValidator SessionCredentialValidator { get; }

        public TcpConfig(IPEndPoint endPoint, bool enableAuthentication, ISessionCredentialValidator sessionCredentialValidator)
        {
            EndPoint = endPoint;
            EnableAuthentication = enableAuthentication;
            SessionCredentialValidator = sessionCredentialValidator;
        }
    }
}