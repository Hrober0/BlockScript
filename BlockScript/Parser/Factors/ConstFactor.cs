using BlockScript.Lexer.FactorValues;
using BlockScript.Reader;

namespace BlockScript.Parser.Factors;

public record ConstFactor(IFactorValue Value, Position Position) : IFactor
{
    public override string ToString() => $"{Value}";
}