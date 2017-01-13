// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** (A | B | C) */
    public class LL1AltBlock : LL1Choice
    {
        public LL1AltBlock(OutputModelFactory factory, GrammarAST blkAST, IList<CodeBlockForAlt> alts)
            : base(factory, blkAST, alts)
        {
            this.decision = ((DecisionState)blkAST.atnState).decision;

            /* Lookahead for each alt 1..n */
            IntervalSet[] altLookSets = factory.GetGrammar().decisionLOOK[decision];
            altLook = GetAltLookaheadAsStringLists(altLookSets);

            IntervalSet expecting = IntervalSet.Or(altLookSets); // combine alt sets
            this.error = GetThrowNoViableAlt(factory, blkAST, expecting);
        }
    }
}
