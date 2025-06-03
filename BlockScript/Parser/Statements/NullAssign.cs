using BlockScript.Reader;

namespace BlockScript.Parser.Statements;

public record NullAssign(string Identifier, IStatement Value, Position Position) : IStatement
{
    public NullAssign(string identifier, IStatement value) : this(identifier, value, Position.Default) {}
        
    public override string ToString() => $"${Identifier} := {Value}";
}