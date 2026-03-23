using System;

namespace badlang;

internal class Program {
    static void Main(string[] args) {
        if (args.Length == 0) {
            Console.WriteLine("Usage: badlang <file>");
            return;
        }

        var file = args[0];
        var code = File.ReadAllText(file);
        var lexer = new Lexer(code);
        var tokens = lexer.Tokenize();
        foreach (var token in tokens) {
            Console.Write(token.Type.ToString() + " ");
            if (token.Value != null)
                Console.Write($"\"{token.Value}\"");
            Console.Write("\n");
        }
        // var parser = new Parser(tokens);
        // var ast = parser.Parse();
        // var interpreter = new Interpreter();
        // interpreter.Interpret(ast);
    }
}
