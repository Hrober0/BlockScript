using BlockScript.Parser.Statements;
using BlockScript.Reader;

namespace BlockScript.Interpreter.BuildInStatements
{
    public record Print(Position Position) : IStatement
    {
        public const string PARAMTEER_NAME = "printMessage";
    }
}