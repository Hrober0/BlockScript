using BlockScript.Lexer;
using BlockScript.Utilities;

namespace BlockScript.Parser.Expressions;

public class ArithmeticalExpression(List<IExpression> expressions, List<TokenType> operators) : IExpression
{
    public override string ToString() => $"{expressions.Stringify(index => $" {operators[index - 1].TextValue()} ")}";
}