using BlockScript.Lexer;
using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Statements;

public record Lambda(List<string> Arguments, IStatement Body, Position Position) : IStatement
{
    public override string ToString() => $"Lambda ({Arguments.Stringify()}) => {Body}";
}