using BlockScript.Lexer.FactorValues;
using BlockScript.Reader;

namespace BlockScript.Interpreter.BuildInMethods
{
    public class Print : BuildInMethod
    {
        private const string PARAMETER_NAME = "printMessage";

        public override string Identifier => "print";
        public override List<string> Arguments => [PARAMETER_NAME];

        public override IFactorValue Execute(Context context)
        {
            var value = context.GetContextData(PARAMETER_NAME, Position.Default);
            Console.WriteLine(value);
            return value;
        }
    }
}