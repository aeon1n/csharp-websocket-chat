using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace client
{
    internal class Program
    {
        private static List<string> messageLog = new List<string>();
        private static object lockObj = new object(); // Prevents concurrency UI issues

        static async Task Main(string[] args)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[bold cyan]Welcome to C#HAT[/]");

            // Ask for server input
            string server = AnsiConsole.Ask<string>("[green]Type the server you want to connect to (e.g., ws://127.0.0.1:5000):[/]");

            if (!Uri.IsWellFormedUriString(server, UriKind.Absolute))
            {
                AnsiConsole.MarkupLine("[red]Invalid server address! Please provide a valid WebSocket URL.[/]");
                return;
            }
            string username = AnsiConsole.Ask<string>("Type your username: ");
            AnsiConsole.MarkupLine($"[yellow]Connecting to {server}...[/]");

            var ws = new ClientWebSocket();

            try
            {
                await ws.ConnectAsync(new Uri(server), CancellationToken.None);
                AnsiConsole.MarkupLine("[green]Connected successfully![/]");

                await SendMessageAsync(ws, "(USERNAME)" + username);

                // Start message handling
                var receiveTask = ReceiveMessagesAsync(ws);
                var sendTask = SendMessagesAsync(ws);

                // Run both tasks until WebSocket closes
                await Task.WhenAll(receiveTask, sendTask);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error connecting to the server: {ex.Message}[/]");
            }
        }

        // Handles receiving messages
        private static async Task ReceiveMessagesAsync(ClientWebSocket ws)
        {
            var buffer = new byte[1024];

            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }

                    // Decode received message
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Thread-safe UI update
                    lock (lockObj)
                    {
                        messageLog.Add($"{message}");
                        UpdateUI();
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error receiving message: {ex.Message}[/]");
                }
            }
        }

        // Handles sending messages
        private static async Task SendMessagesAsync(ClientWebSocket ws)
        {
            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    lock (lockObj)
                    {
                        UpdateUI();
                    }

                    var message = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        await SendMessageAsync(ws, message);

                        lock (lockObj)
                        {
                            messageLog.Add($"[bold yellow]You:[/] {message}");
                            UpdateUI();
                        }
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error in input: {ex.Message}[/]");
                }
            }
        }

        // Sends messages to the WebSocket server
        private static async Task SendMessageAsync(ClientWebSocket ws, string message)
        {
            if (ws.State == WebSocketState.Open)
            {
                var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                try
                {
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error sending message: {ex.Message}[/]");
                }
            }
        }

        // Update UI without 
        private static void UpdateUI()
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[bold cyan]C#HAT - Chat Client[/]");
            AnsiConsole.Write(new Rule("[blue]Messages[/]") { Justification = Justify.Left });

            foreach (var msg in messageLog)
            {
                AnsiConsole.MarkupLine(msg);
            }

            var rule = new Rule("[yellow]Type your message below[/]") { Justification = Justify.Left };
            AnsiConsole.Write(rule);
            Console.Write("You: ");
        }
    }
}
