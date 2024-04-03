using System.Security.Cryptography.X509Certificates;
using TermCalculator;
using TermCalculator.Shell;

// ToDo: Fix infinite loop when parentheses evaluation returns parentheses

ExpressionEvaluator.maxDepth = 20;
ExpressionEvaluator.showDebugInfo = false;

ExpressionShell shell = new ExpressionShell();
shell.StartShell();
return;

Expression e = Parser.ParseExpression("0.3*x^3+4");
e.DisplayExpression();
Console.WriteLine("Expression");
e.PrintHumanReadable();
Expression eOrg = Parser.ParseExpression(e.HumanReadable());
Expression tangent = ExpressionCreator.GetTangent(e, 1);
Console.WriteLine("Tangent");
tangent.PrintHumanReadable();
eOrg.PrintHumanReadable();
Graphing.GraphFunctions(new List<Expression> {eOrg, tangent}, -5, 5, 0, 16, 150, 45);
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