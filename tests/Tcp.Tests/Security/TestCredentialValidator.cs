using System.Threading.Tasks;
using Dms.Common.Configurations;
using Tcp.Tests.Packets;

namespace Tcp.Tests.Security;

public class TestCredentialValidator: ISessionCredentialValidator
{
    public ValueTask<bool> ValidateCredential(string credential)
    {
        return ValueTask.FromResult(credential == AuthenticationPacket.SecurityKey);
    }
}