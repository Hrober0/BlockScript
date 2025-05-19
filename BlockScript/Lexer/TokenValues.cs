using BlockScript.Utilities;

namespace BlockScript.Lexer
{
    public static class TokenValues
    {
        public static readonly List<(string token, TokenType tokenType)> Symbols =
        [
            // Syntax
            (UnifiedCharacters.EndOfText.ToString(), TokenType.EndOfText),
            (";", TokenType.EndOfStatement),
            (",", TokenType.Comma),
            ("(", TokenType.ParenthesesOpen),
            (")", TokenType.ParenthesesClose),
            ("{", TokenType.BraceOpen),
            ("}", TokenType.BraceClose),

            // Logical operators
            ("==", TokenType.OperatorEqual),
            ("=>", TokenType.OperatorArrow),
            ("=",  TokenType.OperatorAssign),
            ("<=", TokenType.OperatorLessEqual),
            ("<",  TokenType.OperatorLess),
            (">=", TokenType.OperatorGreaterEqual),
            (">",  TokenType.OperatorGreater),
            ("!=", TokenType.OperatorNotEqual),
            ("||", TokenType.OperatorOr),
            ("&&", TokenType.OperatorAnd),
            ("??", TokenType.OperatorNullCoalescing),
            ("?=", TokenType.OperatorNullAssign),

            // Arithmetical operators
            ("+", TokenType.OperatorAdd),
            ("-", TokenType.OperatorSubtract),
            ("*", TokenType.OperatorMultiply),
            ("/", TokenType.OperatorDivide),
        ];

        public static string TextValue(this TokenType tokenType) =>
            $"'{Symbols.FindOrNull(x => x.tokenType == tokenType)?.token ?? tokenType.ToString()}'";
    }
}