using BenchmarkDotNet.Attributes;
using ConsoleApp.Models.Configuration;
using ConsoleApp.Services;
using Microsoft.Extensions.Options;

namespace ConsoleApp;

public class BenchmarkService(
    IDemoService demoService, 
    IOptions<ConsoleAppSettings> options)
{
    private readonly IDemoService _demoService = demoService;
    private readonly IOptions<ConsoleAppSettings> _options = options;

    [GlobalSetup]
    public void Setup()
    {
        // initialize things in class if needed
    }

    [Benchmark]
    public void WriteWelcomeMessage()
    {
        for (var i = 0; i < 100; i++)
        {
            _demoService.WriteWelcomeMessage();
        }
    }
}
