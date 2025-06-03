using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record CompereEqualsExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public CompereEqualsExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} == {Rhs}";
}