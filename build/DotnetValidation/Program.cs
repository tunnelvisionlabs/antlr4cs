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

            var lexer = new global::DotnetValidation.GrammarLexer(new AntlrInputStream("text"));
            var parser = new GrammarParser(new CommonTokenStream(lexer));
            var tree = parser.compilationUnit();
            writeLine(tree.ToStringTree(parser));

            var subLexer = new global::DotnetValidation.SubFolder.SubGrammarLexer(new AntlrInputStream("text"));
            var subParser = new SubFolder.SubGrammarParser(new CommonTokenStream(subLexer));
            var subTree = subParser.compilationUnit();
            writeLine(subTree.ToStringTree(subParser));
        }
    }

    class GrammarLexer : global::DotnetValidation.AbstractGrammarLexer
    {
        public GrammarLexer(ICharStream input)
            : base(input)
        {
        }
    }

    class GrammarParser : global::DotnetValidation.AbstractGrammarParser
    {
        public GrammarParser(ITokenStream input)
            : base(input)
        {
        }
    }
}
