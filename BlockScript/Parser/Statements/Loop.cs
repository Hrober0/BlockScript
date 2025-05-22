using BlockScript.Parser.Expressions;
using BlockScript.Reader;

namespace BlockScript.Parser.Statements
{
    public record Loop(IExpression Condition, IStatement Body, Position Position) : IStatement
    {
        public override string ToString() => $"Loop {Condition} {Body}";
    }
}