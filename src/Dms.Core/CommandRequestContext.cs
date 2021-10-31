using Dms.Common.Binary;
using Dms.Storage;
using Dms.Tcp;

namespace Dms.Core;

public class CommandRequestContext
{
    public IStorage Storage { get; }
    public BinaryRequestReader RequestReader { get; }
    public BinaryResponseWriter ResponseWriter { get; }

    public CommandRequestContext(IStorage storage, BinaryRequestReader requestReader, BinaryResponseWriter responseWriter)
    {
        Storage = storage;
        RequestReader = requestReader;
        ResponseWriter = responseWriter;
    }

    public void WriteNack(string message)
    {
        ResponseWriter.WriteType(ResponseTypes.Nack);
        ResponseWriter.WriteGuid(RequestReader.Id);
        ResponseWriter.WriteString(message);
    }

    public void WriteAck()
    {
        ResponseWriter.WriteType(ResponseTypes.Ack);
        ResponseWriter.WriteGuid(RequestReader.Id);
    }
}