using BlockScript.Lexer;

namespace BlockScript.Parser.Factors;

public record ConstFactor(object Value, Position Position) : IFactor
{
    
}