// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class Sync : SrcOp
    {
        public int decision;
        //	public BitSetDecl expecting;

        public Sync(OutputModelFactory factory,
                    GrammarAST blkOrEbnfRootAST,
                    IntervalSet expecting,
                    int decision,
                    string position)
            : base(factory, blkOrEbnfRootAST)
        {
            this.decision = decision;
            //		this.expecting = factory.createExpectingBitSet(ast, decision, expecting, position);
            //		factory.defineBitSet(this.expecting);
        }
    }
}
