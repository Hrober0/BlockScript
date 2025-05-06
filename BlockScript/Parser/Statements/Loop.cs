using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public class Loop : IStatement
    {
        public IExpression Condition { get; }
        public IStatement Body { get; }

        public Loop(IExpression condition, IStatement body)
        {
            Condition = condition;
            Body = body;
        }

        public override string ToString() => $"Loop {Condition} {Body}";
    }
}