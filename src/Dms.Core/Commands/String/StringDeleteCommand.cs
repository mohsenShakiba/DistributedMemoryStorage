using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Core.Types;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core.Commands.String
{
    public class StringDeleteCommand: ICommand
    {
        private readonly IStorage _storage;

        public KeyType Key { get; init; }

        public StringDeleteCommand(IStorage storage)
        {
            _storage = storage;
        }
        
        public async ValueTask HandleAsync(BinaryRequestReader input, Session session)
        {
            var key = input.ReadNextString();

            await _storage.DeleteAsync(key);

            var responseWriter = new BinaryResponseWriter();
            
            responseWriter.WriteType(ResponseTypes.Ack);
            responseWriter.WriteGuid(input.Id);

            await session.SendAsync(responseWriter.Buffer);
        }
    }
}