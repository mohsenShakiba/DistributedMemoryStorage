using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core.Commands.String;

public class StringDeleteCommand: ICommand
{
    public async ValueTask HandleAsync(CommandRequestContext ctx)
    {
        var key = ctx.RequestReader.ReadNextString();
        
        if (key is null)
        {
            ctx.WriteNack(ErrorCodes.InvalidInput);
            return;
        }

        await ctx.Storage.DeleteAsync(key);

        ctx.WriteAck();
    }
}