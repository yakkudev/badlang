namespace badlang;

internal class Token {
    internal enum TokenType {
        LEFT_PAREN, RIGHT_PAREN,
        LEFT_BRACE, RIGHT_BRACE,
        LEFT_BRACKET, RIGHT_BRACKET,
        SEMICOLON, COMMA, DOT, COLON,
        PLUS, MINUS, STAR, SLASH,
        EQUAL,
        EQUAL_EQUAL,
        BANG, BANG_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,
        LOGIC_AND, LOGIC_OR,
        BITWISE_AND, BITWISE_OR,

        STRING_LITERAL, INT_LITERAL,

        IDENTIFIER,

        TRUE, FALSE,
        FUN, RETURN,
        STRUCT,
        IF, FOR,
        RESERVED,

        EOF
    }

    required public TokenType Type { get; set; }
    required public object? Value { get; set; }

    required public int Line { get; set; }

}