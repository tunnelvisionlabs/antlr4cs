// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    public class RuleContextListDecl : RuleContextDecl
    {
        public RuleContextListDecl(OutputModelFactory factory, string name, string ctxName)
            : base(factory, name, ctxName)
        {
            isImplicit = false;
        }
    }
}
