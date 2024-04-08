using System.Collections.Generic;
using System.Diagnostics;

namespace TermCalculator.Shell;

public class ExpressionShell {
    public ExpressionShell() {
        commands = new Dictionary<string, ExpressionShellCommand> {
            {"help", new ExpressionShellCommand(Help, "Displays this")},
            {"exit", new ExpressionShellCommand(Exit, "Exits the shell")},
            {"show", new ExpressionShellCommand(Show, "Shows the content of the selected expression")},
            {"showall", new ExpressionShellCommand(ShowAll, "Shows all expressions")},
            {"set", new ExpressionShellCommand(Set, "Sets the content of the selected expression", "[content]")},
            {"eval", new ExpressionShellCommand(Eval, "Evaluates the current expression")},
            {"select", new ExpressionShellCommand(Select, "Selects another expression", "<num>")},
            {"graph", new ExpressionShellCommand(Graph, "Graphs a graph in the given area", "<minx> <maxx> <miny> <maxy>")},
        };
    }

    private List<Expression> expressions = new List<Expression>();
    private int selectedExpression = 0;

    private Dictionary<string, ExpressionShellCommand> commands;

    private void Graph(List<string> args) {
        if(selectedExpression >= expressions.Count) {
            Error("Selected expression does not exist, create it or select a different expression");
            return;
        }
        if(args.Count < 4) {
            Error("Not all arguments were given");
            return;
        }
        int minX;
        int maxX;
        int minY;
        int maxY;
        if(!int.TryParse(args[0], out minX) || !int.TryParse(args[1], out maxX) || !int.TryParse(args[2], out minY) || !int.TryParse(args[3], out maxY)) {
            Error("All arguments must be ints");
            return;
        }
        double deltaX = maxX - minX;
        double deltaY = maxY - minY;
        double ratio = deltaX / deltaY;

        int width;
        int height;
        if(ratio * Console.WindowHeight < Console.WindowWidth) {

            width = (int)Math.Floor(ratio * Console.WindowHeight);
            height = Console.WindowHeight;
        } else {
            height = (int)Math.Floor(ratio / Console.WindowWidth);
            width = Console.WindowWidth;
        }
        Graphing.GraphFunction(expressions[selectedExpression], minX, maxX, minY, maxY, width, height);
    }

    private void Select(List<string> args) {
        if(args.Count <= 0) {
            Error("No id provided");
            return;
        }
        int i;
        if(!int.TryParse(args[0], out i)) {
            Error("No valid id provided");
            return;
        }
        selectedExpression = i;
        Console.WriteLine("Changed to expression #" + i);
    }

    private void ShowAll(List<string> args) {
        for(int i = 0; i < expressions.Count; i++) {
            Console.ForegroundColor = i % 2 == 0 ? ConsoleColor.White : ConsoleColor.Yellow;
            Console.WriteLine("#" + i.ToString().PadRight(5) + expressions[i].HumanReadable());
        }
    }

    private void Eval(List<string> args) {
        if(selectedExpression >= expressions.Count) {
            Error("Selected expression does not exist, create it or select a different expression");
            return;
        }

        Console.WriteLine("Selected expression: ");
        expressions[selectedExpression].PrintHumanReadable();

        foreach(string function in expressions[selectedExpression].GetUnassignedFunctions()) {
            string value = Question("'" + function + "' is unknown. Assign a value (Leave empty for simplification): ");
            // ToDo: Define non constant functions
            if(value == "") continue;
            double d;
            if(!double.TryParse(value, out d)) {
                Error("Could not parse provided value");
                continue;
            }
            expressions[selectedExpression].SetConstant(function, d);
        }

        Expression res = expressions[selectedExpression].EvaluateExpression();
        expressions[selectedExpression] = res;
        if(res.evaluationResult != EvaluationResult.EvaluatedSuccessfully) {
            res.DisplayExpression("Error during evaluation");
            return;
        }
        Console.WriteLine("Result:");
        res.PrintHumanReadable();
    }

    private void Set(List<string> args) {
        while(selectedExpression >= expressions.Count) {
            expressions.Add(new Expression());
        }
        string exp = args.Count == 0 ? Question("Expression content: ") : String.Join(' ', args);
        Expression parsed = Parser.ParseExpression(exp);
        parsed.DisplayExpression("Parsed expression");
        expressions[selectedExpression] = parsed;
    }

    private void Show(List<string> args) {
        if(selectedExpression >= expressions.Count) {
            Error("Selected expression does not exist, create it or select a different expression");
            return;
        }
        Console.WriteLine("Content of expression #" + selectedExpression);
        expressions[selectedExpression].PrintHumanReadable();
    }

    private void Exit(List<string> args) {
        Environment.Exit(0);
    }

    private void Help(List<string> args) {
        int length = commands.Keys.Select(x => x.Length).Max() + 3;
        int usageLength = commands.Values.Select(x => x.usage.Length).Max();
        length += usageLength;
        foreach(KeyValuePair<string, ExpressionShellCommand> e in commands) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write((e.Key + " " + e.Value.usage).PadRight(length));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(e.Value.description);
        }
    }

    private void Error(string msg) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ForegroundColor = ConsoleColor.White;
    }

    /// <summary>
    /// Asks a question and gives back the users response
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private string Question(string msg) {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(msg);
        Console.ForegroundColor = ConsoleColor.White;
        return Console.ReadLine();
    }

    private void PrintShellPrefix() {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\nTermCalculator #" + selectedExpression  + ": ");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public void StartShell() {
        while(true) {
            PrintShellPrefix();
            string input = Console.ReadLine();
            List<string> parts = input.Split(' ').ToList();
            string cmd = parts[0].ToLower();
            parts.RemoveAt(0);
            if(commands.ContainsKey(cmd)) commands[cmd].Invoke(parts);
            else {
                Error("Command not found, type 'help' for a list of available commands");
            }
        }
    }
}