using System.Reflection;
using System.Text.RegularExpressions;
using ConsoleApp.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCalc;

namespace ConsoleApp.Services;

public class NCalcExpressionService(ILogger<NCalcExpressionService> logger,
        IOptions<ConsoleAppSettings> consoleAppSettings) : IExpressionService
{
    private readonly ILogger<NCalcExpressionService> _logger = logger;
    private readonly IOptions<ConsoleAppSettings> _consoleAppSettings = consoleAppSettings;

    public async Task<T?> EvaluateExpression<T>(string expression, IDictionary<string, object> embeddedObjects)
    {
        var flattenedObjects = FlattenObject(embeddedObjects);
        var parameters = ExtractParameters(expression);

        var expr = new Expression(expression);

        foreach (var param in parameters)
        {
            if (flattenedObjects.TryGetValue(param, out var value))
            {
                expr.Parameters[param] = value;
            }
            else
            {
                _logger.LogWarning($"Parameter '{param}' was not found in the provided objects.");
            }
        }
        var result = (T)expr.Evaluate();
        return result;
    }

    private static IEnumerable<string> ExtractParameters(string expression)
    {
        var regex = new Regex(@"\b(\w+(\.\w+)*)\b");
        var matches = regex.Matches(expression);
        var parameters = new HashSet<string>();

        foreach (Match match in matches)
        {
            parameters.Add(match.Value);
        }

        return parameters;
    }

    private static IDictionary<string, object> FlattenObject(object obj, string prefix = "")
    {
        var dict = new Dictionary<string, object>();
        if (obj is IDictionary<string, object> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                if (kvp.Value != null && !kvp.Value.GetType().IsPrimitive && !(kvp.Value is string))
                {
                    foreach (var nestedKvp in FlattenObject(kvp.Value, key))
                    {
                        dict[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else
                {
                    dict[key] = kvp.Value;
                }
            }
        }
        else
        {
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(obj);
                var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

                if (value != null && !value.GetType().IsPrimitive && !(value is string))
                {
                    foreach (var nestedKvp in FlattenObject(value, key))
                    {
                        dict[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else
                {
                    dict[key] = value;
                }
            }
        }
        return dict;
    }

    public string ConvertExpression(string expression)
    {
        var regex = new Regex(@"\b(\w+(\.\w+)+)\b");
        var convertedExpression =  regex.Replace(expression, "[$1]").Replace("\"", "'");
        _logger.LogInformation("Converted {expression} to {convertedExpression}", expression, convertedExpression);
        return convertedExpression;
    }
}
