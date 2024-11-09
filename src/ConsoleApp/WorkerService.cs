using ConsoleApp.Models;
using ConsoleApp.Models.Configuration;
using ConsoleApp.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp;

internal class WorkerService(
    ILogger<WorkerService> logger,
    CustomExpressionService customExpressionService,
    NCalcExpressionService nCalcExpressionService,
    LinqExpressionService linqExpressionService,
    IOptions<ConsoleAppSettings> options) : IHostedService
{
    private readonly ILogger<WorkerService> _logger = logger;
    private readonly CustomExpressionService _customExpressionService = customExpressionService;
    private readonly NCalcExpressionService _nCalcExpressionService = nCalcExpressionService;
    private readonly LinqExpressionService _linqExpressionService = linqExpressionService;
    private readonly IOptions<ConsoleAppSettings> _options = options;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker started");

        var a = new DemoObject { Name = "A", Age = 30, Nested = new NestedDemoObject { Name = "A1", Age = 31 } };
        var b = new DemoObject { Name = "B", Age = 30, Nested = new NestedDemoObject { Name = "B1", Age = 41 } };
        var embeddedObjects = new Dictionary<string, object> { { "a", a }, { "b", b } };
        var trueSimpleExpression = "a.Name == \"A\" && a.Age < 50";
        var falseNestedExpression = "a.Name == \"A\" && (a.Nested.Age > 50 || b.Nested.Age > 45) ";

        Console.WriteLine($"Simple Test: {trueSimpleExpression}");
        Console.WriteLine($"CustomExpression Result: {await RunCustomExpression(trueSimpleExpression, embeddedObjects) == true}");
        Console.WriteLine($"NCalcExpression Result: {await RunNCalcExpression(trueSimpleExpression, embeddedObjects) == true}");
        Console.WriteLine($"LinqExpression Result: {await RunLinqExpression(trueSimpleExpression, embeddedObjects) == true}");

        Console.WriteLine($"Nested Test: {falseNestedExpression}");
        Console.WriteLine($"CustomExpression Result: (throws exception)");
        Console.WriteLine($"NCalcExpression Result: {await RunNCalcExpression(falseNestedExpression, embeddedObjects) == false}");
        Console.WriteLine($"LinqExpression Result: {await RunLinqExpression(falseNestedExpression, embeddedObjects) == false}");

        do
        {
            Console.WriteLine("Enter an expression to evaluate or 'exit' to quit:");
            var userExpression = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userExpression) || string.Equals(userExpression, "exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine($"CustomExpression Result: {await RunCustomExpression(userExpression, embeddedObjects)}");
            Console.WriteLine($"NCalcExpression Result: {await RunNCalcExpression(userExpression, embeddedObjects)}");
            Console.WriteLine($"LinqExpression Result: {await RunLinqExpression(userExpression, embeddedObjects)}");

        } while (!cancellationToken.IsCancellationRequested);
    }

    private async Task<bool?> RunCustomExpression(string expression, IDictionary<string, object> embeddedObjects)
    {
        try
        {
            var result = await _customExpressionService.EvaluateExpression<bool>(expression, embeddedObjects);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate custom expression: {Expression}", expression);
            return null;
        }
    }

    private async Task<bool?> RunNCalcExpression(string expression, IDictionary<string, object> embeddedObjects)
    {
        try
        {
            var nCalcExpression = _nCalcExpressionService.ConvertExpression(expression);
            var result = await _nCalcExpressionService.EvaluateExpression<bool>(nCalcExpression, embeddedObjects);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate NCalc expression: {Expression}", expression);
            return null;
        }
    }

    private async Task<bool?> RunLinqExpression(string expression, IDictionary<string, object> embeddedObjects)
    {
        try
        {
            var result = await _linqExpressionService.EvaluateExpression<bool>(expression, embeddedObjects);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate LINQ expression: {Expression}", expression);
            return null;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
