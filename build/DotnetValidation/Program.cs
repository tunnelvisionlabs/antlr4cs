using System;
using Antlr4.Runtime;

[assembly: CLSCompliant(false)]

namespace DotnetValidation
{
    class Program
    {
        static void Main(string[] args)
        {
            var lexer = new GrammarLexer(new AntlrInputStream("text"));
            var parser = new GrammarParser(new CommonTokenStream(lexer));
            var tree = parser.compilationUnit();

            Console.WriteLine(tree.ToStringTree(parser));
        }
    }
}
