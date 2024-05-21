using HsnSoft.Base.Logging;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace NetCoreEventBus.Shared.Serilog;

public sealed class FrameworkLogger : BaseLogger, IPersistentLogger
{
    public FrameworkLogger(IConfiguration configuration) : base(configuration)
    {
    }
    
    public void PersistentInfoLog<T>(T t) where T : IPersistentLog => Write(LogEventLevel.Verbose, t);
    public void PersistentErrorLog<T>(T t) where T : IPersistentLog => Write(LogEventLevel.Fatal, t);

    private void Write<T>(LogEventLevel logLevel, T log) => Logger.Write(logLevel, "{@Log}", log);
}