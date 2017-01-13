// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;

    public class StarBlock : Loop
    {
        public string loopLabel;

        public StarBlock(OutputModelFactory factory,
                         GrammarAST blkOrEbnfRootAST,
                         IList<CodeBlockForAlt> alts)
            : base(factory, blkOrEbnfRootAST, alts)
        {
            loopLabel = factory.GetTarget().GetLoopLabel(blkOrEbnfRootAST);
            StarLoopEntryState star = (StarLoopEntryState)blkOrEbnfRootAST.atnState;
            loopBackStateNumber = star.loopBackState.stateNumber;
            decision = star.decision;
        }
    }
}
