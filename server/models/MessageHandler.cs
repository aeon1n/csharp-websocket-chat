using System;
using System.Net.WebSockets;
using System.Text;

namespace server.models;

public class MessageHandler
{
    private readonly List<Client> _clients;
    private readonly Broadcaster _broadcaster;

    public MessageHandler(List<Client> clients)
    {
        _clients = clients;
        _broadcaster = new Broadcaster(_clients);
    }

    public async Task HandleClientAsync(Client client)
    {
        await ReceiveUsernameAsync(client);
        await _broadcaster.BroadcastMessageAsync($"{client.Username} joined the room", client);
        await ReceiveMessagesAsync(client);
    }

    private async Task ReceiveMessagesAsync(Client client)
    {
        var buffer = new byte[1024];
        while (client.Socket.State == WebSocketState.Open)
        {
            var result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                _clients.Remove(client);
                Console.WriteLine("Client disconnected");
                await _broadcaster.BroadcastMessageAsync($"{client.Username} left the room", client);
                break;
            }
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            if (message == "/show users")
            {
                await SendUsernamesAsync(client);
            }
            else
            {
                await _broadcaster.BroadcastMessageAsync($"{client.Username}: {message}", client);
            }
        }
    }

    private async Task ReceiveUsernameAsync(Client client)
    {
        var buffer = new byte[1024];
        var result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        if (message.StartsWith("(USERNAME)"))
        {
            client.Username = message.Substring(10).Trim();
            Console.WriteLine($"Client {client.UID} set their username to {client.Username}");
        }
    }

    private async Task SendUsernamesAsync(Client client)
    {
        var usernames = string.Join(", ", _clients.Where(c => !string.IsNullOrWhiteSpace(c.Username)).Select(c => c.Username));
        if (string.IsNullOrEmpty(usernames)) usernames = "No users online.";
        var buffer = Encoding.UTF8.GetBytes(usernames);
        if (client.Socket.State == WebSocketState.Open)
        {
            await client.Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
