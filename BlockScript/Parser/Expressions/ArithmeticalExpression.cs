using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public record ArithmeticalExpression(List<IExpression> Expressions, List<TokenType> Operators) : IExpression
{
    public override string ToString() => $"{Expressions.Stringify(index => $" {Operators[index - 1].TextValue()} ")}";
}