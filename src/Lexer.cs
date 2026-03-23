
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TokenType = Token.TokenType;

internal class Lexer {
    private string code;
    private List<Token> tokens = new();

    private int position = -1;
    private int line = 1;

    public Lexer(string code) {
        this.code = code;
    }

    public List<Token> Tokenize() {
        while (!IsAtEnd()) {
            ScanToken();
        }
        return tokens;
    }

    static TokenType? MatchKeyword(string str) => str switch {
        "true" => TokenType.TRUE,
        "false" => TokenType.FALSE,
        "ret" => TokenType.RETURN,
        "fun" => TokenType.FUN,
        "struct" => TokenType.STRUCT,
        "if" => TokenType.IF,
        "for" => TokenType.FOR,
        _ => null,
    };

    private void ScanToken() {
        char c = Advance();

        Action action = c switch {
            '(' => () => PushToken(TokenType.LEFT_PAREN),
            ')' => () => PushToken(TokenType.RIGHT_PAREN),
            '{' => () => PushToken(TokenType.LEFT_BRACE),
            '}' => () => PushToken(TokenType.RIGHT_BRACE),
            '[' => () => PushToken(TokenType.LEFT_BRACKET),
            ']' => () => PushToken(TokenType.RIGHT_BRACKET),
            ';' => () => PushToken(TokenType.SEMICOLON),
            ',' => () => PushToken(TokenType.COMMA),
            '.' => () => PushToken(TokenType.DOT),
            ':' => () => PushToken(TokenType.COLON),
            '=' => () => PushToken(AdvanceIf(c => c == '=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL),
            '!' => () => PushToken(AdvanceIf(c => c == '=') ? TokenType.BANG_EQUAL : TokenType.BANG),
            '>' => () => PushToken(AdvanceIf(c => c == '=') ? TokenType.GREATER_EQUAL : TokenType.GREATER),
            '<' => () => PushToken(AdvanceIf(c => c == '=') ? TokenType.LESS_EQUAL : TokenType.LESS),
            '+' => () => PushToken(TokenType.PLUS),
            '-' => () => PushToken(TokenType.MINUS),
            '*' => () => PushToken(TokenType.STAR),
            '/' => (() => {
                if (AdvanceIf(c => c == '/')) {
                    _ = ScanUntil(c => c == '\n');
                } else if (AdvanceIf(c => c == '*')) {
                    _ = ScanUntil(c => c == '*' && Peek(2) == '/');
                    Advance();
                    Advance();
                } else {

                    PushToken(TokenType.SLASH);
                }
            }),
            '\0' => () => PushToken(TokenType.EOF),
            '"' => () => ScanString(),
            '\n' => (() => { line++; }),
            '\r' or '\t' or ' ' => (() => { }),
            _ => (() => {
                if (IsAllowedIdentifierStart(c)) {
                    ScanIdentifier();
                } else if (IsDigit(c)) {
                    ScanNumber();
                } else {
                    throw new Exception($"unexpected character {c} at line {line}");
                }
            }),
        };
        action();
    }


    static bool IsWhiteSpace(char c) => char.IsWhiteSpace(c);
    static bool IsAlpha(char c) => char.IsAsciiLetter(c);
    static bool IsAllowedIdentifier(char c) => char.IsAsciiLetterOrDigit(c) | c == '_';
    static bool IsAllowedIdentifierStart(char c) => char.IsAsciiLetter(c) | c == '_';
    static bool IsDigit(char c) => char.IsAsciiDigit(c);


    private string ScanUntil(Func<char, bool> condition, bool skipFirst = false) {
        StringBuilder sb = new();

        if (!skipFirst) {
            if (!condition(Current()))
                sb.Append(Current());
            else
                return sb.ToString();
        }
        while (AdvanceIf((next) => !condition(next) && !IsAtEnd())) {
            sb.Append(Current());
        }

        return sb.ToString();
    }

    private void ScanIdentifier() {
        string scanStr = ScanUntil(c => !IsAllowedIdentifier(c));
        PushToken(MatchKeyword(scanStr) ?? TokenType.IDENTIFIER, scanStr);
    }

    private void ScanNumber() {
        string scanStr = ScanUntil(c => !IsDigit(c));
        PushToken(TokenType.INT_LITERAL, int.Parse(scanStr));
    }

    private void ScanString() {
        string scanStr = ScanUntil(c => c == '"', true);
        Advance();
        PushToken(TokenType.STRING_LITERAL, scanStr);
    }

    private void PushToken(TokenType type, object? value = null) {
        tokens.Add(new() { Type = type, Value = value, Line = line });
    }


    private bool IsAtEnd() {
        return position >= code.Length;
    }

    private char Current() {
        return code.ElementAtOrDefault(position);
    }

    private bool AdvanceIf(Func<char, bool> fn) {
        bool result = fn(Peek());
        if (result)
            position++;
        return result;
    }

    private char Peek(int offset = 1) {
        return code.ElementAtOrDefault(position + offset);
    }

    private char Advance() {
        return code.ElementAtOrDefault(++position);
    }

    private char GoBack() {
        return code.ElementAtOrDefault(--position);
    }

}