// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Codegen.Model.Decl;

    /** Contains Rewrite block (usually as last op) */
    public class CodeBlockForAlt : CodeBlock
    {
        public CodeBlockForAlt(OutputModelFactory factory)
            : base(factory)
        {
        }
    }
}
