using TermCalculator;

// Fix infinite loop when parentheses evaluation returns parentheses
/*
Expression e = Parser.ParseExpression("2*(x+4)*(x+x)*2");
e.SetConstant("x", 2);
e.EvaluateExpression().DisplayExpression("Final Content");
//e.DisplayExpression("Parsed expression");
*/

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