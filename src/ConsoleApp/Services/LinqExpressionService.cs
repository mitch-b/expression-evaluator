using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Services;

public class LinqExpressionService(ILogger<LinqExpressionService> logger) : IExpressionService
{
    private readonly ILogger<LinqExpressionService> _logger = logger;

    public async Task<T?> EvaluateExpression<T>(string expression, IDictionary<string, object> embeddedObjects)
    {
        var expressionParameters = new List<ParameterExpression>();
        foreach (var kvp in embeddedObjects)
        {
            expressionParameters.Add(Expression.Parameter(kvp.Value.GetType(), kvp.Key));
        }
        var lambda = DynamicExpressionParser.ParseLambda([.. expressionParameters], typeof(T), expression);
        var result = lambda.Compile().DynamicInvoke(embeddedObjects.Values.ToArray());
        if (result is T typedResult)
        {
            return await Task.FromResult(typedResult);
        }
        else
        {
            _logger.LogError("Failed to cast result to type {Type} - {Value}", typeof(T), result);
            return await Task.FromResult(default(T));
        }
    }
}
