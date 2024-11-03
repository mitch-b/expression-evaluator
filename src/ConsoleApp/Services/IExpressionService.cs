
namespace ConsoleApp.Services;

public interface IExpressionService
{
    Task<T> EvaluateExpression<T>(string expression, IDictionary<string, object> embeddedObjects);
}
