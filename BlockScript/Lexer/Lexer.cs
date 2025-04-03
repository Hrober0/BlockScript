namespace BlockScript.Lexer;

public class Lexer(TextReader textReader) : ILexer
{
    private const char EOT = '\u0004';

    private char currentCharacter = '\0';
    private int line = 1;
    private int column = 0;

    public TokenData GetToken()
    {
        var character = ReadCharacter();
        if (character == EOT)
        {
            return new TokenData()
            {
                Type = TokenType.EOT,
                Value = character.ToString(),
                Line = line,
                Column = column,
            };
        }

        var token = new TokenData()
        {
            Type = TokenType.Operator,
            Value = character.ToString(),
            Line = line,
            Column = column,
        };
        return token;
    }

    private char ReadCharacter()
    {
        if (currentCharacter == EOT)
        {
            return EOT;
        }

        currentCharacter = ParseChar(textReader.Read());

        // new line
        if (currentCharacter is '\r' or '\n')
        {
            line++;
            column = 0;

            // check windows CR LF sequence
            char nextChar = ParseChar(textReader.Peek());
            if (currentCharacter is '\r' && nextChar is '\n')
            {
                textReader.Read();
            }

            return ReadCharacter();
        }

        // skip white space
        if (char.IsWhiteSpace(currentCharacter))
        {
            column++;
            return ReadCharacter();
        }

        column++;
        return currentCharacter;
    }

    private static char ParseChar(int streamChar)
    {
        if (streamChar < 0)
        {
            return EOT;
        }
        return (char)streamChar;
    }
}