using Microsoft.Extensions.Logging;
using Terminal.Gui;

public class ChatLogger : ILogger
{   
    
    Action<string> _onLog;
    public Action<string> OnLog { get => _onLog; }

    public ChatLogger(Action<string> onLog) {
        _onLog = onLog;
    }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if(OnLog != null)
            OnLog(state.ToString());
    }
}