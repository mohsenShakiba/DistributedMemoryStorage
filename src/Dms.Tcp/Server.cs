using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dms.Common.Configurations;
using Dms.Common.Logging;
using Microsoft.Extensions.Logging;

namespace Dms.Tcp;

/// <summary>
/// Server will listen to incoming tcp connections
/// </summary>
public class Server: IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationSource;
    private readonly ILogger<Server> _logger;
    private readonly TcpListener _listener;
    private readonly TcpConfig _config;
    private readonly List<Session> _sessions;

    private Task _acceptTask;

    public event Action<ISession, Packet> OnPacketReceived;

    public IEnumerable<Session> Session => _sessions.AsReadOnly();

    public Server()
    {
        _logger = LogProvider.GetLogger<Server>();
        _config = ConfigurationProvider.GetTcpConfig();
        _listener = new (_config.EndPoint);
        _sessions = new();
        _cancellationSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Will start a task that accepts incoming connections
    /// </summary>
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

                _logger.LogInformation($"Accepted connection from end point: {tcpClient.Client.RemoteEndPoint} now waiting for session authentication");

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
        OnPacketReceived?.Invoke(session, packet);
    }

    private void OnSessionClosedInternal(Session session)
    {
        Debug.Assert(_sessions.Contains(session));
        _sessions.Remove(session);
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