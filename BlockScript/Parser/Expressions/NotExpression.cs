using BlockScript.Parser.Factors;

namespace BlockScript.Parser.Expressions;

public class NotExpression(IFactor factor, bool nagate) : IExpression
{
    public override string ToString() => $"{(nagate ? "-" : "")}{factor}";
}