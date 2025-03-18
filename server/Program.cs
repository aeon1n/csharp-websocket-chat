using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using server.models;

namespace server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new WebSocketServer("http://localhost:5000/");
            await server.StartAsync();
        }
    }
}
