using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebSocketManager;
using WebSocketManager.Common;

namespace Cloudberry.DeepSpaceNetwork;

public class RobotMessageHandler : WebSocketHandler
{
    public RobotMessageHandler(WebSocketConnectionManager webSocketConnectionManager)
        : base(webSocketConnectionManager)
    { }

    public async Task SendHello()
    {
        var message = new Message()
        {
            MessageType = MessageType.Text,
            Data = $"Hello!"
        };

        await SendMessageToAllAsync(message);
    }

    public override async Task OnConnected(WebSocket socket)
    {
        await base.OnConnected(socket);

        var socketId = WebSocketConnectionManager.GetId(socket);

        var message = new Message()
        {
            MessageType = MessageType.Text,
            Data = $"{socketId} is now connected"
        };

        await SendMessageToAllAsync(message);
    }

    // this method can be called from a client, returns the integer result or throws an exception.
    public long DoMath(WebSocket socket, long a, long b)
    {
        if (a == 0 || b == 0) throw new Exception("That makes no sense.");
        return a + b;
    }

    public override async Task OnDisconnected(WebSocket socket)
    {
        var socketId = WebSocketConnectionManager.GetId(socket);

        await base.OnDisconnected(socket);

        var message = new Message()
        {
            MessageType = MessageType.Text,
            Data = $"{socketId} disconnected"
        };
        await SendMessageToAllAsync(message);
    }
}