using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record ArithmeticalDivideExpression(IExpression Lhs, IExpression Rhs, Position Position) : IExpression
{
    public ArithmeticalDivideExpression(IExpression Lhs, IExpression Rhs) : this(Lhs, Rhs, Position.Default) { }
    
    public override string ToString() => $"{Lhs} / {Rhs}";
}