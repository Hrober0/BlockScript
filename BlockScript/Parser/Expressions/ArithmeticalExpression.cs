using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record ArithmeticalExpression(IExpression Lhs, IExpression Rhs, TokenType Operator, Position Position) : IExpression
{
    public ArithmeticalExpression(IExpression Lhs, IExpression Rhs, TokenType Operator) : this(Lhs, Rhs, Operator, Position.Default) { }
    
    public override string ToString() => $"{Lhs} {Operator.TextValue()} {Rhs}";
}