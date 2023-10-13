namespace TermCalculator;

public enum ExpressionPartType
{
    /// <summary>
    /// spaces and co
    /// </summary>
    Invalid = -1,
    /// <summary>
    /// Floating point numbers matching [0-9\.]+
    /// </summary>
    Number = 0,
    /// <summary>
    /// Addition
    /// </summary>
    Add = 1,
    /// <summary>
    /// Subtraction
    /// </summary>
    Subtract = 2,
    /// <summary>
    /// Multiplication
    /// </summary>
    Multiply = 3,
    /// <summary>
    /// Division
    /// </summary>
    Divide = 4,
    /// <summary>
    /// Opening parenthesis
    /// </summary>
    ParenthesisOpen = 5,
    /// <summary>
    /// Closing parenthesis
    /// </summary>
    ParenthesisClose = 6,
    /// <summary>
    /// Exponentiation operation
    /// </summary>
    Exponentiation = 7,
    /// <summary>
    /// Anything that involves letters or words. Matches [A-Za-z]
    /// </summary>
    FUNCTION = 8,
    /// <summary>
    /// Equal sign
    /// </summary>
    EQUAL = 9,
    /// <summary>
    /// Variable, functions will change to variable when evaluating expressions. After parsing they'll stay function
    /// </summary>
    VARIABLE = 10,
    /// <summary>
    /// Separator for function arguments
    /// ','
    /// </summary>
    SEPARATOR = 11
}