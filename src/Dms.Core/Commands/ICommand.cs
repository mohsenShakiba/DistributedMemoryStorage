using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Tcp;

namespace Dms.Core.Commands
{
    public interface ICommand
    {
        ValueTask HandleAsync(BinaryRequestReader input, Session session);
    }
}