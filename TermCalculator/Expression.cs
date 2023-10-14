namespace TermCalculator;

public class Expression
{
    public List<ExpressionPart> parts { get; set; } = new List<ExpressionPart>();
    public int depth { get; set; } = 0;
    public EvaluationResult evaluationResult = EvaluationResult.NotEvaluated;
    public EvaluationResultDetails evaluationResultDetails = new EvaluationResultDetails();

    public int decrementI = 0;
    public ParenthesesSearchResult parenthesesSearchResult = new ParenthesesSearchResult();
    
    public static Expression NaN => new Expression(double.NaN);
    
    /// <summary>
    /// Functions stores variable values and functions which can be executed by the evaluator
    /// </summary>
    public Functions functions = new Functions();

    /// <summary>
    /// Tells the calling function to decrement it's counter by the specified amount
    /// </summary>
    /// <returns></returns>
    public Expression DecrementI(int amount)
    {
        decrementI = amount;
        return this;
    }

    /// <summary>
    /// Sets up the expression so the evaluator will replace name with the value
    /// </summary>
    /// <param name="name">Name of the constant</param>
    /// <param name="value">Value of the constant</param>
    public void SetConstant(string name, double value)
    {
        functions.AddConstantToFunctions(name, value);
    }
    
    /// <summary>
    /// Creates an Expression with the number as only part
    /// For use in ExpressionEvaluator.Replace or similar
    /// </summary>
    /// <param name="number">content of the expression</param>
    public Expression(double number)
    {
        parts.Add(ExpressionPart.Number(number));
    }
    
    public Expression() {}
    
    /// <summary>
    /// Evaluated this expression
    /// </summary>
    /// <returns>evaluated expression</returns>
    public Expression EvaluateExpression()
    {
        return ExpressionEvaluator.EvaluateExpression(this);
    }

    /// <summary>
    /// Creates a copy of the object
    /// </summary>
    /// <returns>a copy of the object</returns>
    public Expression Clone()
    {
        Expression n = new Expression();
        n.parts = new List<ExpressionPart>(this.parts);
        n.depth = this.depth;
        n.evaluationResult = this.evaluationResult;
        n.evaluationResultDetails = this.evaluationResultDetails;
        n.functions = this.functions;
        n.decrementI = this.decrementI;
        n.parenthesesSearchResult = this.parenthesesSearchResult;
        return n;
    }

    public int Count => parts.Count;

    public ExpressionPart this[int index]
    {
        get => parts[index];
        set => parts[index] = value;
    }

    /// <summary>
    /// Removes an expression part from the desired index
    /// </summary>
    /// <param name="index">index to remove from</param>
    public void RemoveAt(int index)
    {
        parts.RemoveAt(index);
    }

    /// <summary>
    /// Inserts an expression part at the desired index
    /// </summary>
    /// <param name="index">index where to insert the item</param>
    /// <param name="item">item to insert</param>
    public void Insert(int index, ExpressionPart item)
    {
        parts.Insert(index, item);
    }
    
    /// <summary>
    /// Inserts an expression part at the end of an expression
    /// </summary>
    /// <param name="item">item to insert</param>
    public void Append(ExpressionPart item)
    {
        parts.Insert(this.Count, item);
    }
    
    public bool Any(Func<ExpressionPart, bool> predicate)
    {
        return parts.Any(predicate);
    }
    
    public void Append(Expression expression)
    {
        for (int i = 0; i < expression.Count; i++)
        {
            Append(expression[i]);
        }
    }

    public void DisplayExpression(string text = "Expression contents")
    {
        DisplayExpression(new List<int>(), new List<int>(), new List<int>(), text);
    }
    
    public void DisplayExpression(List<int> highlightBlue, string text = "Expression contents")
    {
        DisplayExpression(highlightBlue, new List<int>(), new List<int>(), text);
    }
    
    public void DisplayExpression(List<int> highlightBlue, List<int> highlightRed, string text = "Expression contents")
    {
        DisplayExpression(highlightBlue, highlightRed, new List<int>(), text);
    }

    public void DisplayExpression(List<int> highlightBlue, List<int> highlightRed, List<int> highlightYellow, string text = "Expression contents")
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine(text);
        Console.WriteLine();
        for (int i = 0; i < parts.Count; i++)
        {
            if (highlightBlue.Contains(i)) Console.ForegroundColor = ConsoleColor.Cyan;
            else if (highlightRed.Contains(i)) Console.ForegroundColor = ConsoleColor.Red;
            else if (highlightYellow.Contains(i)) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(parts[i]);
        }

        if (evaluationResult == EvaluationResult.EvaluationFail)
        {
            // On evaluation fail display reason
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("EVALUATION FAILED!");
            Console.WriteLine(Enum.GetName(typeof(EvaluationResultDetailsEnum), evaluationResultDetails.detailsEnum));
            if (evaluationResultDetails.referenceIndices.Count > 0)
            {
                // If reference indices are available show where the evaluator thinks the issue is
                for (int i = 0; i < parts.Count; i++)
                {
                    if (evaluationResultDetails.referenceIndices.Contains(i))
                        Console.ForegroundColor = ConsoleColor.Red;
                    else Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(parts[i]);
                }
            }
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
    
    public override string ToString()
    {
        string s = "";
        foreach (ExpressionPart p in parts)
        {
            s += p + "\n";
        }

        return s;
    }

    public Expression SetEvaluating()
    {
        evaluationResult = EvaluationResult.Evaluating;
        return this;
    }

    public void PrintHumanReadable()
    {
        Console.WriteLine(HumanReadable());
    }

    public string HumanReadable()
    {
        string s = "";
        for (int i = 0; i < this.Count; i++)
        {
            s += this[i].HumanReadable();
        }

        return s;
    }
}