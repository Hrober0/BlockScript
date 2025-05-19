using BlockScript.Lexer;
using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public record Print(IStatement Body, Position Position) : IStatement
    {
        public override string ToString() => $"Print({Body})";
    }
}