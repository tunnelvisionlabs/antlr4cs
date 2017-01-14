// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Chunk
{
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.StringTemplate;

    public class ActionTemplate : ActionChunk
    {
        public Template st;

        public ActionTemplate(StructDecl ctx, Template st)
            : base(ctx)
        {
            this.st = st;
        }
    }
}
