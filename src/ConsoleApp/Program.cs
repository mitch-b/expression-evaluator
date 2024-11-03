using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ConsoleApp;
using ConsoleApp.Models.Configuration;
using ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Serilog;
using OpenTelemetry.Logs;
using BenchmarkDotNet.Running;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
    {
        configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configBuilder.AddUserSecrets<Program>();
        configBuilder.AddEnvironmentVariables();
        configBuilder.AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<WorkerService>();
        services.AddOptions<ConsoleAppSettings>().BindConfiguration(nameof(ConsoleAppSettings));
        services.AddOptions<ConfidentialClientApplicationOptions>().BindConfiguration("EntraConfig");
        services.AddScoped<IClientCredentialService, ClientCredentialService>();
        services.AddScoped<IDemoService, DemoService>();
        services.AddHttpClient("GraphApi", httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://graph.microsoft.com/");
        });

        var otlpEndpoint = hostContext.Configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(ConsoleApp)))
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
            })
            .WithLogging(builder =>
            {
                builder.AddConsoleExporter();
            });
    })
    .UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });

using var host = builder.Build();

var configuration = host.Services.GetRequiredService<IConfiguration>();

if (configuration.GetValue<bool?>("benchmark") == true)
{
    BenchmarkRunner.Run<BenchmarkService>();
}
else
{
    await host.RunAsync();
}
