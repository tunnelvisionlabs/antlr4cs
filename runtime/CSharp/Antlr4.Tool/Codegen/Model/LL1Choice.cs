// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Tool.Ast;

    public abstract class LL1Choice : Choice
    {
        /** Token names for each alt 0..n-1 */
        public IList<string[]> altLook;
        [ModelElement]
        public ThrowNoViableAlt error;

        public LL1Choice(OutputModelFactory factory, GrammarAST blkAST,
                         IList<CodeBlockForAlt> alts)
            : base(factory, blkAST, alts)
        {
        }
    }
}
