using System;
using System.Net.WebSockets;

namespace server.models
{
    public class Client
    {
        public Guid UID { get; }
        public WebSocket Socket { get; }
        public string Username { get; set; }

        public Client(WebSocket socket)
        {
            UID = Guid.NewGuid();
            Socket = socket;
            Username = "Guest" + new Random().Next(1000, 10000); // randon 4 digit number
        }
    }
}