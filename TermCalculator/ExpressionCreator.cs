
namespace TermCalculator;

public class ExpressionCreator {
    public static Expression GetTangent(Expression e, double x) {
        // Get m
        Expression tangent = Parser.ParseExpression("m*x-m*u+y");
        
        Expression derivative = ExpressionEvaluator.Derivative(e.Clone()).EvaluateExpression();
        derivative.SetConstant("x", x);
        //derivative.DisplayExpression();
        Expression d = derivative.EvaluateExpression();
        d.DisplayExpression();
        d.ExtractNumericalAnswer();
        if(d.returnedNumeric == null) return d.CloneEvaluationResultIntoEmptyExpression();
        double m = d.returnedNumeric.Value;

        e.SetConstant("x", x);
        Console.WriteLine("for y");
        e.PrintHumanReadable();
        e = e.EvaluateExpression();
        e.ExtractNumericalAnswer();
        if(e.returnedNumeric == null) return e.CloneEvaluationResultIntoEmptyExpression();
        double y = e.returnedNumeric.Value;
        tangent.SetConstant("m", m);
        tangent.SetConstant("y", y);
        tangent.SetConstant("u", x);
        return tangent.EvaluateExpression();
    }

    
}