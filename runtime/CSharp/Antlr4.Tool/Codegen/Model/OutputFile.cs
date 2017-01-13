// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool;

    public abstract class OutputFile : OutputModelObject
    {
        public readonly string fileName;
        public readonly string grammarFileName;
        public readonly string ANTLRVersion;
        public readonly string TokenLabelType;
        public readonly string InputSymbolType;

        protected OutputFile(OutputModelFactory factory, string fileName)
            : base(factory)
        {
            this.fileName = fileName;
            Grammar g = factory.GetGrammar();
            grammarFileName = g.fileName;
            ANTLRVersion = AntlrTool.VERSION;
            TokenLabelType = g.GetOptionString("TokenLabelType");
            InputSymbolType = TokenLabelType;
        }
    }
}
