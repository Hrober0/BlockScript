using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record ArithmeticalAddExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public ArithmeticalAddExpression(IExpression Lhs, IExpression Rhs) : this(Lhs, Rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} + {Rhs}";
}