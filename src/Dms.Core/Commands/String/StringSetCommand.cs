using System;
using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core.Commands.String;

public class StringSetCommand: ICommand
{
    public async ValueTask HandleAsync(CommandRequestContext ctx)
    {
        var key = ctx.RequestReader.ReadNextString();
        var validUntil = ctx.RequestReader.ReadNextDateTime();
        var value = ctx.RequestReader.ReadNextBinary();
        
        if (key is null)
        {
            ctx.WriteNack(ErrorCodes.InvalidInput);
            return;
        }
        
        if (validUntil is null)
        {
            ctx.WriteNack(ErrorCodes.InvalidInput);
            return;
        }
        
        if (value.IsEmpty)
        {
            ctx.WriteNack(ErrorCodes.InvalidInput);
            return;
        }
        
        await ctx.Storage.WriteAsync(key, value);

        ctx.WriteAck();        
    }
        
}