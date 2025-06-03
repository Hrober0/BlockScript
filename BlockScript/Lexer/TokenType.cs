namespace BlockScript.Lexer;

public enum TokenType
{
    // Syntax
    EndOfText,
    EndOfStatement,
    Comma,
    ParenthesesOpen,
    ParenthesesClose,
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
    OperatorLessEqual,
    OperatorLess,
    OperatorGreaterEqual,
    OperatorGreater,
    OperatorNotEqual,
    OperatorOr,
    OperatorAnd,
    OperatorNullCoalescing,
    OperatorArrow,
    OperatorAssign,
    OperatorNullAssign,
    OperatorDeclaration,

    // Arithmetical Operators
    OperatorAdd,
    OperatorSubtract,
    OperatorMultiply,
    OperatorDivide,
}
