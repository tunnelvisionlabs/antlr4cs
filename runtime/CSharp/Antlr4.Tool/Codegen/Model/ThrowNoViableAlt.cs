// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class ThrowNoViableAlt : ThrowRecognitionException
    {
        public ThrowNoViableAlt(OutputModelFactory factory, GrammarAST blkOrEbnfRootAST,
                                IntervalSet expecting)
            : base(factory, blkOrEbnfRootAST, expecting)
        {
        }
    }
}
