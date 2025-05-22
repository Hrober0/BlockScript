using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;
using BlockScript.Reader;

namespace BlockScript.Interpreter.BuildInMethods
{
    public abstract class BuildInMethod : IStatement
    {
        public Position Position => Position.Default;
        
        public abstract string Identifier { get; }
        public abstract List<string> Arguments { get; }

        public abstract IFactorValue Execute(Context context);
    }
}