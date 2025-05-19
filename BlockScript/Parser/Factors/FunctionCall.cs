using BlockScript.Lexer;
using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public record FunctionCall(string Identifier, List<IExpression> Arguments, Position Position) : IFactor
{
    public override string ToString() => $"{Identifier}({Arguments.Stringify()})";
}