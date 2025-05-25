using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;

namespace BlockScript.Interpreter;

public record ContextLambda(Lambda Lambda, Context Context) : IFactorValue;