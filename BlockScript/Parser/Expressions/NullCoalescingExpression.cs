using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public record NullCoalescingExpression(List<IExpression> Expressions) : IExpression
{
    public override string ToString() => $"{Expressions.Stringify(" ?? ")}";
}