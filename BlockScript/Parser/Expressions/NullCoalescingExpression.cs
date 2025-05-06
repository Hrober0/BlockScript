using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class NullCoalescingExpression : IExpression
{
    public List<IExpression> Expressions { get; }

    public NullCoalescingExpression(List<IExpression> expressions)
    {
        Expressions = expressions;
    }

    public override string ToString() => $"{Expressions.Stringify(" ?? ")}";
}