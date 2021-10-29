using System;
using System.Collections.Generic;
using Dms.Common.Helpers;
using Dms.Core.Commands.String;

namespace Dms.Core.Commands
{
    public class CommandRouter
    {
        private readonly Dictionary<string, ICommand> _mappings = new();

        public CommandRouter()
        {
            // string commands
            _mappings["STRGET"] = new StringGetCommand();
            _mappings["STRSET"] = new StringSetCommand();
            _mappings["STRDEL"] = new StringDeleteCommand();
        }

        public ICommand ResolveCommand(Memory<byte> packetType)
        {
            
            var packetTypeStr = BinaryToStringConverter.Shared.GetStringForBytes(packetType.Span);
            return _mappings[packetTypeStr];
        }
    }
}