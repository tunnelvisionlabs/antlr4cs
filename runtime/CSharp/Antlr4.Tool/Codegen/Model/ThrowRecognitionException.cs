// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class ThrowRecognitionException : SrcOp
    {
        public int decision;
        public string grammarFile;
        public int grammarLine;
        public int grammarCharPosInLine;

        public ThrowRecognitionException(OutputModelFactory factory, GrammarAST ast, IntervalSet expecting)
            : base(factory, ast)
        {
            //this.decision = ((BlockStartState)ast.ATNState).decision;
            grammarLine = ast.Line;
            grammarLine = ast.CharPositionInLine;
            grammarFile = factory.GetGrammar().fileName;
            //this.expecting = factory.createExpectingBitSet(ast, decision, expecting, "error");
            //		factory.defineBitSet(this.expecting);
        }
    }
}
