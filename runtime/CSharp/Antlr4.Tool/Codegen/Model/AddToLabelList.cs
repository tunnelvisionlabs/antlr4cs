// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    /** */
    public class AddToLabelList : SrcOp
    {
        public Decl.Decl label;
        public string listName;

        public AddToLabelList(OutputModelFactory factory, string listName, Decl.Decl label)
            : base(factory)
        {
            this.label = label;
            this.listName = listName;
        }
    }
}
