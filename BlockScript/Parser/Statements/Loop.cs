using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public class Loop(IExpression condition, IStatement body) : IStatement
    {
        public override string ToString() => $"Loop {condition} {body}";
    }
}