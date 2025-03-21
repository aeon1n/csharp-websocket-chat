using System;
using System.Net.WebSockets;
using System.Text;
using Spectre.Console;

namespace client.models;

public class MessageHandler
{
    private readonly ClientWebSocket _ws;
    private readonly UIManager _uiManager;

    public MessageHandler(ClientWebSocket ws, UIManager uiManager)
    {
        _ws = ws;
        _uiManager = uiManager;
    }

    public async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024];
        while (_ws.State == WebSocketState.Open)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                _uiManager.AddMessage(message);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error receiving message: {ex.Message}[/]");
            }
        }
    }
    public async Task SendMessagesAsync()
    {
        while (_ws.State == WebSocketState.Open)
        {
            /* read message from console and add to messagelist */
            var message = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(message))
            {
                await SendMessageAsync(message);
                _uiManager.AddMessage($"[bold yellow]You:[/] {message}");
            }
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_ws.State == WebSocketState.Open)
        {
            var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            try
            {
                await _ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error sending message: {ex.Message}[/]");
            }
        }
    }
}
