namespace TermCalculator;

public class Functions
{
    Dictionary<string, Function> functions = new Dictionary<string, Function>
    {
        {
            "sin",
            new Function(1, "sin", Sin)
        },
        {
            "cos",
            new Function(1, "cos", Cos)
        },
        {
            "tan",
            new Function(1, "tan", Tan)
        },
        {
            "pi",
            new Function(0, "pi", PI)
        },
        {
            "e",
            new Function(0, "pi", E)
        },
    };
    
    static Expression E(Expression expression, int functionOccurrenceIndex, Function self)
    {
        return ReplaceExpressionAndArgumentsWithNumberResult(expression, functionOccurrenceIndex, self, Math.E);
    }
    
    static Expression PI(Expression expression, int functionOccurrenceIndex, Function self)
    {
        return ReplaceExpressionAndArgumentsWithNumberResult(expression, functionOccurrenceIndex, self, Math.PI);
    }

    static Expression Sin(Expression expression, int functionOccurrenceIndex, Function self)
    {
        double res = Math.Sin(GetArgument(expression, functionOccurrenceIndex, 0));
        return ReplaceExpressionAndArgumentsWithNumberResult(expression, functionOccurrenceIndex, self, res);
    }
    
    static Expression Cos(Expression expression, int functionOccurrenceIndex, Function self)
    {
        double res = Math.Cos(GetArgument(expression, functionOccurrenceIndex, 0));
        return ReplaceExpressionAndArgumentsWithNumberResult(expression, functionOccurrenceIndex, self, res);
    }
    
    static Expression Tan(Expression expression, int functionOccurrenceIndex, Function self)
    {
        double res = Math.Tan(GetArgument(expression, functionOccurrenceIndex, 0));
        return ReplaceExpressionAndArgumentsWithNumberResult(expression, functionOccurrenceIndex, self, res);
    }
    
    static Expression ReplaceExpressionAndArgumentsWithNumberResult(Expression expression, int functionOccurrenceIndex, Function self, double res)
    {
        return ExpressionEvaluator.Replace(expression, functionOccurrenceIndex,
            functionOccurrenceIndex + self.argumentsNeeded, new Expression(res));
    }

    public static double GetArgument(Expression expression, int functionOccurrenceIndex, int argumentIndex)
    {
        return expression[functionOccurrenceIndex + 1 + argumentIndex].number;
    }
    
    /// <summary>
    /// Checks if a function with the given name and max number of arguments exists
    /// </summary>
    /// <param name="function">function name</param>
    /// <param name="maxArguments">max number of arguments</param>
    /// <returns></returns>
    public bool ContainsFunction(string function, int maxArguments)
    {
        if (!functions.ContainsKey(function)) return false;
        if (functions[function].argumentsNeeded > maxArguments) return false;
        return true;
    }

    /// <summary>
    /// Evaluated a function found at an specific index.
    /// Arguments to the function MUST be right of occurrenceIndex as Type NUMBER
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="occurrenceIndex"></param>
    /// <returns></returns>
    public Expression EvaluateFunction(Expression expression, int occurrenceIndex)
    {
        string functionName = expression[occurrenceIndex].function;
        if (!ContainsFunction(functionName, int.MaxValue)) return expression;
        return functions[functionName].Run(expression, occurrenceIndex);
    }
}