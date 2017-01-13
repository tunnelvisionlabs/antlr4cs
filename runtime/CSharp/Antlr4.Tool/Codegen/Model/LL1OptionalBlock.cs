// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Tool.Ast;

    /** An optional block is just an alternative block where the last alternative
     *  is epsilon. The analysis takes care of adding to the empty alternative.
     *
     *  (A | B | C)?
     */
    public class LL1OptionalBlock : LL1AltBlock
    {
        public LL1OptionalBlock(OutputModelFactory factory, GrammarAST blkAST, IList<CodeBlockForAlt> alts)
            : base(factory, blkAST, alts)
        {
        }
    }
}
