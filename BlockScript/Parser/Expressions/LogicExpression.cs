using BlockScript.Lexer;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public record LogicExpression(IExpression Lhs, IExpression Rhs, TokenType Operator, Position Position) : IExpression
{
    public override string ToString() => $"{Lhs} {Operator.TextValue()} {Rhs}";
}