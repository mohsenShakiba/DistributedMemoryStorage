using System;
using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Common.Configurations;
using Dms.Common.Logging;
using Dms.Core.Commands;
using Dms.Storage;
using Dms.Tcp;
using Microsoft.Extensions.Logging;

namespace Dms.Core;

public class Bootstrap : IAsyncDisposable
{
    private readonly Server _server;
    private readonly ILogger<Bootstrap> _logger;
    private readonly CommandEngine _engine;
    private readonly IStorage _storage;


    public Bootstrap()
    {
        _storage = new FileStorage(ConfigurationProvider.GetStorageConfig());
        _logger = LogProvider.GetLogger<Bootstrap>();
        _server = new();
        _engine = new(_storage, new());
    }

    public async ValueTask StartAsync()
    {
        await _storage.InitializeAsync();
        _server.Start();
        _server.OnPacketReceived += _engine.Execute;
    }


    public async ValueTask DisposeAsync()
    {
        _server.OnPacketReceived -= _engine.Execute;
        await _server.DisposeAsync();
        await _storage.DisposeAsync();
    }
}