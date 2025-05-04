using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class LogicExpression(List<IExpression> expressions, TokenType type) : IExpression
{
    public override string ToString() => $"({expressions.Stringify($" {type.ToString()} ")})";
}