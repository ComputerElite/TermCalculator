namespace TermCalculator.Shell;

public class ExpressionShellCommand {
    private Action<List<string>> action;
    public string description;
    public string usage;

    public ExpressionShellCommand(Action<List<string>> action, string description, string usage = "") {
        this.action = action;
        this.description = description;
        this.usage = usage;
    }

    public void Invoke(List<string> arguments) {
        action.Invoke(arguments);
    }
}