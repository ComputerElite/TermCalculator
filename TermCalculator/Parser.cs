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
            if (t != lastType)
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
        } else if (p.type == ExpressionPartType.FUNCTION)
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
        if (Regex.IsMatch(part.ToString(), @"^[A-Za-z]+$")) return ExpressionPartType.FUNCTION;
        if (part == "=") return ExpressionPartType.EQUAL;
        return ExpressionPartType.Invalid;
    }
}