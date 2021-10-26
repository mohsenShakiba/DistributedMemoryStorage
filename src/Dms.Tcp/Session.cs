using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dms.Common.Configurations;
using Dms.Common.Logging;
using Microsoft.Extensions.Logging;

namespace Dms.Tcp
{
    public class Session: IAsyncDisposable
    {
        private readonly Guid _id;
        private readonly TcpClient _client;
        private readonly ILogger<Session> _logger;
        private readonly CancellationTokenSource _cancellationSource;
        
        private bool _isAuthenticated = false;

        public event Action<Session, Packet> OnPacketReceived;
        public event Action<Session> OnClosed;

        public bool IsAuthenticated => _isAuthenticated;
        
        public Session(TcpClient client)
        {
            _id = Guid.NewGuid();
            _client = client;
            _logger = LogProvider.GetLogger<Session>();
            _cancellationSource = new();
        }
        
        public ValueTask DisposeAsync()
        {
            _logger.LogInformation($"Stopping session with id: {_id}");
            
            _cancellationSource.Cancel();
            
            _client.Dispose();
            
            return ValueTask.CompletedTask;
        }

        public void HandlePackets()
        {
            Task.Run(async () =>
            {
                try
                {
                    await HandlerPacketsInternalAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError($"Exception while handling packets in session: {_id}, exception: {e}");
                    await CloseAsync();
                }
            });
        }

        public ValueTask SendAsync(byte[] payload)
        {
            if (_cancellationSource.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(Session));
            }
            
            var stream = _client.GetStream();
            return stream.WriteAsync(payload);
        }

        private async ValueTask HandlerPacketsInternalAsync()
        {
            await HandleAuthenticationInternalAsync();

            while (!_cancellationSource.IsCancellationRequested)
            {
                Debug.Assert(_isAuthenticated);

                var stream = _client.GetStream();
                
                var packet = await Packet.ReadFromStreamAsync(stream, _cancellationSource.Token);

                if (packet.IsMalformed)
                {
                    _logger.LogInformation($"Malformed packet was received from session with id: {_id}");
                    await CloseAsync();
                    return;
                }
                
                _logger.LogInformation($"Received payload with size {packet.Payload.Length} from session: {_id}");
                
                OnPacketReceived?.Invoke(this, packet);
            }
        }

        private async ValueTask HandleAuthenticationInternalAsync()
        {
            var config = ConfigurationProvider.GetTcpConfig();
            
            // if authentication isn't required we ignore the authentication phase
            if (!config.EnableAuthentication)
            {
                _isAuthenticated = true;
                return;
            }
            
            var stream = _client.GetStream();
                
            using var packet = await Packet.ReadFromStreamAsync(stream, _cancellationSource.Token);

            if (packet.IsMalformed)
            {
                _logger.LogWarning($"Malformed credential was provided for session with id: {_id}");
                await CloseAsync();
                return;
            }

            var credentials = Encoding.UTF8.GetString(packet.Payload.Memory.Span);

            var validator = config.SessionCredentialValidator;

            var credentialIsValid = await validator.ValidateCredential(credentials);

            if (!credentialIsValid)
            {
                _logger.LogWarning($"Invalid credential was provided for session with id: {_id}");
                await CloseAsync();
                return;
            }

            _logger.LogInformation($"Authentication completed for session with id: {_id}");
            
            _isAuthenticated = true;
        }

        private async ValueTask CloseAsync()
        {
            await DisposeAsync();
            
            OnClosed?.Invoke(this);
        }
    }
}