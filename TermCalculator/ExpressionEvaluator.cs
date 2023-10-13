using System.Security.Cryptography;

namespace TermCalculator;

public class ExpressionEvaluator
{
    public static bool showDebugInfo = true;
    
    /// <summary>
    /// Evaluates a given expression
    /// </summary>
    /// <param name="expression">expression to evaluate</param>
    /// <returns>evaluated expression</returns>
    public static Expression EvaluateExpression(Expression expression)
    {
        DisplayDebugExpression(expression, "Evaluating depth " + expression.depth, new List<int>(), new List<int>());
        
        expression = EvaluateParentheses(expression);
        
        // Remove all SEPARATORS
        expression = RemoveSeparators(expression);

        // Replaces all constants
        expression = EvaluateFunctions(expression, 0);
        // Evaluates all functions
        expression = EvaluateFunctions(expression, 10000);

        if (expression.depth == 0) expression.evaluationResult = EvaluationResult.EvaluatedSuccessfully;
        return expression;
    }

    static Expression RemoveSeparators(Expression expression)
    {
        for (int i = 0; i < expression.Count; i++)
        {
            if (expression[i].type != ExpressionPartType.SEPARATOR) continue;
            expression.RemoveAt(i);
            i--;
        }

        return expression;
    }

    static Expression EvaluateFunctions(Expression expression, int maxArgumentCount)
    {
        for (int i = 0; i < expression.Count; i++)
        {
            if (expression[i].type != ExpressionPartType.FUNCTION) continue;
            string functionName = expression[i].function;
            if (!expression.functions.ContainsFunction(functionName, maxArgumentCount)) continue;
            
            expression = expression.functions.EvaluateFunction(expression, i);
                    
            // Start iteration from beginning as we do not know how many expression parts were removed
            i = -1;
        }

        return expression;
    }

    static Expression EvaluateParentheses(Expression expression)
    {
        DisplayDebugInfo("Searching for parentheses");
        int start = -1;
        int end = -1;
        int parenthesisCounter = 0;
        for (int i = 0; i < expression.Count; i++)
        {
            if (expression[i].type == ExpressionPartType.ParenthesisOpen)
            {
                parenthesisCounter++;
                if (start == -1) start = i;
            }

            if (expression[i].type == ExpressionPartType.ParenthesisClose)
            {
                parenthesisCounter--;
                if (parenthesisCounter < 0)
                {
                    // There was a closing parenthesis without an opening one
                    expression.evaluationResult = EvaluationResult.EvaluationFail;
                    expression.evaluationResultDetails.detailsEnum =
                        EvaluationResultDetailsEnum.ClosingParenthesisWithoutOpeningParenthesis;
                    expression.evaluationResultDetails.referenceIndices.Add(i);
                    return expression;
                }
                if (parenthesisCounter == 0 && start != -1)
                {
                    // Matching closing parenthesis was found
                    end = i;
                    break;
                }
            }
        }

        if (parenthesisCounter > 0)
        {
            // There is a non closed opening parenthesis
            expression.evaluationResult = EvaluationResult.EvaluationFail;
            expression.evaluationResultDetails.detailsEnum =
                EvaluationResultDetailsEnum.OpeningParenthesisWithoutClosingParenthesis;
            expression.evaluationResultDetails.referenceIndices.Add(start);
            return expression;
        }

        if (start != -1 && end != -1)
        {
            // Parenthesis have been found, evaluate expression inside parenthesis
            DisplayDebugExpression(expression, "Evaluating parentheses", new List<int> { start, end }, Range(start + 1, end - 1));
            
            // Evaluate updated expression to evaluate any other parentheses at this depth
            expression = EvaluateExpression(Replace(expression, start, end,
                EvaluatePartOfExpression(expression, start + 1, end - 1)));
        }

        return expression;
    }

    public static List<int> Range(int startIndex, int endIndex)
    {
        List<int> range = new List<int>();
        for (int i = startIndex; i <= endIndex; i++)
        {
            range.Add(i);
        }

        return range;
    }

    public static void DisplayDebugExpression(Expression expression, string text, List<int> operatorIndices, List<int> operandIndices)
    {
        if (!showDebugInfo) return;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(text);
        
        expression.DisplayExpression(operandIndices, operatorIndices);
    }

    static void DisplayDebugInfo(string text, ConsoleColor color = ConsoleColor.Magenta)
    {
        if (!showDebugInfo) return;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static Expression EvaluatePartOfExpression(Expression expression, int startIndex, int endIndex)
    {
        expression.depth++;
        Expression evaluated = EvaluateExpression(GetExpressionParts(expression, startIndex, endIndex));
        evaluated.depth--;
        return evaluated;
    }
    
    public static Expression Replace(Expression expression, int startIndex, int endIndex, Expression toInsert)
    {
        for (int i = 0; i < (endIndex - startIndex + 1); i++)
        {
            expression.RemoveAt(startIndex);
        }

        for (int i = 0; i < toInsert.Count; i++)
        {
            expression.Insert(startIndex, toInsert[i]);
        }

        return expression;
    }

    public static Expression GetExpressionParts(Expression expression, int startIndex, int endIndex)
    {
        Expression clone = expression.Clone();
        clone.parts.Clear();
        for (int i = startIndex; i <= endIndex; i++)
        {
            clone.parts.Add(expression.parts[i].Copy());
        }

        return clone;
    }
}