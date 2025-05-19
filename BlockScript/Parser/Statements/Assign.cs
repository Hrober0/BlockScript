using BlockScript.Lexer;
using BlockScript.Reader;

namespace BlockScript.Parser.Statements
{
    public record Assign(string Identifier, IStatement Value, bool NullAssign, Position Position) : IStatement
    {
        public override string ToString() => $"${Identifier} {(NullAssign ? "?=" : "=")} {Value}";
    }
}