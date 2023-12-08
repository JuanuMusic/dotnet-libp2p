using Microsoft.Extensions.Logging;
using Spectre.Console;
namespace Samples.UI;


public class SpectrePanelLogger : ILogger
{
    private readonly string _name;
    Action<string> _onLogMessage;
    //private readonly Func<ColorConsoleLoggerConfiguration> _getCurrentConfig;

    public SpectrePanelLogger(string name, Action<string> onLogMessage) {
        _name = name;
        _onLogMessage = onLogMessage;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if(typeof(TState) == typeof(string))
            _onLogMessage(state!.ToString() ?? "null");
    }
}
