using System;
using System.Net.WebSockets;

namespace server.models
{
    public class Client
    {
        public Guid UID { get; }
        public WebSocket Socket { get; }

        public Client(WebSocket socket)
        {
            UID = Guid.NewGuid();
            Socket = socket;
        }
    }
}