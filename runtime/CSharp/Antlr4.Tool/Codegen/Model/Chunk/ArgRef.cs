// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Chunk
{
    using Antlr4.Codegen.Model.Decl;

    /** */
    public class ArgRef : LocalRef
    {
        public ArgRef(StructDecl ctx, string name)
            : base(ctx, name)
        {
        }
    }
}
