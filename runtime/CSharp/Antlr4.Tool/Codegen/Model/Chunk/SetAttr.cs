// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Chunk
{
    using System.Collections.Generic;
    using Antlr4.Codegen.Model.Decl;

    /** */
    public class SetAttr : ActionChunk
    {
        public string name;

        [ModelElement]
        public IList<ActionChunk> rhsChunks;

        public SetAttr(StructDecl ctx, string name, IList<ActionChunk> rhsChunks)
            : base(ctx)
        {
            this.name = name;
            this.rhsChunks = rhsChunks;
        }
    }
}
