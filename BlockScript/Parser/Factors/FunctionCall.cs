using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public class FunctionCall(string identifier, List<IExpression> arguments) : IFactor
{
    public override string ToString() => $"{identifier}({arguments.Stringify()})";
}