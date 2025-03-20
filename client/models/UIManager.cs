using System;
using System.Collections.Concurrent;
using Spectre.Console;

namespace client.models;

public class UIManager
{
    private readonly ConcurrentQueue<string> _messageLog = new();

    public void AddMessage(string message)
    {
        _messageLog.Enqueue(message);
        UpdateUI();
    }

    public void UpdateUI()
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[bold cyan]C#HAT - Chat Client[/]");
        AnsiConsole.Write(new Rule("[blue]Messages[/]") { Justification = Justify.Left });

        foreach (var msg in _messageLog)
        {
            AnsiConsole.MarkupLine(msg);
        }

        AnsiConsole.Write(new Rule("[yellow]Type your message below[/]") { Justification = Justify.Left });
        Console.Write("You: ");
    }
}
