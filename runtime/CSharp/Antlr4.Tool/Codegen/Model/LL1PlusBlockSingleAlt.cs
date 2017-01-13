// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class LL1PlusBlockSingleAlt : LL1Loop
    {
        public LL1PlusBlockSingleAlt(OutputModelFactory factory, GrammarAST plusRoot, IList<CodeBlockForAlt> alts)
            : base(factory, plusRoot, alts)
        {
            BlockAST blkAST = (BlockAST)plusRoot.GetChild(0);
            PlusBlockStartState blkStart = (PlusBlockStartState)blkAST.atnState;

            stateNumber = blkStart.loopBackState.stateNumber;
            blockStartStateNumber = blkStart.stateNumber;
            PlusBlockStartState plus = (PlusBlockStartState)blkAST.atnState;
            this.decision = plus.loopBackState.decision;
            IntervalSet[] altLookSets = factory.GetGrammar().decisionLOOK[decision];

            IntervalSet loopBackLook = altLookSets[0];
            loopExpr = AddCodeForLoopLookaheadTempVar(loopBackLook);
        }
    }
}
