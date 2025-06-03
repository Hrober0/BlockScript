using BlockScript.Lexer.FactorValues;

namespace BlockScript.Interpreter.InternalFactorValues;

public record BlockBreak(int BreakNumber, IFactorValue? LastValue) : IFactorValue;