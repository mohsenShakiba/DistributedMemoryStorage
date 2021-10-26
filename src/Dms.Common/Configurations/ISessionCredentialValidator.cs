using System.Threading.Tasks;

namespace Dms.Common.Configurations
{
    /// <summary>
    /// Interface that is implemented by the client to validate credential, the type of authentication doesn't matter
    /// and any type of authentication can be used
    /// </summary>
    public interface ISessionCredentialValidator
    {
        /// <summary>
        /// Called if authentication is enabled and the session has provided the security credentials
        /// </summary>
        /// <param name="credential">Credentials provided by the session</param>
        /// <returns>True if the credential is valid, otherwise false</returns>
        public ValueTask<bool> ValidateCredential(string credential);
    }
}