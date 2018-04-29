using System;
using Antlr4.Runtime;

[assembly: CLSCompliant(false)]

namespace DotnetValidation
{
    class Program
    {
        static void Main(string[] args)
        {
            Action<string> writeLine;

#if LEGACY_PCL
            var writeLineMethod = typeof(int).Assembly.GetType("Console").GetMethod("WriteLine", new[] { typeof(string) });
            writeLine = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), writeLineMethod);
#else
            writeLine = Console.WriteLine;
#endif

            var lexer1 = new global::DotnetValidation.GrammarLexer(new AntlrInputStream("keys"));
            var parser1 = new GrammarParser(new CommonTokenStream(lexer1));
            var tree1 = parser1.keys();
            writeLine(tree1.ToStringTree(parser1));

            var lexer2 = new global::DotnetValidation.GrammarLexer(new AntlrInputStream("values"));
            var parser2 = new GrammarParser(new CommonTokenStream(lexer2));
            var tree2 = parser2.values();
            writeLine(tree2.ToStringTree(parser2));

            var lexer = new global::DotnetValidation.GrammarLexer(new AntlrInputStream("text"));
            var parser = new GrammarParser(new CommonTokenStream(lexer));
            var tree = parser.compilationUnit();
            writeLine(tree1.ToStringTree(parser));

            var subLexer = new global::DotnetValidation.SubFolder.SubGrammarLexer(new AntlrInputStream("text"));
            var subParser = new SubFolder.SubGrammarParser(new CommonTokenStream(subLexer));
            var subTree = subParser.compilationUnit();
            writeLine(subTree.ToStringTree(subParser));
        }
    }
}
