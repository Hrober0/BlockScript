using BlockScript.Lexer;
using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public record Loop(IExpression Condition, IStatement Body, Position Position) : IStatement
    {
        public override string ToString() => $"Loop {Condition} {Body}";
    }
}