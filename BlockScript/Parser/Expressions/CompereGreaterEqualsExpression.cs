using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record CompereGreaterEqualExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public CompereGreaterEqualExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} >= {Rhs}";
}