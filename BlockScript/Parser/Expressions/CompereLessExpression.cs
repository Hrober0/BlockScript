using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record CompereLessExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public CompereLessExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} >= {Rhs}";
}