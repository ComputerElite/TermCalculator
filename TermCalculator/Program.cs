using TermCalculator;

Expression e = Parser.ParseExpression("sin(1.575)");
e.EvaluateExpression().DisplayExpression("Final Content");
//e.DisplayExpression("Parsed expression");
Console.WriteLine(Math.Sin(Math.PI/2));