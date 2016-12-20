/*
 * [The "BSD license"]
 *  Copyright (c) 2012 Terence Parr
 *  Copyright (c) 2012 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
