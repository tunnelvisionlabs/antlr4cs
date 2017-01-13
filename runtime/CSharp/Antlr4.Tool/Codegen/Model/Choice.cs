// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Misc;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** The class hierarchy underneath SrcOp is pretty deep but makes sense that,
     *  for example LL1StarBlock is a kind of LL1Loop which is a kind of Choice.
     *  The problem is it's impossible to figure
     *  out how to construct one of these deeply nested objects because of the
     *  long super constructor call chain. Instead, I decided to in-line all of
     *  this and then look for opportunities to re-factor code into functions.
     *  It makes sense to use a class hierarchy to share data fields, but I don't
     *  think it makes sense to factor code using super constructors because
     *  it has too much work to do.
     */
    public abstract class Choice : RuleElement
    {
        public int decision = -1;
        public Decl.Decl label;

        [ModelElement]
        public IList<CodeBlockForAlt> alts;
        [ModelElement]
        public IList<SrcOp> preamble = new List<SrcOp>();

        public Choice(OutputModelFactory factory,
                      GrammarAST blkOrEbnfRootAST,
                      IList<CodeBlockForAlt> alts)
            : base(factory, blkOrEbnfRootAST)
        {
            this.alts = alts;
        }

        public virtual void AddPreambleOp(SrcOp op)
        {
            preamble.Add(op);
        }

        public virtual IList<string[]> GetAltLookaheadAsStringLists(IntervalSet[] altLookSets)
        {
            IList<string[]> altLook = new List<string[]>();
            foreach (IntervalSet s in altLookSets)
            {
                altLook.Add(factory.GetTarget().GetTokenTypesAsTargetLabels(factory.GetGrammar(), s.ToArray()));
            }

            return altLook;
        }

        public virtual TestSetInline AddCodeForLookaheadTempVar(IntervalSet look)
        {
            IList<SrcOp> testOps = factory.GetLL1Test(look, ast);
            TestSetInline expr = Utils.Find<TestSetInline>(testOps);
            if (expr != null)
            {
                Decl.Decl d = new TokenTypeDecl(factory, expr.varName);
                factory.GetCurrentRuleFunction().AddLocalDecl(d);
                CaptureNextTokenType nextType = new CaptureNextTokenType(factory, expr.varName);
                AddPreambleOp(nextType);
            }
            return expr;
        }

        public virtual ThrowNoViableAlt GetThrowNoViableAlt(OutputModelFactory factory,
                                                    GrammarAST blkAST,
                                                    IntervalSet expecting)
        {
            return new ThrowNoViableAlt(factory, blkAST, expecting);
        }
    }
}
