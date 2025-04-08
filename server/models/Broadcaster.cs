using System;
using System.Net.WebSockets;
using System.Text;

namespace server.models;

public class Broadcaster
{
    private readonly List<Client> _clients;

    public Broadcaster(List<Client> clients)
    {
        _clients = clients;
    }

    public async Task BroadcastMessageAsync(string message, Client sender)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var sendTasks = new List<Task>();

        foreach (var client in _clients)
        {
            if (client.Socket.State == WebSocketState.Open && client != sender)
            {
                sendTasks.Add(client.Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
            }
        }

        /* wait for all sending tasks to complete */
        await Task.WhenAll(sendTasks);
    }
}
