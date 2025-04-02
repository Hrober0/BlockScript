using System;

public class Lexer
{
    private const char EOT = '\u0004';

    private readonly TextReader textReader;

    private char currentCharacter = '\0';
    private int line = 1;
    private int column = 0;

    public Lexer(TextReader textReader)
	{
        this.textReader = textReader;
	}

    public Token GetToken()
    {
        currentCharacter = ReadCharacter();
        if (currentCharacter == EOT)
        {
            return new Token()
            {
                Type = TokenType.EOT,
                Value = currentCharacter.ToString(),
                Line = line,
                Column = column,
            };
        }

        var token = new Token()
        {
            Type = TokenType.Operator,
            Value = currentCharacter.ToString(),
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

        char chracter = ParseChar(textReader.Read());

        // new line
        if (chracter is '\r' or '\n')
        {
            line++;
            column = 0;

            // check windows CR LF sequence
            char nextChar = ParseChar(textReader.Peek());
            if (chracter is '\r' && nextChar is '\n')
            {
                textReader.Read();
            }

            return ReadCharacter();
        }

        // skip white space
        if (char.IsWhiteSpace(chracter))
        {
            column++;
            return ReadCharacter();
        }

        column++;
        return chracter;
    }

    private char ParseChar(int streamChar)
    {
        if (streamChar < 0)
        {
            return EOT;
        }
        return (char)streamChar;
    }
}
