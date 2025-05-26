using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record CompereLessEqualExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public CompereLessEqualExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} >= {Rhs}";
}