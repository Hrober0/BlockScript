using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record CompereNotEqualsExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public CompereNotEqualsExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} == {Rhs}";
}