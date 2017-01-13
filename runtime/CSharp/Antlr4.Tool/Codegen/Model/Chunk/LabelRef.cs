// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Chunk
{
    using Antlr4.Codegen.Model.Decl;

    public class LabelRef : ActionChunk
    {
        public string name;

        public LabelRef(StructDecl ctx, string name)
            : base(ctx)
        {
            this.name = name;
        }
    }
}
