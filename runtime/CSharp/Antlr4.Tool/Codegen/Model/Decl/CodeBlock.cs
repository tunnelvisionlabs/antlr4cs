// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    using System.Collections.Generic;
    using Antlr4.Misc;

    public class CodeBlock : SrcOp
    {
        public int codeBlockLevel;
        public int treeLevel;

        [ModelElement]
        public OrderedHashSet<Decl> locals;
        [ModelElement]
        public IList<SrcOp> preamble;
        [ModelElement]
        public IList<SrcOp> ops;

        public CodeBlock(OutputModelFactory factory)
            : base(factory)
        {
        }

        public CodeBlock(OutputModelFactory factory, int treeLevel, int codeBlockLevel)
            : base(factory)
        {
            this.treeLevel = treeLevel;
            this.codeBlockLevel = codeBlockLevel;
        }

        /** Add local var decl */
        public virtual void AddLocalDecl(Decl d)
        {
            if (locals == null)
                locals = new OrderedHashSet<Decl>();
            locals.Add(d);
            d.isLocal = true;
        }

        public virtual void AddPreambleOp(SrcOp op)
        {
            if (preamble == null)
                preamble = new List<SrcOp>();
            preamble.Add(op);
        }

        public virtual void AddOp(SrcOp op)
        {
            if (ops == null)
                ops = new List<SrcOp>();
            ops.Add(op);
        }

        public virtual void InsertOp(int i, SrcOp op)
        {
            if (ops == null)
                ops = new List<SrcOp>();
            ops.Insert(i, op);
        }

        public virtual void AddOps(IList<SrcOp> ops)
        {
            if (this.ops == null)
                this.ops = new List<SrcOp>();

            foreach (var op in ops)
                this.ops.Add(op);
        }
    }
}
