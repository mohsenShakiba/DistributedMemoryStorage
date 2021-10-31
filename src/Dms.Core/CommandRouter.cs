using System;
using System.Collections.Generic;
using Dms.Common.Binary;
using Dms.Core.Commands;
using Dms.Core.Commands.String;
using Dms.Storage;

namespace Dms.Core;

/// <summary>
/// CommandRouter contains an internal map to resolve commands types based on RequestTypes
/// This mapping is manual
/// </summary>
public class CommandRouter
{
    private readonly Dictionary<RequestTypes, ICommand> _mappings = new();

    public CommandRouter()
    {
        // string commands
        _mappings[RequestTypes.StringGet] = new StringGetCommand();
        _mappings[RequestTypes.StringSet] = new StringSetCommand();
        _mappings[RequestTypes.StringDelete] = new StringDeleteCommand();
    }

    public ICommand? ResolveCommand(RequestTypes type)
    {
        if (_mappings.TryGetValue(type, out var command))
        {
            return command;
        }

        return null;
    }
}