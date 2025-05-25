using BlockScript.Reader;

namespace BlockScript.Parser.Statements;

public record Declaration(string Identifier, IStatement Value, Position Position) : IStatement
{
    public Declaration(string identifier, IStatement value) : this(identifier, value, Position.Default) {}
        
    public override string ToString() => $"${Identifier} = {Value}";
}