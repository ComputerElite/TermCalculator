namespace TermCalculator;
public class ExpressionPartTypes {
    /// <summary>
    /// Multiply and Divide
    /// </summary>
    public static List<ExpressionPartType> PointCalculation = new List<ExpressionPartType> {
        ExpressionPartType.Multiply,
        ExpressionPartType.Divide,
    };
    /// <summary>
    /// Add and Subtract
    /// </summary>
    public static List<ExpressionPartType> LineCalculation = new List<ExpressionPartType> {
        ExpressionPartType.Add,
        ExpressionPartType.Subtract,
    };
    /// <summary>
    /// Add, Subtract, Multiply, Divide
    /// </summary>
    public static List<ExpressionPartType> Operands = new List<ExpressionPartType> {
        ExpressionPartType.Add,
        ExpressionPartType.Subtract,
        ExpressionPartType.Multiply,
        ExpressionPartType.Divide,
    };
}