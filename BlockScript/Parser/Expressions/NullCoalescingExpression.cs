using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class NullCoalescingExpression(List<IExpression> expressions) : IExpression
{
    public override string ToString() => $"{expressions.Stringify(" ?? ")}";
}