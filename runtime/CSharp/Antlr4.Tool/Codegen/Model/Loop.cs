// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Tool.Ast;

    public class Loop : Choice
    {
        public int blockStartStateNumber;
        public int loopBackStateNumber;
        public readonly int exitAlt;

        [ModelElement]
        public IList<SrcOp> iteration;

        public Loop(OutputModelFactory factory,
                    GrammarAST blkOrEbnfRootAST,
                    IList<CodeBlockForAlt> alts)
            : base(factory, blkOrEbnfRootAST, alts)
        {
            bool nongreedy = (blkOrEbnfRootAST is QuantifierAST) && !((QuantifierAST)blkOrEbnfRootAST).GetGreedy();
            exitAlt = nongreedy ? 1 : alts.Count + 1;
        }

        public virtual void AddIterationOp(SrcOp op)
        {
            if (iteration == null)
                iteration = new List<SrcOp>();
            iteration.Add(op);
        }
    }
}
