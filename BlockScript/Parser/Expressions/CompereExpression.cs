using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record CompereExpression(IExpression Lhs, IExpression Rhs, TokenType Operator, Position Position) : IExpression
{
    public override string ToString() => $"{Lhs} {Operator.TextValue()} {Rhs}";
}