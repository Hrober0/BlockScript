using BlockScript.Interpreter;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Lexer;
using BlockScript.Parser;

var codePath = args.Length > 0 ? args[0] : "CodeExamples/Test.txt"; 

if (!File.Exists(codePath))
{
    Console.WriteLine($"Error: File '{codePath}' does not exist.");
    return;
}

using TextReader reader = new StreamReader(codePath);

var lexer = new Lexer(reader);
try
{
    var parser = new LanguageParser(lexer.GetToken);
    var program = parser.ParserProgram();
    var buildInMethods = new List<BuildInMethod>()
    {
        new PrintMethod(),
    };
    var interpreter = new LanguageInterpreter(buildInMethods);
    var returnValue = interpreter.ExecuteProgram(program);
    Console.WriteLine($"Execution result: {returnValue}");
}
catch (Exception e)
{
    Console.WriteLine($"Error: {e.Message}");
}