using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class LogicExpression : IExpression
{
    public List<IExpression> Expressions { get; }
    public TokenType Operator { get; }

    public LogicExpression(List<IExpression> expressions, TokenType operatortype)
    {
        Expressions = expressions;
        Operator = operatortype;
    }

    public override string ToString() => $"{Expressions.Stringify($" {Operator.TextValue()} ")}";
}