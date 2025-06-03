using BlockScript.Reader;

namespace BlockScript.Parser.Statements;

public record Assign(string Identifier, IStatement Value, Position Position) : IStatement
{
    public Assign(string identifier, IStatement value) : this(identifier, value, Position.Default) {}
        
    public override string ToString() => $"${Identifier} := {Value}";
}