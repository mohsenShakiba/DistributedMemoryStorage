using System.Threading.Tasks;
using Dms.Tcp;

namespace Dms.Core.Commands
{
    public interface ICommand
    {
        ValueTask HandleAsync(Packet packet);
    }
}