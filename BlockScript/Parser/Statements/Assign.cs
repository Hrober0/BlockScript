using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public class Assign : IStatement
    {
        public string Identifier { get; }
        public IStatement Value { get; }
        public bool NullAssign { get; }
        
        public Assign(string identifier, IStatement value, bool nullAssign)
        {
            Identifier = identifier;
            Value = value;
            NullAssign = nullAssign;
        }
        
        public override string ToString() => $"${Identifier} {(NullAssign ? "?=" : "=")} {Value}";
    }
}