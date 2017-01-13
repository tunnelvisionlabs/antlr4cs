// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class LL1StarBlockSingleAlt : LL1Loop
    {
        public LL1StarBlockSingleAlt(OutputModelFactory factory, GrammarAST starRoot, IList<CodeBlockForAlt> alts)
            : base(factory, starRoot, alts)
        {

            StarLoopEntryState star = (StarLoopEntryState)starRoot.atnState;
            loopBackStateNumber = star.loopBackState.stateNumber;
            this.decision = star.decision;
            IntervalSet[] altLookSets = factory.GetGrammar().decisionLOOK[decision];
            Debug.Assert(altLookSets.Length == 2);
            IntervalSet enterLook = altLookSets[0];
            IntervalSet exitLook = altLookSets[1];
            loopExpr = AddCodeForLoopLookaheadTempVar(enterLook);
        }
    }
}
