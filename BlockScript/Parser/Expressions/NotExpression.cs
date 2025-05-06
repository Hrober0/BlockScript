using BlockScript.Parser.Factors;

namespace BlockScript.Parser.Expressions;

public class NotExpression : IExpression
{
    public IFactor Factor { get; }

    public NotExpression(IFactor factor)
    {
        Factor = factor;
    }

    public override string ToString() => $"-{Factor}";
}