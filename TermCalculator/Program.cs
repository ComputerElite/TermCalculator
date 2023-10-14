using TermCalculator;

// Fix infinite loop when parentheses evaluation returns parentheses

Expression e = Parser.ParseExpression("2(x+4)");
//e.SetConstant("x", 2);
e.EvaluateExpression().DisplayExpression("Final Content");
//e.DisplayExpression("Parsed expression");