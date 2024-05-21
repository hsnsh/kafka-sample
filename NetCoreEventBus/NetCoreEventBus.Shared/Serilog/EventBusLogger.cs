using HsnSoft.Base.EventBus.Logging;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace NetCoreEventBus.Shared.Serilog;

public sealed class EventBusLogger : BaseLogger, IEventBusLogger
{
    public EventBusLogger(IConfiguration configuration) : base(configuration)
    {
    }

    public void EventBusInfoLog<T>(T t) where T : IEventBusLog => Write(LogEventLevel.Verbose, t);

    public void EventBusErrorLog<T>(T t) where T : IEventBusLog => Write(LogEventLevel.Fatal, t);

    private void Write<T>(LogEventLevel logLevel, T log) => Logger.Write(logLevel, "{@Log}", log);
}