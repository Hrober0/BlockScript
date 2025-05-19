using BlockScript.Parser.Factors;

namespace BlockScript.Parser.Expressions;

public record NotExpression(IFactor Factor) : IExpression
{
    public override string ToString() => $"-{Factor}";
}