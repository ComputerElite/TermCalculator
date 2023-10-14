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
        if (expression.evaluationResult != EvaluationResult.Evaluating)
        {
            if (expression.evaluationResult != EvaluationResult.NotEvaluated)
                return expression;
            expression.evaluationResult = EvaluationResult.Evaluating;
        }
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
        
        if (expression.evaluationResult != EvaluationResult.Evaluating) return expression;
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
        
        if (expression.evaluationResult != EvaluationResult.Evaluating) return expression;
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
        if (expression.evaluationResult != EvaluationResult.Evaluating) return expression;
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

    static Expression FindFirstParentheses(Expression expression, int startAt = 0)
    {
        if (expression.evaluationResult != EvaluationResult.Evaluating) return expression;
        expression.parenthesesSearchResult = new ParenthesesSearchResult();
        int parenthesisCounter = 0;
        for (int i = startAt; i < expression.Count; i++)
        {
            if (expression[i].type == ExpressionPartType.ParenthesisOpen)
            {
                parenthesisCounter++;
                if (expression.parenthesesSearchResult.openIndex == -1) expression.parenthesesSearchResult.openIndex = i;
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
                if (parenthesisCounter == 0 && expression.parenthesesSearchResult.openIndex != -1)
                {
                    // Matching closing parenthesis was found
                    expression.parenthesesSearchResult.closeIndex = i;
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
            expression.evaluationResultDetails.referenceIndices.Add(expression.parenthesesSearchResult.openIndex);
            return expression;
        }

        return expression;
    }

    static Expression EvaluateParentheses(Expression expression)
    {
        if (expression.evaluationResult != EvaluationResult.Evaluating) return expression;
        expression = FindFirstParentheses(expression);
        if (!expression.parenthesesSearchResult.found) return expression;
        // Parenthesis have been found, evaluate expression inside parenthesis
        DisplayDebugExpression(expression, "Evaluating parentheses", new List<int> { expression.parenthesesSearchResult.openIndex, expression.parenthesesSearchResult.closeIndex }, Range(expression.parenthesesSearchResult.openIndex + 1, expression.parenthesesSearchResult.closeIndex - 1));

        Expression EvaluatedParanthesis = EvaluatePartOfExpression(expression, expression.parenthesesSearchResult.openIndex + 1, expression.parenthesesSearchResult.closeIndex - 1);
        if (EvaluatedParanthesis.Count > 1)
        {
            EvaluatedParanthesis.Insert(EvaluatedParanthesis.Count, ExpressionPart.ParanthesisClose);
            EvaluatedParanthesis.Insert(0, ExpressionPart.ParanthesisOpen);
        }
        
        // Evaluate updated expression to evaluate any other parentheses at this depth
        expression = EvaluateExpression(Replace(expression, expression.parenthesesSearchResult.openIndex, expression.parenthesesSearchResult.closeIndex, EvaluatedParanthesis));

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
        if (expression.evaluationResult != EvaluationResult.Evaluating) return expression;
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

    public static void DisplayDebugExpression(Expression expression, string text)
    {
        DisplayDebugExpression(expression, text, new List<int>(), new List<int>());
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

        for (int i = toInsert.Count - 1; i >= 0; i--)
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

    /// <summary>
    /// Multiplies out an expression. Must contain 2 pairs of parentheses
    /// Expected expression input examples:
    /// (2+2)*(1-7)
    /// (2+6+8)*(9+1-9)
    /// </summary>
    /// <param name="expression"></param>
    public static Expression MultiplyOut(Expression expression)
    {
        // First split into left parentheses and right parentheses
        Expression firstPart = FindFirstParentheses(expression);
        Console.WriteLine(firstPart.parenthesesSearchResult.openIndex + "   " + firstPart.parenthesesSearchResult.closeIndex);
        firstPart = GetExpressionParts(expression, firstPart.parenthesesSearchResult.openIndex + 1,
            firstPart.parenthesesSearchResult.closeIndex - 1);
        
        Expression secondPart = FindFirstParentheses(expression, firstPart.parenthesesSearchResult.closeIndex + 1);
        secondPart = GetExpressionParts(expression, secondPart.parenthesesSearchResult.openIndex + 1,
            secondPart.parenthesesSearchResult.closeIndex - 1);
        
        DisplayDebugExpression(firstPart, "First");
        DisplayDebugExpression(secondPart, "Second");
        Expression done = new Expression();
        for (int i = 0; i < firstPart.Count; i++)
        {
            if(!firstPart[i].IsNumberOrFunction) i++;
            ExpressionPart iOperator = i == 0 ? ExpressionPart.Add : firstPart[i - 1];
            ExpressionPart iOperand = firstPart[i];
            for (int j = 0; j < secondPart.Count; j++)
            {
                if(!secondPart[j].IsNumberOrFunction) j++;
                ExpressionPart jOperator = j == 0 ? ExpressionPart.Add : secondPart[j - 1];
                ExpressionPart jOperand = secondPart[j];
                done.InsertEnd(GetCorrectOperandForAdditionOrSubtraction(iOperator, jOperator));
                done.InsertEnd(iOperand);
                done.InsertEnd(ExpressionPart.Multiply);
                done.InsertEnd(jOperand);
            }
        }

        return done;
    }

    static ExpressionPart GetCorrectOperandForAdditionOrSubtraction(ExpressionPart operationA,
        ExpressionPart operationB)
    {
        if(operationA.type == ExpressionPartType.Add && operationB.type == ExpressionPartType.Add) return ExpressionPart.Add;
        if(operationA.type == ExpressionPartType.Add && operationB.type == ExpressionPartType.Subtract) return ExpressionPart.Subtract;
        if(operationA.type == ExpressionPartType.Subtract && operationB.type == ExpressionPartType.Add) return ExpressionPart.Subtract;
        if(operationA.type == ExpressionPartType.Subtract && operationB.type == ExpressionPartType.Subtract) return ExpressionPart.Add;
        return ExpressionPart.Add;
    }
}