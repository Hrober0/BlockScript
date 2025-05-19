namespace BlockScript.Lexer;

public enum TokenType
{
    // Syntax
    EndOfText,
    EndOfStatement,
    Comma,
    ParenhticesOpen,
    ParenhticesClose,
    BraceOpen,
    BraceClose,
    
    // Keywords
    Loop,
    If,
    Else,
    
    // Data
    Integer,
    String,
    Boolean,
    Null,
    
    Comment,
    Identifier,

    // Logical Operators
    OperatorEqual,
    OperatorArrow,
    OperatorAssign,
    OperatorLessEqual,
    OperatorLess,
    OperatorGreaterEqual,
    OperatorGreater,
    OperatorNotEqual,
    OperatorOr,
    OperatorAnd,
    OperatorNullCoalescing,
    OperatorNullAssign,

    // Arithmetical Operators
    OperatorAdd,
    OperatorSubtract,
    OperatorMultiply,
    OperatorDivide,
}
