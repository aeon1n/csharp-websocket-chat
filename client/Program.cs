using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using client.models;
using Spectre.Console;

namespace client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            UIManager ui = new UIManager();

            Console.Clear();
            AnsiConsole.MarkupLine("[bold cyan]Welcome to C#HAT[/]");

            string server = AnsiConsole.Ask<string>("[green]Type the server you want to connect to (e.g., ws://127.0.0.1:5000):[/]");
            if (!Uri.IsWellFormedUriString(server, UriKind.Absolute))
            {
                AnsiConsole.MarkupLine("[red]Invalid server address! Please provide a valid WebSocket URL.[/]");
                return;
            }

            string username = AnsiConsole.Ask<string>("Type your username: ");
            AnsiConsole.MarkupLine($"[yellow]Connecting to {server}...[/]");
            ui.UpdateUI();

            var client = new ChatClient(server, username);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true; // Prevent the process from terminating immediately
                Task.Run(async () =>
                {
                    await client.CloseWebSocketAsync();
                    Environment.Exit(0);
                });
            };

            await client.StartAsync();
        }
    }
}
