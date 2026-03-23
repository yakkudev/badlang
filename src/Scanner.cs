using System.Text;
using System.Diagnostics;
using System.IO.Pipelines;

namespace badlang;

internal class Scanner {
    private string code;

    private int position = -1;

    public static bool IsWhiteSpace(char c) => char.IsWhiteSpace(c);
    public static bool IsAlpha(char c) => char.IsAsciiLetter(c);
    public static bool IsAllowedIdentifier(char c) => char.IsAsciiLetterOrDigit(c) | c == '_';
    public static bool IsAllowedIdentifierStart(char c) => char.IsAsciiLetter(c) | c == '_';
    public static bool IsDigit(char c) => char.IsAsciiDigit(c);

    public bool IsAtEnd => position >= code.Length;
    public char Current => code.ElementAtOrDefault(position);

    public Scanner(string code) {
        this.code = code;
    }

    public bool AdvanceIf(Func<char, bool> fn) {
        bool result = fn(Peek());
        if (result)
            position++;
        return result;
    }

    public char Peek(int offset = 1) {
        return code.ElementAtOrDefault(position + offset);
    }

    public char Advance() {
        return code.ElementAtOrDefault(++position);
    }

    public char GoBack() {
        return code.ElementAtOrDefault(--position);
    }

    public string ScanUntil(Func<char, bool> condition, bool skipFirst = false) {
        StringBuilder sb = new();

        if (!skipFirst) {
            if (!condition(Current))
                sb.Append(Current);
            else
                return sb.ToString();
        }
        while (AdvanceIf((next) => !condition(next) && !IsAtEnd)) {
            sb.Append(Current);
        }

        return sb.ToString();
    }
}