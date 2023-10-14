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
        
        expression = EvaluateOperations(expression, new List<ExpressionPartType> {ExpressionPartType.Exponentiation});

        expression = AddMultiplyOperationBetweenAdjacentNumbersAndFunctions(expression);
        
        expression = EvaluateOperations(expression, new List<ExpressionPartType> {ExpressionPartType.Multiply, ExpressionPartType.Divide});
        
        expression = EvaluateOperations(expression, new List<ExpressionPartType> {ExpressionPartType.Add, ExpressionPartType.Subtract});

        if (expression.depth == 0) expression.evaluationResult = EvaluationResult.EvaluatedSuccessfully;
        DisplayDebugExpression(expression, "Evaluated at depth " + expression.depth, new List<int>(), new List<int>());
        return expression;
    }

    static Expression AddMultiplyOperationBetweenAdjacentNumbersAndFunctions(Expression expression)
    {
        for (int i = 0; i < expression.Count - 1; i++)
        {
            ExpressionPartType t = expression[i].type;
            ExpressionPartType nt = expression[i + 1].type;
            if ((t == ExpressionPartType.Number || t == ExpressionPartType.Function) &&
                (nt == ExpressionPartType.Number || nt == ExpressionPartType.Function))
            {
                // Adjacent functions/numbers have been found. A multiply operation must be added so it can be evaluated
                expression.Insert(i+1, ExpressionPart.Multiply);
                i++;
            }
        }

        return expression;
    }
    
    
    /// <summary>
    /// Evaluates all operations of specified type left to right
    /// </summary>
    /// <param name="expression">Expression to process</param>
    /// <param name="typesToEvaluate">ExpressionPartTypes to evaluate</param>
    /// <returns></returns>
    static Expression EvaluateOperations(Expression expression, List<ExpressionPartType> typesToEvaluate)
    {
        // Use decrementI variable
        for (int i = 0; i < expression.Count; i++)
        {
            if (typesToEvaluate.Contains(expression[i].type))
            {
                expression = EvaluateOperation(expression, i);
                i -= expression.decrementI;
            }
        }

        return expression;
    }

    static Expression RemoveSeparators(Expression expression)
    {
        for (int i = 0; i < expression.Count; i++)
        {
            if (expression[i].type != ExpressionPartType.Separator) continue;
            expression.RemoveAt(i);
            i--;
        }

        return expression;
    }

    static Expression EvaluateFunctions(Expression expression, int maxArgumentCount)
    {
        for (int i = 0; i < expression.Count; i++)
        {
            if (expression[i].type != ExpressionPartType.Function) continue;
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

            Expression EvaluatedParanthesis = EvaluatePartOfExpression(expression, start + 1, end - 1);
            if (EvaluatedParanthesis.Count > 1)
            {
                EvaluatedParanthesis.Insert(EvaluatedParanthesis.Count, ExpressionPart.ParanthesisClose);
                EvaluatedParanthesis.Insert(0, ExpressionPart.ParanthesisOpen);
            }
            
            // Evaluate updated expression to evaluate any other parentheses at this depth
            expression = EvaluateExpression(Replace(expression, start, end, EvaluatedParanthesis));
        }

        return expression;
    }

    /// <summary>
    /// Evaluates an operation (add, subtract, multiply, divide, exponentiation) on 2 numbers
    /// Does not evaluate if a function is either right or left of the operator
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="occurrenceIndex"></param>
    /// <returns></returns>
    static Expression EvaluateOperation(Expression expression, int occurrenceIndex)
    {
        if (expression.Count == occurrenceIndex + 1)
        {
            // There's an Operator at the end of en expression which is not supported
            expression.evaluationResult = EvaluationResult.EvaluationFail;
            expression.evaluationResultDetails.detailsEnum =
                EvaluationResultDetailsEnum.OperatorAtEndOfExpression;
            expression.evaluationResultDetails.referenceIndices.Add(occurrenceIndex);
            return expression;
        }
        ExpressionPart prevNumber = occurrenceIndex == 0 ? ExpressionPart.Number(0) : expression[occurrenceIndex - 1];
        ExpressionPart nextNumber = expression[occurrenceIndex + 1];
        ExpressionPartType operation = expression[occurrenceIndex].type;
        Expression result;
        // ToDo:
        // - Check if prevNumber and nextNumber are variables and if so return expression as is
        if (!prevNumber.IsNumber)
        {
            // The operation is tried to be evaluated without a valid number
            expression.evaluationResult = EvaluationResult.EvaluationFail;
            expression.evaluationResultDetails.detailsEnum =
                EvaluationResultDetailsEnum.OperationNeedsNumber;
            expression.evaluationResultDetails.extraInfostring = "Operation " + operation + " needs a number before it. It may be possible that evaluation of some part of the expression failed.";
            expression.evaluationResultDetails.referenceIndices.Add(occurrenceIndex - 1);
            return expression;
        }
        if (!nextNumber.IsNumber)
        {
            // The operation is tried to be evaluated without a valid number
            expression.evaluationResult = EvaluationResult.EvaluationFail;
            expression.evaluationResultDetails.detailsEnum =
                EvaluationResultDetailsEnum.OperationNeedsNumber;
            expression.evaluationResultDetails.extraInfostring = "Operation " + operation + " needs a number after it. It may be possible that evaluation of some part of the expression failed.";
            expression.evaluationResultDetails.referenceIndices.Add(occurrenceIndex + 1);
            return expression;
        }
        if (prevNumber.IsNaN || nextNumber.IsNaN)
            result = Expression.NaN;
        else if (operation == ExpressionPartType.Add)
            result = new Expression(prevNumber.number + nextNumber.number);
        else if (operation == ExpressionPartType.Subtract)
            result = new Expression(prevNumber.number - nextNumber.number);
        else if (operation == ExpressionPartType.Multiply)
            result = new Expression(prevNumber.number * nextNumber.number);
        else if (operation == ExpressionPartType.Divide)
        {
            if (nextNumber.number == 0)
                result = Expression.NaN;
            else
                result = new Expression(prevNumber.number / nextNumber.number);
        } else if (operation == ExpressionPartType.Exponentiation)
        {
            if (nextNumber.number < 0 && prevNumber.number == 0)
            {
                // Division by 0 will occur: 0^(-1) = 1/(0^1) = NaN
                result = Expression.NaN;
            }
            else
                result = new Expression(Math.Pow(prevNumber.number, nextNumber.number));
        }
        else
        {
            // Unknown operation, return expression as is
            return expression.DecrementI(0);
        }

        return Replace(expression, occurrenceIndex == 0 ? 0 : occurrenceIndex - 1, occurrenceIndex + 1, result).DecrementI(occurrenceIndex == 0 ? 0 : 1);
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