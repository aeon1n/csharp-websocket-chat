using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using server.models;

namespace server
{
    public class WebSocketServer
    {
        private readonly HttpListener _httpListener = new();
        private readonly List<Client> _clients = new();

        public WebSocketServer(string url)
        {
            _httpListener.Prefixes.Add(url);
        }

        public async Task StartAsync()
        {
            _httpListener.Start();
            Console.WriteLine("Websocket Server started");

            while (true)
            {
                // Waiting for incoming requests ; returns HttpListenerContext that represents a client
                var context = await _httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    _ = Task.Run(() => HandleClientAsync(context));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }

                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; connect-src 'self' ws://127.0.0.1:5000");
            }
        }

        private async Task HandleClientAsync(HttpListenerContext context)
        {
            /* Accept the WebSocket Connection */
            var wsContext = await context.AcceptWebSocketAsync(null);

            /* Add new Client to the List */
            var client = new Client(wsContext.WebSocket);
            _clients.Add(client);
            Console.WriteLine("Client connected! Total Clients {0}", _clients.Count);
            await ReceiveUsernameAsync(client);
            await BroadcastMessageAsync($"{client.Username}: Joined the Room", client);

            /* Wait for messages from client */
            await ReceiveMessagesAsync(client);
        }

        private async Task ReceiveMessagesAsync(Client client)
        {
            /* 1024 byte buffer */
            var buffer = new Byte[1024];

            while (client.Socket.State == WebSocketState.Open)
            {
                var result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                try
                {

                    /* Client want's to disconnect */
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        _clients.Remove(client);
                        Console.WriteLine("Client disconnected");
                        await BroadcastMessageAsync($"{client.UID}: Left the Room", client);
                        break;
                    }

                    /* decode bytes from buffer array into string */
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message == "/show users")
                    {
                        await SendUsernamesAsync(client);
                    }
                    else
                    {
                        await BroadcastMessageAsync($"{client.Username}: {message}", client);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in ReceivedMessageAsync: {ex}");
                }
            }
        }

        private async Task BroadcastMessageAsync(string message, Client sender)
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

            await Task.WhenAll(sendTasks);
        }

        private async Task ReceiveUsernameAsync(Client client)
        {
            var buffer = new Byte[1024];

            while (client.Socket.State == WebSocketState.Open)
            {
                var result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (message.Trim().StartsWith("(USERNAME)", StringComparison.OrdinalIgnoreCase))
                {
                    client.Username = message.Substring(10).Trim();
                    Console.WriteLine($"Client {client.UID} set their username to {client.Username}");
                    return; // Exit after setting the username
                }
            }
        }

        private async Task SendUsernamesAsync(Client client)
        {
            var usernames = string.Join(", ", _clients.Select(c => c.Username));
            var buffer = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, usernames.Split(", ")));

            if (client.Socket.State == WebSocketState.Open)
            {
                await client.Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}