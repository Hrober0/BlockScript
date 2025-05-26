using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;

namespace BlockScript.Interpreter.BuildInMethods;

public class DebugMethod(List<IFactorValue> stack) : BuildInMethod
{
    public const string IDENTIFIER = "debug";
    
    public override string Identifier => IDENTIFIER;
    public override List<string> Arguments => ["__input"];

    public override IFactorValue Execute(Func<IStatement, IFactorValue> execute, Context context)
    {
        var result = context.GetContextData("__input", Reader.Position.Default);
        stack.Add(result);
        return result;
    } 
}