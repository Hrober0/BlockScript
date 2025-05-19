using BlockScript.Parser.Factors;
using BlockScript.Reader;

namespace BlockScript.Parser.Expressions;

public record NotExpression(IFactor Factor) : IExpression
{
    public Position Position => Factor.Position;
    public override string ToString() => $"-{Factor}";
}