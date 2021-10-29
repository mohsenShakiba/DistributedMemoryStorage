using System.Threading.Tasks;
using Dms.Core.Types;
using Dms.Tcp;

namespace Dms.Core.Commands.String
{
    public class StringGetCommand: ICommand
    {
        
        public KeyType Key { get; init; }
        
        public ValueTask HandleAsync(Packet packet)
        {
        }
    }
}