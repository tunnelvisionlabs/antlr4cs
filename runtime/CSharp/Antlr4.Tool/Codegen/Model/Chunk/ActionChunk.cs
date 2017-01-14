// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Chunk
{
    using Antlr4.Codegen.Model.Decl;

    /** */
    public class ActionChunk : OutputModelObject
    {
        /** Where is the ctx that defines attrs,labels etc... for this action? */
        public StructDecl ctx;

        public ActionChunk(StructDecl ctx)
        {
            this.ctx = ctx;
        }
    }
}
