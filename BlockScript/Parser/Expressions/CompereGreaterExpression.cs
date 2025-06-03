using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record CompereGreaterExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public CompereGreaterExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} > {Rhs}";
}