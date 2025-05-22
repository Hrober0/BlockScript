using BlockScript.Lexer.FactorValues;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Parser.Statements;

public record Lambda(List<string> Arguments, IStatement Body, Position Position) : IStatement, IFactorValue
{
    public override string ToString() => $"Lambda ({Arguments.Stringify()}) => {Body}";
}