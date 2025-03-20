using System;
using System.Net.WebSockets;
using Spectre.Console;

namespace client.models;

public class ChatClient
{
    private readonly string _server;
    private readonly string _username;
    private readonly ClientWebSocket _ws = new();
    private readonly MessageHandler _messageHandler;
    private readonly UIManager _uiManager = new();

    public ChatClient(string server, string username)
    {
        _server = server;
        _username = username;
        _messageHandler = new MessageHandler(_ws, _uiManager);
    }

    public async Task StartAsync()
    {
        try
        {
            await _ws.ConnectAsync(new Uri(_server), CancellationToken.None);
            AnsiConsole.MarkupLine("[green]Connected successfully![/]");
            await _messageHandler.SendMessageAsync("(USERNAME)" + _username);
            await Task.WhenAll(_messageHandler.ReceiveMessagesAsync(), _messageHandler.SendMessagesAsync());
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error connecting to the server: {ex.Message}[/]");
        }
    }
    public async Task CloseWebSocketAsync()
    {
        if (_ws.State == WebSocketState.Open)
        {
            Console.WriteLine("Closing WebSocket...");
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client shutting down", CancellationToken.None);
        }
        _ws.Dispose();
    }
}
