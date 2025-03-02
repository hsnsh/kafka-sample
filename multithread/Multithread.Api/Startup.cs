using HsnSoft.Base.AspNetCore;
using Multithread.Api.Application;
using Multithread.Api.EntityFrameworkCore;

namespace Multithread.Api;

public sealed class Startup
{
    private IConfiguration Configuration { get; }
    private IWebHostEnvironment WebHostEnvironment { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        WebHostEnvironment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddBaseAspNetCoreContextCollection();

        services.AddControllers();

        AddContentServiceInfrastructures(services);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
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
            if (!WebHostEnvironment.IsProduction())
            {
                endpoints.MapDefaultControllerRoute();
            }
            else
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", () => $"Test Service | {WebHostEnvironment.EnvironmentName} | v1.0.0");
            }
        });

        hostApplicationLifetime.ApplicationStopping.Register(OnShutdown);
    }


    private void AddContentServiceInfrastructures(IServiceCollection services)
    {
        // Must be Scoped or Transient => Cannot consume any scoped service
        services.AddScoped<ISampleAppService, SampleAppService>();

        services.AddEfCoreDatabaseConfiguration(Configuration);
        //   services.AddMongoDatabaseConfiguration( Configuration);
    }

    private void OnShutdown()
    {
        Console.WriteLine("Application stopping...");
    }
}