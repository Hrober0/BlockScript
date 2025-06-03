using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;
using BlockScript.Reader;

namespace BlockScript.Interpreter.BuildInMethods
{
    public class PrintMethod : BuildInMethod
    {
        public override string Identifier => "print";
        public override List<string> Arguments => ["__printInput"];

        public override IFactorValue Execute(Func<IStatement, IFactorValue> execute, Context context)
        {
            var value = context.GetContextData("__printInput", Position.Default);
            Console.WriteLine(value);
            return value;
        }
    }
}