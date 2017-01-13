// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** (A B C)? */
    public class LL1OptionalBlockSingleAlt : LL1Choice
    {
        [ModelElement]
        public SrcOp expr;
        [ModelElement]
        public IList<SrcOp> followExpr; // might not work in template if size>1

        public LL1OptionalBlockSingleAlt(OutputModelFactory factory,
                                         GrammarAST blkAST,
                                         IList<CodeBlockForAlt> alts)
            : base(factory, blkAST, alts)
        {
            this.decision = ((DecisionState)blkAST.atnState).decision;

            /* Lookahead for each alt 1..n */
            //		IntervalSet[] altLookSets = LinearApproximator.getLL1LookaheadSets(dfa);
            IntervalSet[] altLookSets = factory.GetGrammar().decisionLOOK[decision];
            altLook = GetAltLookaheadAsStringLists(altLookSets);
            IntervalSet look = altLookSets[0];
            IntervalSet followLook = altLookSets[1];

            IntervalSet expecting = look.Or(followLook);
            this.error = GetThrowNoViableAlt(factory, blkAST, expecting);

            expr = AddCodeForLookaheadTempVar(look);
            followExpr = factory.GetLL1Test(followLook, blkAST);
        }
    }
}
