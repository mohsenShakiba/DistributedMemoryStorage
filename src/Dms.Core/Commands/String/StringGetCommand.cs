using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Core.Types;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core.Commands.String
{
    public class StringGetCommand: ICommand
    {
        private readonly IStorage _storage;

        public KeyType Key { get; init; }

        public StringGetCommand(IStorage storage)
        {
            _storage = storage;
        }
        
        
        public async ValueTask HandleAsync(BinaryRequestReader input, Session session)
        {
            var key = input.ReadNextString();

            var data = await _storage.ReadAsync(key);

            var responseWriter = new BinaryResponseWriter();
            
            responseWriter.WriteType(ResponseTypes.StringGet);
            responseWriter.WriteGuid(input.Id);
            responseWriter.WriteMemory(data);

            await session.SendAsync(responseWriter.Buffer);
        }
    }
}