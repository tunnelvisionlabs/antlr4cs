// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool.Ast;

    /** */
    public abstract class OutputModelObject
    {
        public OutputModelFactory factory;
        public GrammarAST ast;

        protected OutputModelObject()
        {
        }

        protected OutputModelObject(OutputModelFactory factory)
            : this(factory, null)
        {
        }

        protected OutputModelObject(OutputModelFactory factory, GrammarAST ast)
        {
            this.factory = factory;
            this.ast = ast;
        }
    }
}
