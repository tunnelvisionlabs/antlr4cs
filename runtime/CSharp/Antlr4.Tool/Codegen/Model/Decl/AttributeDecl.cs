// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    using Antlr4.Tool;

    /** */
    public class AttributeDecl : Decl
    {
        public AttributeDecl(OutputModelFactory factory, Attribute a)
            : base(factory, a.name, a.decl)
        {
            this.Type = a.type;
            this.InitValue = a.initValue;
        }

        public string Type
        {
            get;
        }

        public string InitValue
        {
            get;
        }
    }
}
