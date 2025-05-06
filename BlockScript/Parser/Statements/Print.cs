using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public class Print(IStatement body) : IStatement
    {
        public override string ToString() => $"Print({body})";
    }
}