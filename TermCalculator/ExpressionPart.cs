namespace TermCalculator;

public class ExpressionPart
{
    /// <summary>
    /// Type of the Expression Part
    /// </summary>
    public ExpressionPartType type { get; set; } = ExpressionPartType.Invalid;
    /// <summary>
    /// If type is number, numerical value of the ExpressionPart
    /// </summary>
    public double number { get; set; } = double.NaN;
    /// <summary>
    /// If type is function or variable, the name of the function/variable
    /// </summary>
    public string function { get; set; } = "";
    
    public bool IsNaN => double.IsNaN(number);
    public bool IsNumber => type == ExpressionPartType.Number;
    public bool IsFunction => type == ExpressionPartType.Function;
    public bool IsAdd => type == ExpressionPartType.Add;
    public bool IsSubtract => type == ExpressionPartType.Subtract;
    public bool IsMultiply => type == ExpressionPartType.Multiply;
    public bool IsDivide => type == ExpressionPartType.Divide;
    public bool IsParenthesisOpen => type == ExpressionPartType.ParenthesisOpen;
    public bool IsParenthesisClose => type == ExpressionPartType.ParenthesisClose;
    public bool IsExponentiation => type == ExpressionPartType.Exponentiation;
    public bool IsAddOrSubtract => IsAdd || IsSubtract;
    public bool IsNumberOrFunction => IsFunction || IsNumber;
    public static ExpressionPart Multiply => new ExpressionPart(ExpressionPartType.Multiply);
    public static ExpressionPart Divide => new ExpressionPart(ExpressionPartType.Divide);
    public static ExpressionPart ParanthesisOpen => new ExpressionPart(ExpressionPartType.ParenthesisOpen);
    public static ExpressionPart ParanthesisClose => new ExpressionPart(ExpressionPartType.ParenthesisClose);
    public static ExpressionPart Add => new ExpressionPart(ExpressionPartType.Add);
    public static ExpressionPart Subtract => new ExpressionPart(ExpressionPartType.Subtract);
    public static ExpressionPart Exponentiation => new ExpressionPart(ExpressionPartType.Exponentiation);
    
    public ExpressionPart(ExpressionPartType type)
    {
        this.type = type;
    }

    public static ExpressionPart Number(double value)
    {
        ExpressionPart part = new ExpressionPart(ExpressionPartType.Number);
        part.number = value;
        return part;
    }

    public static ExpressionPart Function(string function)
    {
        ExpressionPart part = new ExpressionPart(ExpressionPartType.Function);
        part.function = function;
        return part;
    }
    
    public ExpressionPart() {}

    public Expression ToExpression() {
        return new Expression(this);
    }
        

    /// <summary>
    /// Creates a copy of this ExpressionPart
    /// </summary>
    /// <returns></returns>
    public ExpressionPart Clone()
    {
        ExpressionPart n = new ExpressionPart();
        n.type = this.type;
        n.number = this.number;
        n.function = this.function;
        return n;
    }

    public string GetTypeNamePadded()
    {
        return Enum.GetName(typeof(ExpressionPartType), type).ToUpper().PadRight(18);
    }

    public string GetExtra()
    {
        if (type == ExpressionPartType.Number) return Math.Round(number, 15).ToString(); // Btw you idiot if you want more precise shit just remove the round duh, but it will calculate with the precise number
        if (type == ExpressionPartType.Function || type == ExpressionPartType.Variable) return function;
        return "";
    }

    public override string ToString()
    {
        return GetTypeNamePadded() + GetExtra();
    }

    public string HumanReadable(Expression parentExpression, bool expandFunction)
    {
        switch (type)
        {
            case ExpressionPartType.Add:
                return "+";
            case ExpressionPartType.Subtract:
                return "-";
            case ExpressionPartType.Multiply:
                return "*";
            case ExpressionPartType.Divide:
                return "/";
            case ExpressionPartType.ParenthesisOpen:
                return "(";
            case ExpressionPartType.ParenthesisClose:
                return ")";
            case ExpressionPartType.Equal:
                return "=";
            case ExpressionPartType.Exponentiation:
                return "^";
            case ExpressionPartType.Function:
                if(expandFunction) {
                    double v =parentExpression.GetFunctionValueIfPresent(function);
                    if(double.IsNaN(v)) return function;
                    else return v.ToString();
                } else return function;
            case ExpressionPartType.Number:
                return number.ToString();
            case ExpressionPartType.Separator:
                return ",";
            case ExpressionPartType.Variable:
                return function;
        }
        return "";
    }
}