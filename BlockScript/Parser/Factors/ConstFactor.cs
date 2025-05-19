using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Factors;

public record ConstFactor(object Value, Position Position) : IFactor
{
    public override string ToString() => $"{Value}";
}