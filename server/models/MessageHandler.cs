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
        /* Get and set username if send */
        await ReceiveUsernameAsync(client);

        /* Show connected clients, that a new user joined */
        await _broadcaster.BroadcastMessageAsync($"{client.Username} joined the room", client);

        /* Wait for messages */
        await ReceiveMessagesAsync(client);
    }

    private async Task ReceiveMessagesAsync(Client client)
    {
        var buffer = new byte[1024];


        while (client.Socket.State == WebSocketState.Open)
        {
            /* Receive Data from websocket connection asynchronusly */
            #region ArraySegment
            /* ArraySegment<byte> ermöglicht es, nur einen bestimmten Teil des Puffers zu verwenden, 
            ohne ein neues Array erstellen oder kopieren zu müssen. Die Speicherverwaltung wird effizienter 
            und unnötige Allokationen werden vermieden. */
            #endregion

            var result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            /* Client sends a `close` message and wants to disconnect */
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                _clients.Remove(client);
                Console.WriteLine("Client disconnected");
                await _broadcaster.BroadcastMessageAsync($"{client.Username} left the room", client);
                break;
            }

            /* decode sequence of bytes into (utf8) string */
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            /* user want`s the userlist otherwise just broadcast the message to other clients*/
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

        /* check if message contains a username */
        if (message.StartsWith("(USERNAME)"))
        {
            client.Username = message.Substring(10).Trim();
            Console.WriteLine($"Client {client.UID} set their username to {client.Username}");
        }
    }

    private async Task SendUsernamesAsync(Client client)
    {
        /* make list of online users, dont send own username */
        var usernames = string.Join(", ", _clients.Where(c => !string.IsNullOrWhiteSpace(c.Username) && client.Username != c.Username).Select(c => c.Username));
        if (string.IsNullOrEmpty(usernames)) usernames = "No users online.";

        var buffer = Encoding.UTF8.GetBytes(usernames);

        if (client.Socket.State == WebSocketState.Open)
        {
            await client.Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
