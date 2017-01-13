// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool;

    /** The code associated with the outermost alternative of a rule.
     *  Sometimes we might want to treat them differently in the
     *  code generation.
     */
    public class CodeBlockForOuterMostAlt : CodeBlockForAlt
    {
        /**
         * The label for the alternative; or null if the alternative is not labeled.
         */
        public string altLabel;
        /**
         * The alternative.
         */
        public Alternative alt;

        public CodeBlockForOuterMostAlt(OutputModelFactory factory, Alternative alt)
            : base(factory)
        {
            this.alt = alt;
            altLabel = alt.ast.altLabel != null ? alt.ast.altLabel.Text : null;
        }
    }
}
