using System.Text.RegularExpressions;

namespace TermCalculator;

public class Parser
{
    public static Expression ParseExpression(string expression)
    {
        Expression e = new Expression();
        
        // Split expression into individual string parts of different types
        List<string> expressionStringParts = new List<string>();
        string tmp = "";
        ExpressionPartType lastType = ExpressionPartType.Invalid;
        foreach (char c in expression)
        {
            ExpressionPartType t = GetTypeOfExpressionStringPart(c.ToString());
            if (t != lastType || (t == lastType && IsSingle(lastType)))
            {
                expressionStringParts.Add(tmp);
                tmp = "";
            }
            tmp += c;
            lastType = t;
        }
        expressionStringParts.Add(tmp);
        
        // Parse string parts into ExpressionParts
        foreach (string part in expressionStringParts)
        {
            if (part != "" && GetTypeOfExpressionStringPart(part) != ExpressionPartType.Invalid)
            {
                e.parts.Add(ParseExpressionPart(part));
            }
        }

        return e;
    }

    public static bool IsSingle(ExpressionPartType type) {
        switch(type) {
            case ExpressionPartType.Invalid:
                return false;
            case ExpressionPartType.Add:
                return true;
            case ExpressionPartType.Subtract:
                return true;
            case ExpressionPartType.Multiply:
                return true;
            case ExpressionPartType.Divide:
                return true;
            case ExpressionPartType.ParenthesisOpen:
                return true;
            case ExpressionPartType.ParenthesisClose:
                return true;
            case ExpressionPartType.Exponentiation:
                return true;
            case ExpressionPartType.Equal:
                return true;
            case ExpressionPartType.Number:
                return false;
            case ExpressionPartType.Function:
                return false;
            case ExpressionPartType.Variable:
                return false;
            default:
                return false;
        }
    } 

    /// <summary>
    /// Parses an expression part string into an ExpressionPart
    /// </summary>
    /// <param name="partString">part to parse</param>
    /// <returns>parsed ExpressionPart</returns>
    public static ExpressionPart ParseExpressionPart(string partString)
    {
        ExpressionPart p = new ExpressionPart();
        p.type = GetTypeOfExpressionStringPart(partString);
        if (p.type == ExpressionPartType.Number)
        {
            p.number = double.Parse(partString);
        } else if (p.type == ExpressionPartType.Function)
        {
            p.function = partString;
        }
        return p;
    }

    /// <summary>
    /// Gets the most likely ExpressionPartType of a character
    /// </summary>
    /// <param name="part">Type to get type of</param>
    /// <returns>Most likely ExpressionPartType</returns>
    public static ExpressionPartType GetTypeOfExpressionStringPart(string part)
    {
        if (Regex.IsMatch(part.ToString(), @"^[0-9\.]+$")) return ExpressionPartType.Number;
        if (part == "+") return ExpressionPartType.Add;
        if (part == "-") return ExpressionPartType.Subtract;
        if (part == "*") return ExpressionPartType.Multiply;
        if (part == "/") return ExpressionPartType.Divide;
        if (part == "(") return ExpressionPartType.ParenthesisOpen;
        if (part == ")") return ExpressionPartType.ParenthesisClose;
        if (part == "^") return ExpressionPartType.Exponentiation;
        if (Regex.IsMatch(part.ToString(), @"^[A-Za-z]+$")) return ExpressionPartType.Function;
        if (part == "=") return ExpressionPartType.Equal;
        return ExpressionPartType.Invalid;
    }
}