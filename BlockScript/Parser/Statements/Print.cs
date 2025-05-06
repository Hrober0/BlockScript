using BlockScript.Parser.Expressions;

namespace BlockScript.Parser.Statements
{
    public class Print : IStatement
    {
        public IStatement Body { get; }

        public Print(IStatement body)
        {
            Body = body;
        }

        public override string ToString() => $"Print({Body})";
    }
}