using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dms.Common.Configurations;
using Dms.Tcp;
using Tcp.Tests.Packets;
using Tcp.Tests.Security;
using Xunit;

namespace Tcp.Tests
{
    public class AuthenticationTests
    {
        [Fact]
        public async Task Connection_ValidCredentials_SessionEstablished()
        {
            var sessionCredentialValidator = new TestCredentialValidator();
            
            var endPoint = IPEndPoint.Parse("0.0.0.0:8100");
            
            ConfigurationProvider.SetTcpConfig(new TcpConfig(endPoint, true, sessionCredentialValidator));
        
            var server = new Server();
        
            server.Start();
        
            var client = new TcpClient();
            
            await Task.Delay(100);
        
            client.Connect(endPoint);

            var stream = client.GetStream();

            var authenticationPacket = new AuthenticationPacket();

            stream.Write(authenticationPacket.Payload);
            
            await Task.Delay(100);
            
            Assert.True(server.Session.First().IsAuthenticated);
        }
        
        [Fact]
        public async Task Connection_InvalidCredentials_SessionClosed()
        {
            var sessionCredentialValidator = new TestCredentialValidator();
            
            var endPoint = IPEndPoint.Parse("0.0.0.0:8101");
            
            ConfigurationProvider.SetTcpConfig(new TcpConfig(endPoint, true, sessionCredentialValidator));
        
            var server = new Server();
        
            server.Start();
        
            var client = new TcpClient();
            
            await Task.Delay(100);
        
            client.Connect(endPoint);

            var stream = client.GetStream();

            var authenticationPacket = new InvalidAuthenticationPacket();

            stream.Write(authenticationPacket.Payload);
            
            await Task.Delay(100);
            
            Assert.False(server.Session.Any());
        }
    }
}