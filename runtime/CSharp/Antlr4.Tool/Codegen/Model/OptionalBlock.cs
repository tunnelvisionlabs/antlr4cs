// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Tool.Ast;

    /** */
    public class OptionalBlock : AltBlock
    {
        public OptionalBlock(OutputModelFactory factory,
                             GrammarAST questionAST,
                             IList<CodeBlockForAlt> alts)
            : base(factory, questionAST, alts)
        {
        }
    }
}
