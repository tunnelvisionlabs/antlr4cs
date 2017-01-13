// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;

    public class PlusBlock : Loop
    {
        [ModelElement]
        public ThrowNoViableAlt error;

        public PlusBlock(OutputModelFactory factory,
                         GrammarAST plusRoot,
                         IList<CodeBlockForAlt> alts)
            : base(factory, plusRoot, alts)
        {
            BlockAST blkAST = (BlockAST)plusRoot.GetChild(0);
            PlusBlockStartState blkStart = (PlusBlockStartState)blkAST.atnState;
            PlusLoopbackState loop = blkStart.loopBackState;
            stateNumber = blkStart.loopBackState.stateNumber;
            blockStartStateNumber = blkStart.stateNumber;
            loopBackStateNumber = loop.stateNumber;
            this.error = GetThrowNoViableAlt(factory, plusRoot, null);
            decision = loop.decision;
        }
    }
}
