﻿using BlockScript.Parser.Expressions;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Parser.Statements
{
    public record Condition(List<(IExpression condition, IStatement body)> ConditionaryItems, IStatement? ElseBody, Position Position)
        : IStatement
    {
        public Condition(List<(IExpression condition, IStatement body)> conditionaryItems, IStatement? elseBody) : this(conditionaryItems, elseBody, Position.Default){}
        
        public override string ToString() => $"if {ConditionaryItems.Stringify("else if")} {(ElseBody != null ? $"else {ElseBody}" : "")}";
    }
}