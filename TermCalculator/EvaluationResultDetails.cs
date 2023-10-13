namespace TermCalculator;

public class EvaluationResultDetails
{
    public EvaluationResultDetailsEnum detailsEnum { get; set; } = EvaluationResultDetailsEnum.None;
    public List<int> referenceIndices { get; set; } = new List<int>();
    public string extraInfostring { get; set; } = "";
}