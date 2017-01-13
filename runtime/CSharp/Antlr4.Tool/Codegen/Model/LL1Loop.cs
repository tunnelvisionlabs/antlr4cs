// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public abstract class LL1Loop : Choice
    {
        /** The state associated wih the (A|B|...) block not loopback, which
         *  is super.stateNumber
         */
        public int blockStartStateNumber;
        public int loopBackStateNumber;

        [ModelElement]
        public OutputModelObject loopExpr;
        [ModelElement]
        public IList<SrcOp> iteration;

        public LL1Loop(OutputModelFactory factory,
                       GrammarAST blkAST,
                       IList<CodeBlockForAlt> alts)
            : base(factory, blkAST, alts)
        {
        }

        public virtual void AddIterationOp(SrcOp op)
        {
            if (iteration == null)
                iteration = new List<SrcOp>();
            iteration.Add(op);
        }

        public virtual SrcOp AddCodeForLoopLookaheadTempVar(IntervalSet look)
        {
            TestSetInline expr = AddCodeForLookaheadTempVar(look);
            if (expr != null)
            {
                CaptureNextTokenType nextType = new CaptureNextTokenType(factory, expr.varName);
                AddIterationOp(nextType);
            }

            return expr;
        }
    }
}
