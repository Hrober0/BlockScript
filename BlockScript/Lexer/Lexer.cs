using System.Text;
using BlockScript.Reader;
using BlockScript.Utilities;

namespace BlockScript.Lexer;

public class Lexer : ILexer
{
    private const int MAX_BUFFER_LENGTH = 255;
    
    private delegate bool TryBuildMethod(out TokenData token);
    
    private readonly CharacterReader _reader;
    private readonly TryBuildMethod[] _tryBuildMethods;

    private readonly List<(string token, TokenType tokenType)> _symbols =
    [
        // Syntax
        (UnifiedCharacters.EndOfText.ToString(), TokenType.EndOfText),
        (";", TokenType.EndOfStatement),
        ("(", TokenType.ParenhticesOpen),
        (")", TokenType.ParenhticesClose),
        ("{", TokenType.BraceOpen),
        ("}", TokenType.BraceClose),

        // Logical operators
        ("==", TokenType.OperatorEqual),
        ("=>", TokenType.OperatorArrow),
        ("=", TokenType.OperatorAssign),
        ("<=", TokenType.OperatorLessEqual),
        ("<", TokenType.OperatorLess),
        (">=", TokenType.OperatorGreaterEqual),
        (">", TokenType.OperatorGreater),
        ("!=", TokenType.OperatorNotEqual),
        ("!", TokenType.OperatorNot),
        ("||", TokenType.OperatorOr),
        ("&&", TokenType.OperatorAnd),
        ("??", TokenType.OperatorNullCoalescing),
        ("?=", TokenType.OperatorNullAssign),
        ("?", TokenType.OperatorTernaryIf),
        (":", TokenType.OperatorTernaryElse),

        // Arithmetical operators
        ("+", TokenType.OperatorAdd),
        ("-", TokenType.OperatorSubtract),
        ("*", TokenType.OperatorMultiply),
        ("/", TokenType.OperatorDivide),
    ];

    private readonly Dictionary<string, TokenType> _keyWords = new()
    {
        { "true", TokenType.Boolean },
        { "false", TokenType.Boolean },
        { "null", TokenType.Null },
        { "loop", TokenType.Loop },
    };
    
    private Character _currentCharacter;
    private Character _nextCharacter;

    public Lexer(TextReader textReader)
    {
        _reader = new CharacterReader(textReader);
        _nextCharacter = _reader.GetCharacter();
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
        TakeCharacter();
        
        // skip white spaces
        while (_currentCharacter.Char is UnifiedCharacters.WhiteSpace or UnifiedCharacters.NewLine)
        {
            TakeCharacter();
        }
        
        var startCharacter = _currentCharacter;
        foreach (var tryBuildMethod in _tryBuildMethods)
        {
            if (tryBuildMethod(out var token))
            {
                return token;
            }
        }
        
        throw new TokenException(startCharacter.Line, startCharacter.Column, $"Invalid token \'{startCharacter.Char}\'");
    }

    private void TakeCharacter()
    {
        _currentCharacter = _nextCharacter;
        _nextCharacter = _reader.GetCharacter();
    }
    
    private bool TryBuildNumber(out TokenData token)
    {
        if (!int.TryParse(_currentCharacter.Char.ToString(), out int value))
        {
            token = default;
            return false;
        }

        var startCharacter = _currentCharacter;
        while (int.TryParse(_nextCharacter.Char.ToString(), out int nextDigit) && value != 0)
        {
            var newValue = value * 10 + nextDigit;
            if (newValue < value)
            {
                throw new TokenException(startCharacter.Line, startCharacter.Column,
                    $"Int value {value}{nextDigit} exceeds {int.MaxValue}.");
            }
            value = newValue;
            TakeCharacter();
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
        if (!char.IsLetter(_currentCharacter.Char))
        {
            token = default;
            return false;
        }
        
        var startCharacter = _currentCharacter;
        var stringBuilder = new StringBuilder();
        while (char.IsLetterOrDigit(_nextCharacter.Char))
        {
            if (stringBuilder.Length >= MAX_BUFFER_LENGTH)
            {
                throw new TokenException(_nextCharacter.Line, _nextCharacter.Column,
                    $"Identifier exceeds {MAX_BUFFER_LENGTH} characters.");
            }

            stringBuilder.Append(_currentCharacter.Char);
            TakeCharacter();
        }
        
        stringBuilder.Append(_currentCharacter.Char);
        
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
        if (_currentCharacter.Char != COMMENT_IDENTIFIER)
        {
            token = default;
            return false;
        }

        var startCharacter = _currentCharacter;
        TakeCharacter();
        var stringBuilder = new StringBuilder();
        while (_currentCharacter.Char is not (UnifiedCharacters.EndOfText or UnifiedCharacters.NewLine))
        {
            if (stringBuilder.Length >= MAX_BUFFER_LENGTH)
            {
                throw new TokenException(_currentCharacter.Line, _currentCharacter.Column,
                    $"Comment exceeds {MAX_BUFFER_LENGTH} characters.");
            }
                
            stringBuilder.Append(_currentCharacter.Char);
            TakeCharacter();
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
        var foundSymbols = _symbols.FindAll(symbol =>
                symbol.token[0] == _currentCharacter.Char &&
                (symbol.token.Length == 1 || symbol.token[1] == _nextCharacter.Char))
            .OrderByDescending(symbol => symbol.token.Length).ToArray();
        if (foundSymbols.Length == 0)
        {
            token = default;
            return false;
        }
        
        var symbol = foundSymbols[0];
        token = new TokenData
        {
            Line = _currentCharacter.Line,
            Column = _currentCharacter.Column,
            Type = symbol.tokenType,
            Value = symbol.token,
        };
        for (var i = 0; i < symbol.token.Length - 1; i++)
        {
            TakeCharacter();
        }
        return true;
    }
    
    private bool TryBuildString(out TokenData token)
    {
        const char STRING_IDENTIFIER = '\"';
        const char SPECIAL_CHARACTER = '\\';
        if (_currentCharacter.Char != STRING_IDENTIFIER)
        {
            token = default;
            return false;
        }

        var startCharacter = _currentCharacter;
        TakeCharacter();
        var stringBuilder = new StringBuilder();
        var isSpecialCharacter = false;
        do
        {
            if (_currentCharacter.Char == STRING_IDENTIFIER && !isSpecialCharacter)
            {
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
                throw new TokenException(_nextCharacter.Line, _nextCharacter.Column,
                    $"String exceeds {MAX_BUFFER_LENGTH} characters.");
            }

            isSpecialCharacter = _currentCharacter.Char == SPECIAL_CHARACTER && !isSpecialCharacter;

            stringBuilder.Append(_currentCharacter.Char);
            TakeCharacter();
        } while (_currentCharacter.Char is not (UnifiedCharacters.EndOfText or UnifiedCharacters.NewLine));

        throw new TokenException(_currentCharacter.Line, _currentCharacter.Column,
            $"Invalid token \'{_currentCharacter.Char}\', expected \'{STRING_IDENTIFIER}\'");
    }
}