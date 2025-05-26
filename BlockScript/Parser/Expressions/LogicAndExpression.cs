using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record LogicAndExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public LogicAndExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} and {Rhs}";
}