using System;
using System.Text;
using System.Threading.Tasks;
using Dms.Common.Logging;
using Dms.Core.Commands;
using Dms.Core.Types;
using Dms.Tcp;
using Microsoft.Extensions.Logging;

namespace Dms.Core
{
    public class Kernel
    {
        private readonly Server _server;
        private readonly ILogger<Kernel> _logger;
        private readonly CommandRouter _router;
        

        public Kernel()
        {
            _router = new();
            _logger = LogProvider.GetLogger<Kernel>();
            _server = new();
            _server.OnPacketReceived += OnPacketReceivedInternal;
        }


        private async void OnPacketReceivedInternal(Session session, Packet packet)
        {
            var type = packet.Type;
            
            var command = _router.ResolveCommand(type);

            if (command is null)
            {
                _logger.LogError($"Packet type: {Encoding.ASCII.GetString(type.Span)} was not resolved to any command");
                return;
            }
            
            _logger.LogInformation($"Packet is resolved to command: {command}");
            
            try
            {
                await command.HandleAsync(packet);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while trying to handle command: {nameof(command)}, error: {e}");
            }
        }
        
    }
}