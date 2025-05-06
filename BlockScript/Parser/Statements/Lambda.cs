using BlockScript.Parser.Expressions;
using BlockScript.Utilities;

namespace BlockScript.Parser.Statements;

public class Lambda : IStatement
{
    public List<IExpression> Arguments { get; }
    public IStatement Body { get; }

    public Lambda(List<IExpression> arguments, IStatement body)
    {
        Arguments = arguments;
        Body = body;
    }

    public override string ToString() => $"Lambda ({Arguments.Stringify()}) => {Body}";
}