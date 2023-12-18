﻿using Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogConsumer;

internal static class Program
{
    public static void Main(string[] args)
    {
        var configuration = GetConfiguration();

        var services = new ServiceCollection();

        services.AddSingleton<ILoggerFactory>(sp => LoggerFactory.Create(static builder =>
            builder.SetMinimumLevel(LogLevel.Information).AddConsole()));

        // Add event bus instance
        services.AddEventBus(configuration);

        var sp = services.BuildServiceProvider();

        // Subscribe all event handlers
        sp.UseEventBus();

        // IEventBus _eventBus = sp.GetRequiredService<IEventBus>();

        while (true)
        {
            var result = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(result) && result.ToLower().Equals("q")) break;
        }
    }

    private static IConfiguration GetConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
}