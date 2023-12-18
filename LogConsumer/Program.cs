﻿using Base.EventBus;
using Base.EventBus.Kafka;
using LogConsumer.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace LogConsumer;

internal static class Program
{
    public static void Main(string[] args)
    {
        var configuration = GetConfiguration();
        
        var services = new ServiceCollection();

        services.AddSingleton<ILoggerFactory>(sp =>
        {
            return LoggerFactory.Create(static builder => builder.SetMinimumLevel(LogLevel.Information).AddConsole());
        });

        // Add our Config object so it can be injected
        services.Configure<KafkaEventBusSettings>(configuration.GetSection("Kafka:EventBus"));
        services.Configure<KafkaConnectionSettings>(configuration.GetSection("Kafka:Connection"));
        services.AddSingleton<IEventBus, EventBusKafka>(sp =>
        {
            // var busSettings = new KafkaEventBusSettings();
            // var conf= sp.GetRequiredService<IConfiguration>();
            // conf.Bind("Kafka:EventBus", busSettings);
            var busSettings = sp.GetRequiredService<IOptions<KafkaEventBusSettings>>();
            var connectionSettings = sp.GetRequiredService<IOptions<KafkaConnectionSettings>>();

            EventBusConfig config = new()
            {
                SubscriberClientAppName = busSettings.Value.ConsumerGroupId, DefaultTopicName = string.Empty, ConnectionRetryCount = busSettings.Value.ConnectionRetryCount, EventNameSuffix = busSettings.Value.EventNameSuffix,
            };

            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            return new EventBusKafka(sp, loggerFactory, config, $"{connectionSettings.Value.HostName}:{connectionSettings.Value.Port}");
        });

        services.AddTransient<OrderStartedIntegrationEventHandler>();
        services.AddTransient<OrderShippingStartedIntegrationEventHandler>();
        services.AddTransient<ShipmentStartedIntegrationEventHandler>();
        services.AddTransient<OrderShippingCompletedIntegrationEventHandler>();

        var sp = services.BuildServiceProvider();

        IEventBus _eventBus = sp.GetRequiredService<IEventBus>();

        _eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
        _eventBus.Subscribe<OrderShippingStartedIntegrationEvent, OrderShippingStartedIntegrationEventHandler>();
        _eventBus.Subscribe<ShipmentStartedIntegrationEvent, ShipmentStartedIntegrationEventHandler>();
        _eventBus.Subscribe<OrderShippingCompletedIntegrationEvent, OrderShippingCompletedIntegrationEventHandler>();
        
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