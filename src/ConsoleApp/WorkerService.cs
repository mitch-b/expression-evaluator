using ConsoleApp.Models.Configuration;
using ConsoleApp.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp;

internal class WorkerService(
    ILogger<WorkerService> logger,
    IDemoService demoService, 
    IOptions<ConsoleAppSettings> options) : IHostedService
{
    private readonly ILogger<WorkerService> _logger = logger;
    private readonly IDemoService _demoService = demoService;
    private readonly IOptions<ConsoleAppSettings> _options = options;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker started");
        
        if (_options.Value.EntraEnabled == true)
        {
            var response = await _demoService.MakeGraphApiCall();
            if (!string.IsNullOrWhiteSpace(response))
            {
                Console.WriteLine(response);
            }
        }

        while (true)
        {
            _demoService.WriteWelcomeMessage();
            await Task.Delay(3000, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
