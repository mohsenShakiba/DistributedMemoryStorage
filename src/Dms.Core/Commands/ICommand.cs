using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core.Commands;

public interface ICommand
{
    ValueTask HandleAsync(CommandRequestContext ctx);
}