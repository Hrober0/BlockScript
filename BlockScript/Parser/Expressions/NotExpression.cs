using BlockScript.Parser.Factors;

namespace BlockScript.Parser.Expressions;

public class NotExpression : IExpression
{
    public IFactor Factor { get; }
    public bool Nagate { get; }

    public NotExpression(IFactor factor, bool nagate)
    {
        Factor = factor;
        Nagate = nagate;
    }

    public override string ToString() => $"{(Nagate ? "-" : "")}{Factor}";
}