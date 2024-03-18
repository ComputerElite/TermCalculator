using System.Security.Cryptography.X509Certificates;
using TermCalculator;

// ToDo: Fix infinite loop when parentheses evaluation returns parentheses

ExpressionEvaluator.maxDepth = 20;
ExpressionEvaluator.showDebugInfo = true;
Expression e = Parser.ParseExpression("2(x+2)5+5x*9");
e.SetEvaluating();
e = ExpressionEvaluator.CommutateExpressionAndMultiplyNumbers(e);
e.DisplayExpression();
e.PrintHumanReadable();
/*
Expression e1 = ExpressionEvaluator.Derivative(e).EvaluateExpression();
e1.PrintHumanReadable();
Expression e2 = ExpressionEvaluator.Derivative(e1).EvaluateExpression();
e2.PrintHumanReadable();
*/
//Graphing.DrawGraph(Graphing.GraphFunction(ExpressionEvaluator.Derivative(e), -2, 2, -10, 10, 50, 30), 50, 30);

/*
Expression e = Parser.ParseExpression("1/2+3/4");
ExpressionEvaluator.CommutateExpressionAndMultiplyNumbers(e).PrintHumanReadable();
*/
/*
return;
Expression s = Parser.ParseExpression("1/2*a*t^2");
ExpressionEvaluator.Derivative(s).EvaluateExpression().DisplayExpression();

return;
Expression e1 = Parser.ParseExpression("2*x^(-2)");
Expression e2 = Parser.ParseExpression("((-4)/27)*x+2/3");
e2.SetConstant("x", 0);
e2.PrintHumanReadable(true);
Parser.ParseExpression("((-4)/27)*0+2/3").EvaluateExpression().PrintHumanReadable();
return;
string expression = "x^4";
Expression e = Parser.ParseExpression(expression);
double pX = 2;
//e.EvaluateExpression().DisplayExpression();
//return;
Expression d =ExpressionEvaluator.Derivative(e.EvaluateExpression());
e = Parser.ParseExpression(expression);
e.SetConstant("x", pX);
d.SetConstant("x", pX);
//return;
double pY = e.EvaluateExpression()[0].number;
double m = d.EvaluateExpression()[0].number;
Console.WriteLine(pY);
Console.WriteLine(m);
Expression t = Parser.ParseExpression("m*(x-px)+py");
t.SetConstant("m", m);
t.SetConstant("px", pX);
t.SetConstant("py", pY);
e = Parser.ParseExpression(expression);
Graphing.GraphFunctions(new List<Expression> {e, t}, pX - 1, pX + 1, pY - 1, pY + 1, 70, 50);
t.EvaluateExpression().PrintHumanReadable(true);

//ExpressionEvaluator.Derivative(e1.EvaluateExpression()).PrintHumanReadable();
//Graphing.GraphFunctions(new List<Expression> {e1, e2}, -3, 3, 0, 3, 70, 50);

//Expression e = Parser.ParseExpression("x^7+x^9");
//ExpressionEvaluator.Derivative(e).PrintHumanReadable();
//e.SetConstant("x", 2);
//e.EvaluateExpression().DisplayExpression("Final Content");
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