using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record NullCoalescingExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public NullCoalescingExpression(IExpression Lhs, IExpression Rhs) : this(Lhs, Rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} {TokenType.OperatorNullCoalescing.TextValue()} {Rhs}";
}