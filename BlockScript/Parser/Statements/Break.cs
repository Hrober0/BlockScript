using BlockScript.Parser.Factors;
using BlockScript.Reader;

namespace BlockScript.Parser.Statements;

public record Break(IStatement? BreakNumber, Position Position) : IStatement
{
    public Break(IStatement? BreakNumber) : this(BreakNumber, Position.Default) {}
    
    public override string ToString() => $"$break  {BreakNumber}";
}