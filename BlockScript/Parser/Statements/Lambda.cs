using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Statements;

public class Lambda(List<IExpression> arguments, IExpression body) : IStatement
{
    public override string ToString() => $"Lambda ({arguments.Stringify()}) => {body}";
}