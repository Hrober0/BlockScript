using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;

namespace BlockScript.Interpreter.InternalFactorValues;

public record FunctionContext(Lambda Lambda, Context Context) : IFactorValue;