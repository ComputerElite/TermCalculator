using System.Security.Cryptography.X509Certificates;
using TermCalculator;

// ToDo: Fix infinite loop when parentheses evaluation returns parentheses

ExpressionEvaluator.maxDepth = 20;
ExpressionEvaluator.showDebugInfo = false;
Expression e = Parser.ParseExpression("x^2+1");
e.DisplayExpression();
Console.WriteLine("Expression");
e.PrintHumanReadable();
Expression eOrg = Parser.ParseExpression(e.HumanReadable());
Expression tangent = ExpressionCreator.GetTangent(e, 1);
Console.WriteLine("Tangent");
Console.WriteLine(tangent.evaluationResultDetails.extraInfostring);
tangent.PrintHumanReadable();
eOrg.PrintHumanReadable();
//Graphing.GraphFunctions(new List<Expression> {eOrg, tangent}, -4, 4, 0, 16, 50, 20);
//Graphing.DrawGraph(Graphing.GraphFunction(ExpressionEvaluator.Derivative(e), -2, 2, -10, 10, 50, 30), 50, 30);

/*
Expression e = Parser.ParseExpression("2*(x+4)*(x+x)*2");
e.SetConstant("x", 2);
e.EvaluateExpression().DisplayExpression("Final Content");
//e.DisplayExpression("Parsed expression");

/*
Expression e = Parser.ParseExpression("(1/x-6)*(8+7-8*x)").SetEvaluating();
e.SetConstant("x", 2);
Console.WriteLine("Input");
e.PrintHumanReadable();
Console.WriteLine("\nOutput");
Expression o = ExpressionEvaluator.MultiplyOut(e);
o.SetConstant("x", 2);
Expression oe = o.EvaluateExpression();
Expression ee = e.EvaluateExpression();
oe.DisplayExpression();
ee.DisplayExpression();
*/