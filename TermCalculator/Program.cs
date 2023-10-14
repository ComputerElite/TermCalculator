using TermCalculator;

Expression e = Parser.ParseExpression("2^(-2)");
e.EvaluateExpression().DisplayExpression("Final Content");
//e.DisplayExpression("Parsed expression");