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
        private readonly MessageHandler _messageHandler;

        public WebSocketServer(string url)
        {
            _httpListener.Prefixes.Add(url);
            _messageHandler = new MessageHandler(_clients);
        }

        public async Task StartAsync()
        {
            _httpListener.Start();
            Console.WriteLine("WebSocket Server started");

            while (true)
            {
                /* Wait for incoming requests (asynchrom) ; send req to handleclient method */
                var context = await _httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    _ = Task.Run(() => HandleClientAsync(context));
                }
                else
                {
                    /* Status 400: Bad Request */
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task HandleClientAsync(HttpListenerContext context)
        {
            /* Accept WebSocket Connection async */
            var wsContext = await context.AcceptWebSocketAsync(null);

            /* Create new Client ant pass websocket instance to it */
            var client = new Client(wsContext.WebSocket);
            _clients.Add(client);

            Console.WriteLine("Client connected! Total Clients: " + _clients.Count);

            /* Send client to message handler */
            await _messageHandler.HandleClientAsync(client);
        }
    }
}