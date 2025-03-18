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
            Console.WriteLine("WebSocket Server started...");

            while (true)
            {
                var context = await _httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    await HandleClientAsync(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }

                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; connect-src 'self' ws://localhost:5000");
            }
        }

        private async Task HandleClientAsync(HttpListenerContext context)
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            var client = new Client(wsContext.WebSocket);
            _clients.Add(client);

            Console.WriteLine($"Client connected! Total clients: {_clients.Count}");

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
                    Console.WriteLine("Client disconnected.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                // Broadcast the message to all clients
                await BroadcastMessageAsync($"{client.UID}: {message}");
            }
        }

        private async Task BroadcastMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            foreach (var client in _clients)
            {
                if (client.Socket.State == WebSocketState.Open)
                {
                    await client.Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}