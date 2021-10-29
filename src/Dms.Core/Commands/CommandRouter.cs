using System;
using System.Collections.Generic;
using Dms.Common.Binary;
using Dms.Core.Commands.String;
using Dms.Storage;

namespace Dms.Core.Commands
{
    public class CommandRouter
    {
        private readonly Dictionary<RequestTypes, ICommand> _mappings = new();

        public CommandRouter(IStorage storage)
        {
            // string commands
            _mappings[RequestTypes.StringGet] = new StringGetCommand(storage);
            _mappings[RequestTypes.StringSet] = new StringSetCommand(storage);
            _mappings[RequestTypes.StringDelete] = new StringDeleteCommand(storage);
        }

        public ICommand ResolveCommand(RequestTypes type)
        {
            return _mappings[type];
        }
    }
}