using System.Text;
using BlockScript.Exceptions;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Lexer;

public class Lexer
{
    private const int MAX_BUFFER_LENGTH = 255;
    
    private delegate bool TryBuildMethod(out TokenData token);
    
    private readonly StreamBuffer<Character> _buffer;
    private readonly TryBuildMethod[] _tryBuildMethods;

    private readonly Dictionary<string, TokenType> _keyWords = new()
    {
        { "true",  TokenType.Boolean },
        { "false", TokenType.Boolean },
        { "null",  TokenType.Null },
        { "loop",  TokenType.Loop },
        { "if",    TokenType.If },
        { "else",  TokenType.Else },
        { "print",  TokenType.Print },
    };

    public Lexer(TextReader textReader)
    {
        _buffer = new StreamBuffer<Character>(new CharacterReader(textReader).GetCharacter);
        _tryBuildMethods =
        [
            TryBuildNumber,
            TryBuildingIdentifierOrKeyword,
            TryBuildComment,
            TryBuildSymbol,
            TryBuildString,
        ];
    }

    public TokenData GetToken()
    {
        // skip white spaces
        while (_buffer.Current.Char is UnifiedCharacters.WhiteSpace or UnifiedCharacters.NewLine)
        {
            _buffer.Take();
        }
        
        var startCharacter = _buffer.Current;
        foreach (var tryBuildMethod in _tryBuildMethods)
        {
            if (tryBuildMethod(out var token))
            {
                return token;
            }
        }
        
        throw new TokenException(startCharacter.Line, startCharacter.Column, $"Invalid token \'{startCharacter.Char}\'");
    }
    
    private bool TryBuildNumber(out TokenData token)
    {
        if (!int.TryParse(_buffer.Current.Char.ToString(), out int value))
        {
            token = default;
            return false;
        }

        var startCharacter = _buffer.Take();
        while (int.TryParse(_buffer.Current.Char.ToString(), out int nextDigit))
        {
            var newValue = value * 10 + nextDigit;
            if (newValue < value)
            {
                throw new TokenException(startCharacter.Line, startCharacter.Column,
                    $"Int value {value}{nextDigit} exceeds {int.MaxValue}.");
            }
            value = newValue;
            _buffer.Take();
        }
            
        token = new TokenData
        {
            Line = startCharacter.Line,
            Column = startCharacter.Column,
            Type = TokenType.Integer,
            Value = value,
        };
        return true;
    }
    
    private bool TryBuildingIdentifierOrKeyword(out TokenData token)
    {
        if (!char.IsLetter(_buffer.Current.Char))
        {
            token = default;
            return false;
        }
        
        var startCharacter = _buffer.Take();
        var stringBuilder = new StringBuilder(startCharacter.Char.ToString());
        while (char.IsLetterOrDigit(_buffer.Current.Char))
        {
            if (stringBuilder.Length >= MAX_BUFFER_LENGTH)
            {
                throw new TokenException(_buffer.Next.Line, _buffer.Next.Column,
                    $"Identifier exceeds {MAX_BUFFER_LENGTH} characters.");
            }

            stringBuilder.Append(_buffer.Current.Char);
            _buffer.Take();
        }
        
        var stringValue = stringBuilder.ToString();
        if (_keyWords.TryGetValue(stringValue, out var tokenType))
        {
            object value = tokenType switch
            {
                TokenType.Boolean => stringValue == "true",
                _ => stringValue,
            };
            
            token = new TokenData
            {
                Line = startCharacter.Line,
                Column = startCharacter.Column,
                Type = tokenType,
                Value = value,
            };
            return true;
        }
            
        token = new TokenData
        {
            Line = startCharacter.Line,
            Column = startCharacter.Column,
            Type = TokenType.Identifier,
            Value = stringValue,
        };
        return true;
    }
    
    private bool TryBuildComment(out TokenData token)
    {
        const char COMMENT_IDENTIFIER = '#';
        if (_buffer.Current.Char != COMMENT_IDENTIFIER)
        {
            token = default;
            return false;
        }

        var startCharacter = _buffer.Take();
        var stringBuilder = new StringBuilder();
        while (_buffer.Current.Char is not (UnifiedCharacters.EndOfText or UnifiedCharacters.NewLine))
        {
            if (stringBuilder.Length >= MAX_BUFFER_LENGTH)
            {
                throw new TokenException(_buffer.Current.Line, _buffer.Current.Column,
                    $"Comment exceeds {MAX_BUFFER_LENGTH} characters.");
            }
                
            stringBuilder.Append(_buffer.Current.Char);
            _buffer.Take();
        }
            
        token = new TokenData
        {
            Line = startCharacter.Line,
            Column = startCharacter.Column,
            Type = TokenType.Comment,
            Value = stringBuilder.ToString(),
        };
        return true;
    }

    private bool TryBuildSymbol(out TokenData token)
    {
        var foundSymbols = TokenValues.Symbols.FindAll(symbol =>
                symbol.token[0] == _buffer.Current.Char &&
                (symbol.token.Length == 1 || symbol.token[1] == _buffer.Next.Char))
            .OrderByDescending(symbol => symbol.token.Length).ToArray();
        if (foundSymbols.Length == 0)
        {
            token = default;
            return false;
        }
        
        var symbol = foundSymbols[0];
        token = new TokenData
        {
            Line = _buffer.Current.Line,
            Column = _buffer.Current.Column,
            Type = symbol.tokenType,
            Value = symbol.token,
        };
        for (var i = 0; i < symbol.token.Length; i++)
        {
            _buffer.Take();
        }
        return true;
    }
    
    private bool TryBuildString(out TokenData token)
    {
        const char STRING_IDENTIFIER = '\"';
        const char SPECIAL_CHARACTER = '\\';
        if (_buffer.Current.Char != STRING_IDENTIFIER)
        {
            token = default;
            return false;
        }

        var startCharacter = _buffer.Take();
        var stringBuilder = new StringBuilder();
        var isSpecialCharacter = false;
        while (_buffer.Current.Char is not (UnifiedCharacters.EndOfText or UnifiedCharacters.NewLine))
        {
            if (_buffer.Current.Char == STRING_IDENTIFIER && !isSpecialCharacter)
            {
                _buffer.Take();
                token = new TokenData
                {
                    Line = startCharacter.Line,
                    Column = startCharacter.Column,
                    Type = TokenType.String,
                    Value = stringBuilder.ToString(),
                };
                return true;
            }

            if (stringBuilder.Length >= MAX_BUFFER_LENGTH)
            {
                throw new TokenException(_buffer.Next.Line, _buffer.Next.Column,
                    $"String exceeds {MAX_BUFFER_LENGTH} characters.");
            }

            isSpecialCharacter = _buffer.Current.Char == SPECIAL_CHARACTER && !isSpecialCharacter;

            stringBuilder.Append(_buffer.Current.Char);
            _buffer.Take();
        }

        if (isSpecialCharacter)
        {
            throw new TokenException(_buffer.Current.Line, _buffer.Current.Column,
                $"Unexpected end of string, expected character after \'{SPECIAL_CHARACTER}\' and string close \'{STRING_IDENTIFIER}\'.");    
        }
        
        throw new TokenException(_buffer.Current.Line, _buffer.Current.Column,
            $"Unexpected end of string, expected \'{STRING_IDENTIFIER}\'");
    }
}