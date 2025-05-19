using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public record LogicExpression(List<IExpression> Expressions, TokenType Operator) : IExpression
{
    public override string ToString() => $"{Expressions.Stringify($" {Operator.TextValue()} ")}";
}