namespace TermCalculator;

public class Function
{
    public int argumentsNeeded = 0;
    public string name = "";
    public Func<Expression, int, Function, Expression> function = null;

    /// <summary>
    /// Create new function for use in Functions.
    /// Function will only get executed if enough arguments are given when calling Run().
    /// Otherwise evaluation will result in an evaluation fail
    /// </summary>
    /// <param name="argumentsNeeded">count of needed arguments for the function</param>
    /// <param name="name">name of the function for errors and co</param>
    /// <param name="function">function to update the expression</param>
    public Function(int argumentsNeeded, string name, Func<Expression, int, Function, Expression> function)
    {
        this.argumentsNeeded = argumentsNeeded;
        this.name = name;
        this.function = function;
    }

    public Expression Run(Expression expression, int functionOccurrenceIndex)
    {
        // Check if all arguments needed to run the function are present
        for (int i = 0; i < argumentsNeeded; i++)
        {
            int argumentExpressionIndex = functionOccurrenceIndex + 1 + i;

            if (expression.Count <= argumentExpressionIndex)
            {
                expression.evaluationResult = EvaluationResult.EvaluationFail;
                expression.evaluationResultDetails.detailsEnum =
                    EvaluationResultDetailsEnum.TooLittleArgumentsProvidedForFunction;
                expression.evaluationResultDetails.referenceIndices.Add(functionOccurrenceIndex);
                expression.evaluationResultDetails.extraInfostring = "Function '" + name + "' needs " +
                                                                     argumentsNeeded + " arguments but " + i +
                                                                     " were given";
                return expression;
            }
        }

        List<int> operandsIndices = new List<int>();
        if (argumentsNeeded > 0)
            operandsIndices =
                ExpressionEvaluator.Range(functionOccurrenceIndex + 1, functionOccurrenceIndex + argumentsNeeded);
        ExpressionEvaluator.DisplayDebugExpression(expression, "Evaluating function " + name, new List<int> {functionOccurrenceIndex}, operandsIndices);
        return function(expression, functionOccurrenceIndex, this);
    }
}