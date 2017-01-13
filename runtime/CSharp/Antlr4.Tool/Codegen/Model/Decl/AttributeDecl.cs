// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    using Antlr4.Tool;

    /** */
    public class AttributeDecl : Decl
    {
        public string type;
        public string initValue;

        public AttributeDecl(OutputModelFactory factory, Attribute a)
            : base(factory, a.name, a.decl)
        {
            this.type = a.type;
            this.initValue = a.initValue;
        }
    }
}
