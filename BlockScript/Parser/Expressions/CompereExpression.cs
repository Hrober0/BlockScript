using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public record CompereExpression(IExpression LeftExpression, IExpression RightExpression, TokenType Operator) : IExpression
{
    public override string ToString() => $"{LeftExpression} {Operator} {RightExpression}";
}