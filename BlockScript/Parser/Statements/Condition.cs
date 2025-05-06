using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Statements
{
    public class Condition : IStatement
    {
        public List<(IExpression condition, IStatement body)> ConditionaryItems { get; }
        public IStatement? ElseBody { get; }

        public Condition(List<(IExpression condition, IStatement body)> conditionaryItems, IStatement? elseBody)
        {
            ConditionaryItems = conditionaryItems;
            ElseBody = elseBody;
        }

        public override string ToString() => $"if {ConditionaryItems.Stringify("else if")} {(ElseBody != null ? $"else {ElseBody}" : "")}";
    }
}