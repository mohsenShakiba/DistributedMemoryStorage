using System;
using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Common.Logging;
using Dms.Storage;
using Dms.Tcp;
using Microsoft.Extensions.Logging;

namespace Dms.Core;

public class CommandEngine
{
    private readonly IStorage _storage;
    private readonly CommandRouter _router;
    private readonly ILogger<CommandEngine> _logger;

    public CommandEngine(IStorage storage, CommandRouter router)
    {
        _storage = storage;
        _logger = LogProvider.GetLogger<CommandEngine>();
        _router = router;
    }

    public async void Execute(ISession session, Packet packet)
    {
        await ExecuteAsync(session, packet);
    }

    public async ValueTask ExecuteAsync(ISession session, Packet packet)
    {
        using var requestReader = new BinaryRequestReader(packet.Payload);

        if (requestReader.IsMalformed)
        {
            _logger.LogError($"Request could not be parsed");

            return;
        }

        var command = _router.ResolveCommand((RequestTypes)requestReader.Type);

        if (command is null)
        {
            _logger.LogError($"Request type: {requestReader.Type.ToString()} was not resolved to any command");

            await SendNackWithMessageAsync(session, requestReader, ErrorCodes.InvalidRequestType);

            return;
        }

        try
        {
            var responseWriter = new BinaryResponseWriter();

            var ctx = new CommandRequestContext(_storage, requestReader, responseWriter);

            await command.HandleAsync(ctx);

            await session.SendAsync(responseWriter.Buffer);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error while trying to handle command: {nameof(command)}, error: {e}");

            await SendNackWithMessageAsync(session, requestReader, ErrorCodes.InternalError);
        }
    }

    private async Task SendNackWithMessageAsync(ISession session, BinaryRequestReader requestReader, string message)
    {
        using var responseWriter = new BinaryResponseWriter();

        responseWriter.WriteType(ResponseTypes.Nack);
        responseWriter.WriteGuid(requestReader.Id);
        responseWriter.WriteString(ErrorCodes.InternalError);

        await session.SendAsync(responseWriter.Buffer);
    }
}