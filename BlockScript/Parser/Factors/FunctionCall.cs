using BlockScript.Parser.Expressions;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public record FunctionCall(string Identifier, List<IExpression> Arguments, Position Position) : IFactor
{
    public FunctionCall(string identifier, List<IExpression> arguments) : this(identifier, arguments, Position.Default) { }
    
    public override string ToString() => $"{Identifier}({Arguments.Stringify()})";
}