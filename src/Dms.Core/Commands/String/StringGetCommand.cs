using System;
using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core.Commands.String;

public class StringGetCommand: ICommand
{
        
    public async ValueTask HandleAsync(CommandRequestContext ctx)
    {
        var key = ctx.RequestReader.ReadNextString();

        if (key is null)
        {
            ctx.WriteNack(ErrorCodes.InvalidInput);
            return;
        }

        var data = await ctx.Storage.ReadAsync(key);

        if (data.IsEmpty)
        {
            ctx.WriteNack(ErrorCodes.KeyNotFound);
        }
        else
        {
            ctx.ResponseWriter.WriteType(ResponseTypes.StringGet);
            ctx.ResponseWriter.WriteGuid(ctx.RequestReader.Id);
            ctx.ResponseWriter.WriteMemory(data);
        }
    }
}