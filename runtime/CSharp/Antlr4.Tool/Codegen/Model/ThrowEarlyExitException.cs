// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class ThrowEarlyExitException : ThrowRecognitionException
    {
        public ThrowEarlyExitException(OutputModelFactory factory, GrammarAST ast, IntervalSet expecting)
            : base(factory, ast, expecting)
        {
        }
    }
}
