using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dms.Common.Configurations;
using Dms.Tcp;
using Tcp.Tests.Packets;
using Xunit;

namespace Tcp.Tests;

public class ConnectionTests
{
     
    [Fact]
    public async Task Connection_ClosedByClient_RemovedFromServer()
    {
        var endPoint = IPEndPoint.Parse("0.0.0.0:8000");
            
        ConfigurationProvider.SetTcpConfig(new TcpConfig(endPoint, false, null));

        var server = new Server();
            
        server.Start();

        var client = new TcpClient();
            
        await Task.Delay(100);

        await client.ConnectAsync(endPoint);
            
        await Task.Delay(100);
            
        Assert.True(server.Session.Any());
            
        client.Close();
            
        await Task.Delay(100);
            
        Assert.True(!server.Session.Any());
    }

    [Fact]
    public async Task Connection_ValidPacket_ReceivedByServer()
    {
        var endPoint = IPEndPoint.Parse("0.0.0.0:8001");
            
        ConfigurationProvider.SetTcpConfig(new TcpConfig(endPoint, false, null));

        var server = new Server();
        var packetReceived = false;
        var packetPayloadMatch = false;
        var packet = new SamplePacket(100);

        server.OnPacketReceived += (_, p) =>
        {
            packetReceived = true;
            packetPayloadMatch = Encoding.UTF8.GetString(p.Payload.Memory.Span) == packet.Payload;
        };
            
        server.Start();

        var client = new TcpClient();
            
        await Task.Delay(100);

        await client.ConnectAsync(endPoint);

        var stream = client.GetStream();

        stream.Write(packet.Buffer);

        await Task.Delay(100);
            
        Assert.True(packetReceived);
        Assert.True(packetPayloadMatch);
    }

    [Fact]
    public async Task Connection_InvalidPacket_CloseClient()
    {
        var endPoint = IPEndPoint.Parse("0.0.0.0:8002");
            
        ConfigurationProvider.SetTcpConfig(new TcpConfig(endPoint, false, null));

        var server = new Server();
        var packetReceived = false;

        server.OnPacketReceived += (_, _) =>
        {
            packetReceived = true;
        };
            
        server.Start();

        var client = new TcpClient();
            
        await Task.Delay(100);

        await client.ConnectAsync(endPoint);

        var stream = client.GetStream();

        var packet = new InvalidPacket();
            
        stream.Write(packet.Buffer);

        await Task.Delay(100);
            
        Assert.False(packetReceived);
        Assert.True(!server.Session.Any());
    }

    [Fact]
    public async Task Connection_SendPacketByServer_ReceivedByClient()
    {
        var endPoint = IPEndPoint.Parse("0.0.0.0:8003");
            
        ConfigurationProvider.SetTcpConfig(new TcpConfig(endPoint, false, null));

        var server = new Server();

        server.Start();

        var client = new TcpClient();
            
        await Task.Delay(100);

        await client.ConnectAsync(endPoint);

        var session = new Session(client);
            
        var packetReceived = false;
            
        session.OnPacketReceived += (_, _) =>
        {
            packetReceived = true;
        };
                
        session.HandlePackets();

        await Task.Delay(100);

        var samplePacket = new SamplePacket(100);

        await server.Session.First().SendAsync(samplePacket.Buffer);
            
        await Task.Delay(100);
   
        Assert.True(packetReceived);
    }
}