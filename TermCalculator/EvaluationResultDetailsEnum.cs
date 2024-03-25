namespace TermCalculator;

public enum EvaluationResultDetailsEnum
{
    None,
    ClosingParenthesisWithoutOpeningParenthesis,
    OpeningParenthesisWithoutClosingParenthesis,
    TooLittleArgumentsProvidedForFunction,
    OperatorAtEndOfExpression,
    OperationNeedsNumber,
    MaximumDepthReached,
    NoNumericResult,
    NoDefiniteAnswer
}