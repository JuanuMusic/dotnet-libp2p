using Microsoft.Extensions.Logging;
using Terminal.Gui;

public class LoggerFactory : ILoggerFactory
{
    Action<string> _onMessage;
    public LoggerFactory(Action<string> onMessage) => _onMessage = onMessage;
    public void AddProvider(ILoggerProvider provider) {}

    public ILogger CreateLogger(string categoryName)
        => new ChatLogger(_onMessage);

    public void Dispose()
    {
    }
}
