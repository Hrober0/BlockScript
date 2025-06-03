using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;

namespace BlockScript.Interpreter;

public record FunctionCall(Lambda Lambda, Context Context) : IFactorValue;