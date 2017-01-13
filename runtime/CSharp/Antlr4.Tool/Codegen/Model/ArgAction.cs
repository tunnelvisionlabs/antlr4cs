// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool.Ast;

    public class ArgAction : Action
    {
        /** Context type of invoked rule */
        public string ctxType;

        public ArgAction(OutputModelFactory factory, ActionAST ast, string ctxType)
            : base(factory, ast)
        {
            this.ctxType = ctxType;
        }
    }
}
