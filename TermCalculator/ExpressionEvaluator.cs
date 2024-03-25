using System.Security.Cryptography;

namespace TermCalculator;

public class ExpressionEvaluator
{
    public static bool showDebugInfo = true;
    public static int maxDepth = 1000;
    
    /// <summary>
    /// Evaluates a given expression
    /// </summary>
    /// <param name="expression">expression to evaluate</param>
    /// <returns>evaluated expression</returns>
    public static Expression EvaluateExpression(Expression expression)
    {
        if (expression.evaluationResult != EvaluationResult.Evaluating)
        {
            if (expression.evaluationResult != EvaluationResult.NotEvaluated) {
                DisplayDebugInfo("Expression has failed evaluation, returning depth" + expression.depth);
                return expression;
            }
                
            expression.evaluationResult = EvaluationResult.Evaluating;
        }
        if(expression.depth > maxDepth) {
            expression.evaluationResult = EvaluationResult.EvaluationFail;
            expression.evaluationResultDetails.detailsEnum = EvaluationResultDetailsEnum.MaximumDepthReached;
            expression.evaluationResultDetails.extraInfostring = "Maximum depth of " + maxDepth + " reached. Evaluation stopped.";
            return expression;
        }
        DisplayDebugExpression(expression, "Evaluating depth " + expression.depth, new List<int>(), new List<int>());
        expression = EvaluateParentheses(expression);
        
        // Remove all Seperators
        expression = RemoveSeparators(expression);

        // Replaces all constants
        expression = EvaluateFunctions(expression, 0);
        // Evaluates all functions
        expression = EvaluateFunctions(expression, 10000);
        
        expression = EvaluateOperations(expression, new List<ExpressionPartType> {ExpressionPartType.Exponentiation});

        expression = AddMultiplyOperationBetweenAdjacentNumbersAndFunctions(expression);
        
        expression = EvaluateOperations(expression, new List<ExpressionPartType> {ExpressionPartType.Multiply, ExpressionPartType.Divide});
        
        expression = EvaluateOperations(expression, new List<ExpressionPartType> {ExpressionPartType.Add, ExpressionPartType.Subtract});

        if (expression.depth == 0) {
            expression.evaluationResult = EvaluationResult.EvaluatedSuccessfully;
            // At last: Simplify
            expression = Simplify(expression);
        }
        DisplayDebugExpression(expression, "Evaluated at depth " + expression.depth, new List<int>(), new List<int>());
        return expression;
    }

