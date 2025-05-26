using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record LogicOrExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public LogicOrExpression(IExpression lhs, IExpression rhs) : this(lhs, rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} or {Rhs}";
}