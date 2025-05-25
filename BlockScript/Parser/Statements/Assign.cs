using BlockScript.Reader;

namespace BlockScript.Parser.Statements
{
    public record Assign(string Identifier, IStatement Value, bool NullAssign, Position Position) : IStatement
    {
        public Assign(string identifier, IStatement value, bool nullAssign = false) : this(identifier, value, nullAssign, Position.Default) {}
        
        public override string ToString() => $"${Identifier} {(NullAssign ? "?=" : "=")} {Value}";
    }
}