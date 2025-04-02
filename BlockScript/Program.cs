//using TextReader reader = new StringReader("Hello, world!");
using TextReader reader = new StreamReader("test.txt");

var lexer = new Lexer(reader);

Console.WriteLine("WW");
while(true)
{
    var token = lexer.GetToken();
    if (token.Type == TokenType.EOT)
    {
        break;
    }
    Console.WriteLine(token);
}
Console.WriteLine($"EOT");