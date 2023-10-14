namespace TermCalculator;

public class ParenthesesSearchResult
{
    public int openIndex { get; set; } = -1;
    public int closeIndex { get; set; } = -1;
    public bool found { get => openIndex != -1 && closeIndex != -1; }
    
    public ParenthesesSearchResult(int openIndex, int closeIndex)
    {
        this.openIndex = openIndex;
        this.closeIndex = closeIndex;
    }
    
    public ParenthesesSearchResult() { }
}