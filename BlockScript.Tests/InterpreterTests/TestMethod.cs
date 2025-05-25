using BlockScript.Interpreter;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser.Statements;

namespace BlockScript.Tests.InterpreterTests;

public class TestMethod(List<IFactorValue> stack) : BuildInMethod
{
    public const string IDENTIFIER = "testMethod";
    
    public override string Identifier => IDENTIFIER;
    public override List<string> Arguments => ["__input"];

    public override IFactorValue Execute(Func<IStatement, IFactorValue> execute, Context context)
    {
        var result = context.GetContextData("__input", Reader.Position.Default);
        stack.Add(result);
        return result;
    } 
}