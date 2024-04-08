namespace TermCalculator;

public class Graphing {
    public static void GraphFunctions(List<Expression> expressionsToGraph, double startX, double endX, double startY, double endY, int displayWidth, int displayHeight) {
        Dictionary<int, List<int>> consoleXToConsoleY = new();
        foreach(Expression toGraph in expressionsToGraph) {
            Dictionary<int, List<int>> v = GraphFunction(toGraph, startX, endX, startY, endY, displayWidth, displayHeight);
            // Add all values from v to consoleXToConsoleY
            foreach(KeyValuePair<int, List<int>> kvp in v) {
                if(!consoleXToConsoleY.ContainsKey(kvp.Key)) consoleXToConsoleY.Add(kvp.Key, new List<int>());
                consoleXToConsoleY[kvp.Key].AddRange(kvp.Value);
            }
        }
        DrawGraph(consoleXToConsoleY, displayWidth, displayHeight);
    }

    public static Dictionary<int, List<int>> GraphFunction(Expression toGraph, double startX, double endX, double startY, double endY, int displayWidth, int displayHeight) {
        Dictionary<int, List<int>> consoleXToConsoleY = new();
        double xSpan = endX - startX;
        double xToConsole = displayWidth / xSpan;
        double ySpan = endY - startY;
        double yToConsole = displayHeight / ySpan * 0.5;
        Console.WriteLine(xSpan);
        Console.WriteLine(xToConsole);
        Console.WriteLine(ySpan);
        Console.WriteLine(yToConsole);
        for(double x = startX; Math.Abs(x - endX) > 0.00001; x += xSpan / displayWidth / 10) {
            Console.WriteLine(x);
            Expression res = toGraph.Clone();
            res.SetConstant("x", x);
            res = res.EvaluateExpression();
            double value = res[0].number;
            int xAsConsole = (int)Math.Round((x - startX) * xToConsole);
            int yAsConsole = (int)Math.Round((value - startY) * yToConsole);
            if(!consoleXToConsoleY.ContainsKey(xAsConsole)) consoleXToConsoleY.Add(xAsConsole, new List<int>());
            consoleXToConsoleY[xAsConsole].Add(yAsConsole);
            Console.WriteLine(xAsConsole + " - " + yAsConsole);
        }

        DrawGraph(consoleXToConsoleY, displayWidth, displayHeight);
        return consoleXToConsoleY;
    }

    public static void DrawGraph(Dictionary<int, List<int>> consoleXToConsoleY, int displayWidth, int displayHeight) {
        // Display
        for(int line = displayHeight - 1; line >= 0; line--) {
            for(int column = 0; column < displayWidth; column++) {
                if(consoleXToConsoleY.ContainsKey(column) && consoleXToConsoleY[column].Contains(line)) {
                    Console.Write("X");
                } else {
                    Console.Write(" ");
                }
            }
            Console.WriteLine();
        }
    }
}