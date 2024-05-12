using HsnSoft.Base.EventBus.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;

namespace NetCoreEventBus.Shared;

public class DefaultEventBusLogger<T> : IEventBusLogger<T>
{
    private readonly ILogger _logger;

    public DefaultEventBusLogger()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.ClearProviders();

            // Clear Microsoft's default providers (like event logs and others)
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "hh:mm:ss ";
                options.ColorBehavior = LoggerColorBehavior.Default;
            });
        });

        _logger = loggerFactory.CreateLogger(typeof(T).Name);
    }

    public void LogDebug(string? messageTemplate, params object[] args) => _logger.LogDebug(messageTemplate, args);

    public void LogError(string? messageTemplate, params object[] args) => _logger.LogError(messageTemplate, args);

    public void LogWarning(string? messageTemplate, params object[] args) => _logger.LogWarning(messageTemplate, args);

    public void LogInformation(string? messageTemplate, params object[] args) => _logger.LogInformation(messageTemplate, args);

    public void EventBusInfoLog<T>(T? t) where T : IEventBusLog => _logger.Log(LogLevel.Trace, JsonConvert.SerializeObject(t));

    public void EventBusErrorLog<T>(T? t) where T : IEventBusLog => _logger.Log(LogLevel.Critical, JsonConvert.SerializeObject(t));
}