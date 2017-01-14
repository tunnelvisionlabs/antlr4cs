// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    public class ContextTokenListIndexedGetterDecl : ContextTokenListGetterDecl
    {
        public ContextTokenListIndexedGetterDecl(OutputModelFactory factory, string name)
            : base(factory, name)
        {
        }

        public override string GetArgType()
        {
            return "int";
        }
    }
}
