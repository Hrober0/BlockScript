using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public class Assign(string identifier, IExpression expression, bool nullAssign) : IStatement
    {
        public override string ToString() => $"${identifier} {(nullAssign ? "?=" : "=")} {expression}";
    }
}