# Dynamic String Expression Evaluation

There's more with [NCalc](https://github.com/ncalc/ncalc) and DynamicParameters and someone wrapped more around NCalc that might be useful ([PanoramicData.NCalcExtensions](https://github.com/panoramicdata/PanoramicData.NCalcExtensions)). 

There are some annoyances with the default NCalc Parameters that you can't pass more than you use - and you must provide the deepest referenced object used in the expression. Perhaps with DynamicParameters this can be improved.

The goal was to create a simple console app that can evaluate a string expression with a dynamic object, and compare some possible solutions.

```csharp
var a = new DemoObject { Name = "A", Age = 30, Nested = new NestedDemoObject { Name = "A1", Age = 31 } };
var b = new DemoObject { Name = "B", Age = 30, Nested = new NestedDemoObject { Name = "B1", Age = 41 } };

var trueSimpleExpression = "a.Name == \"A\" && a.Age < 50";
var falseNestedExpression = "a.Name == \"A\" && (a.Nested.Age > 50 || b.Nested.Age > 50) "; // because nested ages are smaller

// pass in the objects with data, and the name they're referenced by in the string expression
var embeddedObjects = new Dictionary<string, object> { { "a", a }, { "b", b } };

var customResult = await _customExpressionService.EvaluateExpression<bool>(
    expression, 
    embeddedObjects);
var nCalcResult = await _nCalcExpressionService.EvaluateExpression<bool>(
    expression, 
    embeddedObjects);
```