namespace Antlr4.Runtime.Test
{
    using System;

    internal class LexerTestOptions
    {
        internal delegate TResult Factory<TArg, TResult>(TArg argument);

        public string TestName
        {
            get;
            set;
        }

        public Factory<ICharStream, Lexer> Lexer
        {
            get;
            set;
        }

        public string Input
        {
            get;
            set;
        }

        public string ExpectedOutput
        {
            get;
            set;
        }

        public string ExpectedErrors
        {
            get;
            set;
        }

        public bool ShowDFA
        {
            get;
            set;
        }
    }
}
