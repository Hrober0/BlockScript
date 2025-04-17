using BlockScript.Lexer;

using TextReader reader = new StreamReader("CodeExamples/Test.txt");

var lexer = new Lexer(reader);

Console.WriteLine("Start");
try
{
    while (true)
    {
        var tokenData = lexer.GetToken();
        if (tokenData.Type == TokenType.EndOfText)
        {
            break;
        }

        Console.WriteLine(tokenData);
    }

    Console.WriteLine($"EOT");
}
catch (Exception e)
{
    Console.WriteLine($"Error: {e.Message}");
}