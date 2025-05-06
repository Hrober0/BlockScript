using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public class Assign(string identifier, IStatement statement, bool nullAssign) : IStatement
    {
        public override string ToString() => $"${identifier} {(nullAssign ? "?=" : "=")} {statement}";
    }
}