    /// <summary>
    /// Simplifies an expression to the greatest extend the calculator can do by combining different elements.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static Expression Simplify(Expression expression) {
        expression = CommutateExpressionAndMultiplyNumbers(expression);
        expression = SimplifyConstantExponent(expression);
        return expression;
    }

    /// <summary>
    /// Simplifies everything which is raised to the power of 0
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static Expression SimplifyConstantExponent(Expression expression) {
        for(int i = 0; i < expression.Count; i++) {
            if(!expression[i].IsExponentiation) continue;
            if(!expression[i+1].IsNumber) continue;
            if(expression[i+1].number != 0 && expression[i+1].number != 1) continue;
            if(expression[i-1].IsParenthesisClose) continue;
            if(expression[i+1].number == 0) {
                expression[i-1].number = 1;
                expression[i-1].type = ExpressionPartType.Number;
            }
            expression.RemoveAt(i+1);
            expression.RemoveAt(i);
        }
        return expression;
    }

    static Expression AddMultiplyOperationBetweenAdjacentNumbersAndFunctions(Expression expression)
    {
        if (expression.evaluationResult != EvaluationResult.Evaluating) return expression;
        for (int i = 0; i < expression.Count - 1; i++)
        {
            ExpressionPartType t = expression[i].type;
            ExpressionPartType nt = expression[i + 1].type;
            if ((t == ExpressionPartType.Number || t == ExpressionPartType.Function || t == ExpressionPartType.ParenthesisClose) &&
                (nt == ExpressionPartType.Number || nt == ExpressionPartType.Function || t == ExpressionPartType.ParenthesisClose))
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
        if(!prevNumber.IsNumber && !nextNumber.IsNumber) return expression.DecrementI(0);
        if(prevNumber.IsNumber && nextNumber.IsFunction) {
            if(operation == ExpressionPartType.Multiply) {
                if(prevNumber.number == 0) {
                    result = new Expression(0);
                    return Replace(expression, occurrenceIndex == 0 ? 0 : occurrenceIndex - 1, occurrenceIndex + 1, result).DecrementI(occurrenceIndex == 0 ? 0 : 1);
                }
            }
            return expression.DecrementI(0); // maybe remove in future?
        }
        if(prevNumber.IsFunction && nextNumber.IsNumber) {

            if(operation == ExpressionPartType.Multiply) {
                if(nextNumber.number == 0) {
                    result = new Expression(0);
                    return Replace(expression, occurrenceIndex == 0 ? 0 : occurrenceIndex - 1, occurrenceIndex + 1, result).DecrementI(occurrenceIndex == 0 ? 0 : 1);
                }
            }
            return expression.DecrementI(0); // maybe remove in future?
        }
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
            clone.parts.Add(expression.parts[i].Clone());
        }

        return clone;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static Expression Derivative(Expression expression) {
        // Split Expression parts at addition and subtraction
        List<Expression> parts = SplitExpressionBy(expression, ExpressionPartTypes.LineCalculation);
        // ExponentRule
        for(int i = 0; i < parts.Count; i++) {
            if(parts[i].isSplitPoint) continue;
            parts[i] = ExponentRule(parts[i]);
        }
        expression = MergeExpressionList(parts);
        return expression;
    }

    /// <summary>
    /// Merges expressions into an Expression. Expression will be set to evaluating
    /// </summary>
    /// <param name="expressionList">Expressions to merge</param>
    /// <returns></returns>
    public static Expression MergeExpressionList(List<Expression> expressionList) {
        Expression done = new Expression();
        done.SetEvaluating();
        for(int i = 0; i < expressionList.Count; i++) {
            done.Append(expressionList[i]);
        }
        return done;
    }

    /// <summary>
    /// Evaluates the exponent rule one something the style of 5*x^2. There mustn't be additions or subtractions.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static Expression ExponentRule(Expression e) {
        DisplayDebugInfo("Evaluating exponent rule of " + e.HumanReadable());
        // Add ^1 to all x
        bool somethingChanged = false;
        for(int i = 0; i < e.Count; i++) {
            if(e[i].type == ExpressionPartType.Function) {
                if(i + 1 >= e.Count || e[i+1].type != ExpressionPartType.Exponentiation) {
                    // ToDo: check if it's x
                    e.Insert(i+1, ExpressionPart.Number(1));
                    e.Insert(i+1, ExpressionPart.Exponentiation);
                }
            }
            if(e[i].type != ExpressionPartType.Exponentiation) continue;
            if(e[i-1].type != ExpressionPartType.Function) continue; // it doesn't apply to non variables duh
            // Decrement exponent by 1 and multiply by original exponent
            double exponentNumber = e[i+1].number;
            // ToDo: Check if exponent is whole number
            // Decrement exponent
            e[i+1].number -= 1;
            e.Insert(i-1, ExpressionPart.Multiply);
            e.Insert(i-1, ExpressionPart.Number(exponentNumber));
            i += 2;
            somethingChanged = true;
        }
        if(!somethingChanged) return new Expression(0); // If no exponent was changed there's no x and thus the part of the expression gets lost in the derivative
        return e;
    }

    public static Expression CommutateExpressionAndMultiplyNumbers(Expression expression) {
        /*
        // Why did I do this? It prolly had a reason or so so uuuh yes. Someone explain please
        // Otherwise I wouldn't have done the shit cause it's work
        //
        // 1. Rewrite divisions as multiplication
        //      To do this Split the expression at all operators using the SplitExpressionBy method as it'll ignore parentheses
        List<Expression> splitAtOperands = SplitExpressionBy(expression, ExpressionPartTypes.Operands);
        //      Then iterate over the split expression and check if there are any divisions
        for(int i = 0; i < splitAtOperands.Count; i++) {
            if(!splitAtOperands[i].isSplitPoint) continue;
            if(splitAtOperands[i][0].IsDivide) {
                splitAtOperands[i][0].type = ExpressionPartType.Multiply;
                splitAtOperands[i+1].Append(ExpressionPart.ParanthesisClose);
                splitAtOperands[i+1].Insert(0, ExpressionPart.ParanthesisOpen);
                splitAtOperands[i+1].Insert(0, ExpressionPart.Divide);
                splitAtOperands[i+1].Insert(0, ExpressionPart.Number(1));
                // Now evaluate i + 1 to get a number for the multiplication
                splitAtOperands[i+1] = EvaluateExpression(splitAtOperands[i+1]);
                if(splitAtOperands[i+1].Count > 1) {
                    splitAtOperands[i+1].Append(ExpressionPart.ParanthesisClose);
                    splitAtOperands[i+1].Insert(0, ExpressionPart.ParanthesisClose);
                }
            }
        }
        expression = MergeExpressionList(splitAtOperands);
        */
        expression = AddMultiplyOperationBetweenAdjacentNumbersAndFunctions(expression);
        // 2. Split expression at multiplication and then multiply all numbers that can be found if they are just numbers and thus have a length of 1
        int lastNumber = -1;
        for(int i = 0; i < expression.Count; i++) {
            if(expression[i].IsNumber) {
                if(lastNumber != -1) {
                    double number = expression[i].number;
                    DisplayDebugExpression(expression, "Multiplying commutate", new List<int> {lastNumber+1}, new List<int> {lastNumber, i});
                    expression.RemoveAt(i);
                    i--;
                    expression.RemoveAt(i);
                    i--;
                    expression[lastNumber].number = expression[lastNumber].number * number;
                    i = lastNumber;
                }
                lastNumber = i;
                continue;
            }
            if(expression[i].IsParenthesisOpen) {
                // Skip to closing parenthesis
                expression = FindFirstParentheses(expression, 0);
                DisplayDebugExpression(expression, "Skipping paranthesis", new List<int> {i, expression.parenthesesSearchResult.closeIndex}, new List<int>());
                i = expression.parenthesesSearchResult.closeIndex;
                continue;
            }
            if(!expression[i].IsMultiply && !expression[i].IsNumberOrFunction) {
                lastNumber = -1;
            }
        }
        return expression;
    }

    /// <summary>
    /// Splits an Expression at the given ExpressionPartTypes.
    /// E. g. 2*x+8 will give back
    /// 2*x    processThis: true
    /// +      processThis: false
    /// 8      processThis: true
    /// </summary>
    /// <param name="expression">Expression to split (Should not include parentheses)</param>
    /// <param name="types">Types to split the Expression at</param>
    /// <returns>The split expression</returns>
    public static List<Expression> SplitExpressionBy(Expression expression, List<ExpressionPartType> types) {
        // Split Expression parts at addition and subtraction
        // ToDo: Do not split in parentheses
        List<Expression> parts = new List<Expression>();
        Expression current = new Expression();
        int parenthesesCount = 0;
        for(int i = 0; i < expression.Count; i++) {
            if(expression[i].type == ExpressionPartType.ParenthesisOpen) parenthesesCount++;
            if(expression[i].type == ExpressionPartType.ParenthesisClose) parenthesesCount--;
            if(types.Contains(expression[i].type) && parenthesesCount == 0) {
                parts.Add(current.SetSplitPoint(false));
                parts.Add(expression[i].ToExpression().SetSplitPoint(true));
                current = new Expression();
            } else {
                current.Append(expression[i]);
            }
        }
        parts.Add(current.SetSplitPoint(false));
        return parts;
    }

        

    /// <summary>
    /// Multiplies out an expression. Must contain 2 pairs of parentheses
    /// Expected expression input examples:
    /// (4+2)*(x)
    /// (2+2)*(1-7)
    /// (2+6+8)*(9+1-9)
    /// </summary>
    /// <param name="expression"></param>
    public static Expression MultiplyOut(Expression expression)
    {
        // First split into left parentheses and right parentheses
        Expression firstPart = FindFirstParentheses(expression);
        firstPart = GetExpressionParts(expression, firstPart.parenthesesSearchResult.openIndex + 1,
            firstPart.parenthesesSearchResult.closeIndex - 1);
        
        Expression secondPart = FindFirstParentheses(expression, firstPart.parenthesesSearchResult.closeIndex + 1);
        secondPart = GetExpressionParts(expression, secondPart.parenthesesSearchResult.openIndex + 1,
            secondPart.parenthesesSearchResult.closeIndex - 1);
        
        Expression done = new Expression();
        for (int i = 0; i < firstPart.Count; i++)
        {
            if(!firstPart[i].IsNumberOrFunction) i++;
            ExpressionPart iOperator = i == 0 ? ExpressionPart.Add : firstPart[i - 1];
            Expression iOperands = new Expression();
            while (i < firstPart.Count)
            {
                if (firstPart[i].IsAddOrSubtract)
                {
                    i--;
                    break;
                }
                iOperands.Append(firstPart[i]);
                i++;
            }

            iOperands = PrepareOperandsForMultiplication(iOperands);
            for (int j = 0; j < secondPart.Count; j++)
            {
                if(!secondPart[j].IsNumberOrFunction) j++;
                ExpressionPart jOperator = j == 0 ? ExpressionPart.Add : secondPart[j - 1];
                Expression jOperands = new Expression();
                while (j < secondPart.Count)
                {
                    if (secondPart[j].IsAddOrSubtract)
                    {
                        j--;
                        break;
                    }
                    jOperands.Append(secondPart[j]);
                    j++;
                }

                jOperands = PrepareOperandsForMultiplication(jOperands);
                done.Append(GetCorrectOperandForAdditionOrSubtraction(iOperator, jOperator));
                done.Append(iOperands);
                done.Append(ExpressionPart.Multiply);
                done.Append(jOperands);
            }
        }

        return done;
    }

    static Expression PrepareOperandsForMultiplication(Expression operands)
    {
        if(operands.Count == 1) return operands;
        if(operands.Any(x => x.type == ExpressionPartType.Divide))
        {
            // There is a division in the operands, so we must add parentheses
            operands.Insert(0, ExpressionPart.ParanthesisOpen);
            operands.Append(ExpressionPart.ParanthesisClose);
        }

        return operands;
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