// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Chunk
{
    using System.Collections.Generic;
    using Antlr4.Codegen.Model.Decl;

    public class SetNonLocalAttr : SetAttr
    {
        public string ruleName;
        public int ruleIndex;

        public SetNonLocalAttr(StructDecl ctx,
                               string ruleName, string name, int ruleIndex,
                               IList<ActionChunk> rhsChunks)
                : base(ctx, name, rhsChunks)
        {
            this.ruleName = ruleName;
            this.ruleIndex = ruleIndex;
        }
    }
}
