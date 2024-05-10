using NetCoreEventBus.Infra.EventBus.Bus;
using NetCoreEventBus.Infra.EventBus.RabbitMQ.Extensions;
using NetCoreEventBus.Shared.Events;
using NetCoreEventBus.Web.Shipment.IntegrationEvents.EventHandlers;
using NetCoreEventBus.Web.Shipment.Services;

namespace NetCoreEventBus.Web.Shipment;

public class Startup
{
    public IConfiguration Configuration { get; }
    public IWebHostEnvironment WebHostEnvironment { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        Configuration = configuration;
        WebHostEnvironment = webHostEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        if (WebHostEnvironment.IsDevelopment())
        {
            services.AddSwaggerGen();
        }

        // Must be Scoped or Transient => Cannot consume any scoped service
        services.AddScoped<IShipmentService, ShipmentService>();
        
        // Here we configure the event bus
        ConfigureEventBusDependencies(services);
    }

    public void Configure(IApplicationBuilder app)
    {
        if (WebHostEnvironment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Here we configure event handler subscriptions that the application  has to process
        ConfigureEventBusHandlers(app);
    }

    private void ConfigureEventBusDependencies(IServiceCollection services)
    {
        var rabbitMQSection = Configuration.GetSection("RabbitMQ");
        services.AddRabbitMQEventBus
        (
            connectionUrl: rabbitMQSection["ConnectionUrl"],
            brokerName: "netCoreEventBusBroker",
            queueName: "netCoreEventBusShipmentQueue",
            timeoutBeforeReconnecting: 15
        );

        services.AddTransient<OrderShippingStartedEtoHandler>();
        services.AddTransient<ShipmentStartedEtoHandler>();
    }

    private void ConfigureEventBusHandlers(IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        // Here you add the event handlers for each intergration event.
        eventBus.Subscribe<OrderShippingStartedEto, OrderShippingStartedEtoHandler>();
        eventBus.Subscribe<ShipmentStartedEto, ShipmentStartedEtoHandler>();
    }
}