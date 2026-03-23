
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using badlang;

using TokenType = badlang.Token.TokenType;

namespace badlang;

internal class Lexer {
    private List<Token> tokens = new();
    private Scanner scanner;
    private int line = 1;

    public Lexer(string code) {
        scanner = new Scanner(code);
    }

    public List<Token> Tokenize() {
        while (!scanner.IsAtEnd) {
            (ScanToken())();
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

    private Action ScanToken() {
        char c = scanner.Advance();

        return c switch {
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
            '=' => () => PushToken(scanner.AdvanceIf(c => c == '=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL),
            '!' => () => PushToken(scanner.AdvanceIf(c => c == '=') ? TokenType.BANG_EQUAL : TokenType.BANG),
            '>' => () => PushToken(scanner.AdvanceIf(c => c == '=') ? TokenType.GREATER_EQUAL : TokenType.GREATER),
            '<' => () => PushToken(scanner.AdvanceIf(c => c == '=') ? TokenType.LESS_EQUAL : TokenType.LESS),
            '+' => () => PushToken(TokenType.PLUS),
            '-' => () => PushToken(TokenType.MINUS),
            '*' => () => PushToken(TokenType.STAR),
            '/' => (() => {
                if (scanner.AdvanceIf(c => c == '/')) {
                    _ = scanner.ScanUntil(c => c == '\n');
                } else if (scanner.AdvanceIf(c => c == '*')) {
                    _ = scanner.ScanUntil(c => c == '*' && scanner.Peek(2) == '/');
                    scanner.Advance();
                    scanner.Advance();
                } else {
                    PushToken(TokenType.SLASH);
                }
            }),
            '\0' => () => PushToken(TokenType.EOF),
            '"' => () => ScanString(),
            '\n' => (() => { line++; }),
            '\r' or '\t' or ' ' => (() => { }),
            _ => (() => {
                if (Scanner.IsAllowedIdentifierStart(c)) {
                    ScanIdentifier();
                } else if (Scanner.IsDigit(c)) {
                    ScanNumber();
                } else {
                    throw new Exception($"unexpected character {c} at line {line}");
                }
            }),
        };
    }

    private void ScanIdentifier() {
        string scanStr = scanner.ScanUntil(c => !Scanner.IsAllowedIdentifier(c));
        PushToken(MatchKeyword(scanStr) ?? TokenType.IDENTIFIER, scanStr);
    }

    private void ScanNumber() {
        string scanStr = scanner.ScanUntil(c => !Scanner.IsDigit(c));
        PushToken(TokenType.INT_LITERAL, int.Parse(scanStr));
    }

    private void ScanString() {
        string scanStr = scanner.ScanUntil(c => c == '"', true);
        scanner.Advance();
        PushToken(TokenType.STRING_LITERAL, scanStr);
    }

    private void PushToken(TokenType type, object? value = null) {
        tokens.Add(new() { Type = type, Value = value, Line = line });
    }
}