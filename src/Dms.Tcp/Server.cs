using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dms.Common.Configurations;
using Dms.Common.Logging;
using Microsoft.Extensions.Logging;

namespace Dms.Tcp
{
    public class Server: IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellationSource;
        private readonly ILogger<Server> _logger;
        private readonly TcpListener _listener;
        private readonly TcpConfig _config;
        private readonly List<Session> _sessions;

        private Task _acceptTask;

        public event Action<Packet> OnPacketReceived;

        public IEnumerable<Session> Session => _sessions.AsReadOnly();

        public Server()
        {
            _logger = LogProvider.GetLogger<Server>();
            _config = ConfigurationProvider.GetTcpConfig();
            _listener = new (_config.EndPoint);
            _sessions = new();
            _cancellationSource = new CancellationTokenSource();
        }

        public void Start()
        {
            _acceptTask = Task.Run(StartInternalAsync);
        }

        private async ValueTask StartInternalAsync()
        {
            _listener.Start();
            
            while (!_cancellationSource.Token.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    
                    var session = new Session(tcpClient);
                    
                    _sessions.Add(session);
                    
                    _logger.LogInformation($"Accepted connection from end point: {tcpClient.Client.RemoteEndPoint}");

                    session.OnPacketReceived += OnPacketReceivedInternal;
                    session.OnClosed += OnSessionClosedInternal;
                    
                    session.HandlePackets();
                }
                catch(Exception e)
                {
                    _logger.LogError($"Exception while trying to accept session: {e}");
                }
            }
        }

        private void OnPacketReceivedInternal(Session session, Packet packet)
        {
            OnPacketReceived?.Invoke(packet);
        }

        private void OnSessionClosedInternal(Session session)
        {
            if (_sessions.Contains(session))
            {
                _sessions.Remove(session);
                
                // note: no need to called dispose on session, if it's closed
                // then it's already disposed too
            }
        }

        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("Stopping TCP server");
            
            _cancellationSource.Cancel();

            await _acceptTask;
            
            _listener.Stop();

            foreach (var session in _sessions)
            {
                await session.DisposeAsync();
                
                session.OnPacketReceived -= OnPacketReceivedInternal;
                session.OnClosed -= OnSessionClosedInternal;
            }
            
            _sessions.Clear();
            
            _logger.LogInformation("Stopped TCP server");
        }
    }
}