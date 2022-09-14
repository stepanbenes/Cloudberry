using System;
using System.Linq;
using System.Collections.Generic;
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

    private HashSet<string> connectedIds = new();

    public event Action<string>? OnRobotConnected;
    public event Action<string>? OnRobotDisconnected;

    public async Task SendHello()
    {
        var message = new Message()
        {
            MessageType = MessageType.Text,
            Data = $"Hello!"
        };

        await InvokeClientMethodToAllAsync("rumble", 1, 2, 3);
    }

    public override async Task OnConnected(WebSocket socket)
    {
        await base.OnConnected(socket);

        var socketId = WebSocketConnectionManager.GetId(socket);

        connectedIds.Add(socketId);

        OnRobotConnected?.Invoke(socketId);

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
        await base.OnDisconnected(socket);

        var socketId = WebSocketConnectionManager.GetId(socket);

        connectedIds.Remove(socketId);

        OnRobotDisconnected?.Invoke(socketId);

        var message = new Message()
        {
            MessageType = MessageType.Text,
            Data = $"{socketId} disconnected"
        };
        await SendMessageToAllAsync(message);
    }
}