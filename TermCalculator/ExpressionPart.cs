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
    public bool IsNumberOrFunction => IsFunction || IsNumber;
    public static ExpressionPart Multiply => new ExpressionPart(ExpressionPartType.Multiply);
    public static ExpressionPart ParanthesisOpen => new ExpressionPart(ExpressionPartType.ParenthesisOpen);
    public static ExpressionPart ParanthesisClose => new ExpressionPart(ExpressionPartType.ParenthesisClose);
    public static ExpressionPart Add => new ExpressionPart(ExpressionPartType.Add);
    public static ExpressionPart Subtract => new ExpressionPart(ExpressionPartType.Subtract);
    
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
    
    public ExpressionPart() {}
        

    /// <summary>
    /// Creates a copy of this ExpressionPart
    /// </summary>
    /// <returns></returns>
    public ExpressionPart Copy()
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
        if (type == ExpressionPartType.Number) return number.ToString();
        if (type == ExpressionPartType.Function || type == ExpressionPartType.Variable) return function;
        return "";
    }

    public override string ToString()
    {
        return GetTypeNamePadded() + GetExtra();
    }
}