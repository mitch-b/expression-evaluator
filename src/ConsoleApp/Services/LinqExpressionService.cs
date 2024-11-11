using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Services;

public class ExpressionException : Exception
{
    public ExpressionException(string message) : base(message) { }
}

public class LinqExpressionService(ILogger<LinqExpressionService> logger) : IExpressionService
{
    private readonly ILogger<LinqExpressionService> _logger = logger;

    public async Task<T?> EvaluateExpression<T>(string expression, IDictionary<string, object> embeddedObjects)
    {
        _logger.LogInformation("Evaluating expression: {Expression} with embedded objects: {EmbeddedObjects}", expression, embeddedObjects.Keys);

        var expressionParameters = embeddedObjects
            .Select(kvp => Expression.Parameter(kvp.Value.GetType(), kvp.Key))
            .ToList();
    
        try
        {
            var lambda = DynamicExpressionParser.ParseLambda(expressionParameters.ToArray(), typeof(T), expression);
            _logger.LogDebug("Parsed expression into lambda: {Lambda}", lambda);
    
            var result = lambda.Compile().DynamicInvoke(embeddedObjects.Values.ToArray());
            _logger.LogDebug("Invoked lambda and got result: {Result}", result);
    
            if (result is not T typedResult)
            {
                _logger.LogError("Failed to cast result to type {Type} - {Value}", typeof(T), result);
                var errorMessage = $"Failed to cast result to type {typeof(T)} - {result}";
                throw new ExpressionException(errorMessage);
            }
    
            return await Task.FromResult(typedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating expression: {Expression}", expression);
            throw;
        }
    }
}
