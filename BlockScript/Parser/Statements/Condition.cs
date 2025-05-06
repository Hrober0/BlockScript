using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Statements
{
    public class Condition(List<(IExpression condition, IStatement body)> conditionaryItems, IStatement? elseBody) : IStatement
    {
        public override string ToString() => $"if {conditionaryItems.Stringify("else if")} {(elseBody != null ? $"else {elseBody}" : "")}";
    }
}