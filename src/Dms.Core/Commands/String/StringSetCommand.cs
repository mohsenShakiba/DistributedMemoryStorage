using System.Threading.Tasks;
using Dms.Core.Types;
using Dms.Tcp;

namespace Dms.Core.Commands.String
{
    public class StringSetCommand: ICommand
    {
        public ValueTask HandleAsync(Packet packet)
        {
            var key = "";
            var validUntil = "";
            var value = "";
            
            packet.Payload.Memory
        }
    }
}