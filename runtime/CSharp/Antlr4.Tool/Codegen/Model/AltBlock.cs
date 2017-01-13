// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;

    public class AltBlock : Choice
    {
        //	@ModelElement public ThrowNoViableAlt error;

        public AltBlock(OutputModelFactory factory,
                        GrammarAST blkOrEbnfRootAST,
                        IList<CodeBlockForAlt> alts)
            : base(factory, blkOrEbnfRootAST, alts)
        {
            decision = ((BlockStartState)blkOrEbnfRootAST.atnState).decision;
            // interp.predict() throws exception
            //		this.error = new ThrowNoViableAlt(factory, blkOrEbnfRootAST, null);
        }
    }
}
