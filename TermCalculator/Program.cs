using TermCalculator;

Expression e = Parser.ParseExpression("2x");
e.SetContant("x", 2);
e.EvaluateExpression().DisplayExpression("Final Content");
//e.DisplayExpression("Parsed expression");