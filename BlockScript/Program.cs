using BlockScript.Lexer;

using TextReader reader = new StreamReader("CodeExamples/Test.txt");

var lexer = new Lexer(reader);

Console.WriteLine("Start");
while(true)
{
    var tokenData = lexer.GetToken();
    if (tokenData.Type == TokenType.EOT)
    {
        break;
    }
    Console.WriteLine(tokenData);
}
Console.WriteLine($"EOT");