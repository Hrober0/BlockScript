using BlockScript.Parser.Expressions;
using BlockScript.Parser.Statements;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Parser.Factors;

public record FunctionCall(IStatement Callable, List<IExpression> Arguments, Position Position) : IFactor
{
    public FunctionCall(IStatement callable, List<IExpression> arguments) : this(callable, arguments, Position.Default) { }
    public FunctionCall(string variableFactor, List<IExpression> arguments) : this(new VariableFactor(variableFactor), arguments, Position.Default) { }
    
    public override string ToString() => $"{Callable}({Arguments.Stringify()})";
}