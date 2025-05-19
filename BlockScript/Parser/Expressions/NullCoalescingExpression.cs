using BlockScript.Lexer;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public record NullCoalescingExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public override string ToString() => $"{Lhs} {TokenType.OperatorNullCoalescing.TextValue()} {Rhs}";
}