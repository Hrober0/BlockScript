using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class LogicExpression : IExpression
{
    public List<IExpression> Expressions { get; }
    public TokenType Type { get; }

    public LogicExpression(List<IExpression> expressions, TokenType type)
    {
        Expressions = expressions;
        Type = type;
    }

    public override string ToString() => $"{Expressions.Stringify($" {Type.TextValue()} ")}";
}