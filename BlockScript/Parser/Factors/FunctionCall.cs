using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public class FunctionCall : IFactor
{
    public string Identifier { get; }
    public List<IExpression> Arguments { get; }
    
    public FunctionCall(string identifier, List<IExpression> arguments)
    {
        Identifier = identifier;
        Arguments = arguments;
    }
    
    public override string ToString() => $"{Identifier}({Arguments.Stringify()})";
}