using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class CompereExpression : IExpression
{
    public IExpression LeftExpression { get; }
    public IExpression RightExpression { get; }
    public TokenType Operator { get; }

    public CompereExpression(IExpression leftExpression, IExpression rightExpression, TokenType operatorType)
    {
        RightExpression = rightExpression;
        LeftExpression = leftExpression;
        Operator = operatorType;
    }

    public override string ToString() => $"{LeftExpression} {Operator} {RightExpression}";
}