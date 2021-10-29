using System.Threading.Tasks;
using Dms.Common.Binary;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core.Commands.String
{
    public class StringSetCommand: ICommand
    {
        private readonly IStorage _storage;

        public StringSetCommand(IStorage storage)
        {
            _storage = storage;
        }
        
        public async ValueTask HandleAsync(BinaryRequestReader input, Session session)
        {
            var key = input.ReadNextString();
            var validUntil = input.ReadNextDateTime();
            var value = input.ReadNextBinary();
            
            await _storage.WriteAsync(key, value);

            var responseWriter = new BinaryResponseWriter();
            
            responseWriter.WriteType(ResponseTypes.Ack);
            responseWriter.WriteGuid(input.Id);

            await session.SendAsync(responseWriter.Buffer);
        }
        
    }

}