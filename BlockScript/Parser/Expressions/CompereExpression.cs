using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class CompereExpression : IExpression
{
    public List<IExpression> Expressions { get; }
    public List<TokenType> Operators { get; }

    public CompereExpression(List<IExpression> expressions, List<TokenType> operators)
    {
        Expressions = expressions;
        Operators = operators;
    }

    public override string ToString() => $"{Expressions.Stringify(index => $" {Operators[index - 1].TextValue()} ")}";
}