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

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using Antlr4.Analysis;
    using Antlr4.Codegen.Model;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Parse;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class ParserFactory : DefaultOutputModelFactory
    {
        public ParserFactory(CodeGenerator gen)
            : base(gen)
        {
        }

        public override ParserFile ParserFile(string fileName)
        {
            return new ParserFile(this, fileName);
        }

        public override Parser Parser(ParserFile file)
        {
            return new Parser(this, file);
        }

        public override RuleFunction Rule(Rule r)
        {
            if (r is LeftRecursiveRule)
            {
                return new LeftRecursiveRuleFunction(this, (LeftRecursiveRule)r);
            }
            else if (r.name.Contains(ATNSimulator.RuleLfVariantMarker))
            {
                return new LeftFactoredRuleFunction(this, r);
            }
            else if (r.name.Contains(ATNSimulator.RuleNolfVariantMarker))
            {
                return new LeftUnfactoredRuleFunction(this, r);
            }
            else
            {
                RuleFunction rf = new RuleFunction(this, r);
                return rf;
            }
        }

        public override CodeBlockForAlt Epsilon(Alternative alt, bool outerMost)
        {
            return Alternative(alt, outerMost);
        }

        public override CodeBlockForAlt Alternative(Alternative alt, bool outerMost)
        {
            if (outerMost)
                return new CodeBlockForOuterMostAlt(this, alt);
            return new CodeBlockForAlt(this);
        }

        public override CodeBlockForAlt FinishAlternative(CodeBlockForAlt blk, IList<SrcOp> ops)
        {
            blk.ops = ops;
            return blk;
        }

        public override IList<SrcOp> Action(ActionAST ast)
        {
            return List(new Action(this, ast));
        }

        public override IList<SrcOp> Sempred(ActionAST ast)
        {
            return List(new SemPred(this, ast));
        }

        public override IList<SrcOp> RuleRef(GrammarAST ID, GrammarAST label, GrammarAST args)
        {
            InvokeRule invokeOp = new InvokeRule(this, ID, label);
            // If no manual label and action refs as token/rule not label, we need to define implicit label
            if (controller.NeedsImplicitLabel(ID, invokeOp))
                DefineImplicitLabel(ID, invokeOp);
            AddToLabelList listLabelOp = GetAddToListOpIfListLabelPresent(invokeOp, label);
            return List(invokeOp, listLabelOp);
        }

        public override IList<SrcOp> TokenRef(GrammarAST ID, GrammarAST labelAST, GrammarAST args)
        {
            MatchToken matchOp = new MatchToken(this, (TerminalAST)ID);
            if (labelAST != null)
            {
                string label = labelAST.Text;
                RuleFunction rf = GetCurrentRuleFunction();
                if (labelAST.Parent.Type == ANTLRParser.PLUS_ASSIGN)
                {
                    // add Token _X and List<Token> X decls
                    DefineImplicitLabel(ID, matchOp); // adds _X
                    TokenListDecl l = GetTokenListLabelDecl(label);
                    rf.AddContextDecl(ID.GetAltLabel(), l);
                }
                else
                {
                    Decl d = GetTokenLabelDecl(label);
                    matchOp.labels.Add(d);
                    rf.AddContextDecl(ID.GetAltLabel(), d);
                }

                //			Decl d = getTokenLabelDecl(label);
                //			((MatchToken)matchOp).labels.add(d);
                //			getCurrentRuleFunction().addContextDecl(ID.getAltLabel(), d);
                //			if ( labelAST.parent.getType() == ANTLRParser.PLUS_ASSIGN ) {
                //				TokenListDecl l = getTokenListLabelDecl(label);
                //				getCurrentRuleFunction().addContextDecl(ID.getAltLabel(), l);
                //			}
            }
            if (controller.NeedsImplicitLabel(ID, matchOp))
                DefineImplicitLabel(ID, matchOp);
            AddToLabelList listLabelOp = GetAddToListOpIfListLabelPresent(matchOp, labelAST);
            return List(matchOp, listLabelOp);
        }

        public virtual Decl GetTokenLabelDecl(string label)
        {
            return new TokenDecl(this, label);
        }

        public virtual TokenListDecl GetTokenListLabelDecl(string label)
        {
            return new TokenListDecl(this, GetTarget().GetListLabel(label));
        }

        public override IList<SrcOp> Set(GrammarAST setAST, GrammarAST labelAST, bool invert)
        {
            MatchSet matchOp;
            if (invert)
                matchOp = new MatchNotSet(this, setAST);
            else
                matchOp = new MatchSet(this, setAST);
            if (labelAST != null)
            {
                string label = labelAST.Text;
                RuleFunction rf = GetCurrentRuleFunction();
                if (labelAST.Parent.Type == ANTLRParser.PLUS_ASSIGN)
                {
                    DefineImplicitLabel(setAST, matchOp);
                    TokenListDecl l = GetTokenListLabelDecl(label);
                    rf.AddContextDecl(setAST.GetAltLabel(), l);
                }
                else
                {
                    Decl d = GetTokenLabelDecl(label);
                    matchOp.labels.Add(d);
                    rf.AddContextDecl(setAST.GetAltLabel(), d);
                }
            }
            if (controller.NeedsImplicitLabel(setAST, matchOp))
                DefineImplicitLabel(setAST, matchOp);
            AddToLabelList listLabelOp = GetAddToListOpIfListLabelPresent(matchOp, labelAST);
            return List(matchOp, listLabelOp);
        }

        public override IList<SrcOp> Wildcard(GrammarAST ast, GrammarAST labelAST)
        {
            Wildcard wild = new Wildcard(this, ast);
            // TODO: dup with tokenRef
            if (labelAST != null)
            {
                string label = labelAST.Text;
                Decl d = GetTokenLabelDecl(label);
                wild.labels.Add(d);
                GetCurrentRuleFunction().AddContextDecl(ast.GetAltLabel(), d);
                if (labelAST.Parent.Type == ANTLRParser.PLUS_ASSIGN)
                {
                    TokenListDecl l = GetTokenListLabelDecl(label);
                    GetCurrentRuleFunction().AddContextDecl(ast.GetAltLabel(), l);
                }
            }
            if (controller.NeedsImplicitLabel(ast, wild))
                DefineImplicitLabel(ast, wild);
            AddToLabelList listLabelOp = GetAddToListOpIfListLabelPresent(wild, labelAST);
            return List(wild, listLabelOp);
        }

        public override Choice GetChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts, GrammarAST labelAST)
        {
            int decision = ((DecisionState)blkAST.atnState).decision;
            Choice c;
            if (!g.tool.force_atn && AnalysisPipeline.Disjoint(g.decisionLOOK[decision]))
            {
                c = GetLL1ChoiceBlock(blkAST, alts);
            }
            else
            {
                c = GetComplexChoiceBlock(blkAST, alts);
            }

            if (labelAST != null)
            { // for x=(...), define x or x_list
                string label = labelAST.Text;
                Decl d = GetTokenLabelDecl(label);
                c.label = d;
                GetCurrentRuleFunction().AddContextDecl(labelAST.GetAltLabel(), d);
                if (labelAST.Parent.Type == ANTLRParser.PLUS_ASSIGN)
                {
                    string listLabel = GetTarget().GetListLabel(label);
                    TokenListDecl l = new TokenListDecl(this, listLabel);
                    GetCurrentRuleFunction().AddContextDecl(labelAST.GetAltLabel(), l);
                }
            }

            return c;
        }

        public override Choice GetEBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts)
        {
            if (!g.tool.force_atn)
            {
                int decision;
                if (ebnfRoot.Type == ANTLRParser.POSITIVE_CLOSURE)
                {
                    decision = ((PlusLoopbackState)ebnfRoot.atnState).decision;
                }
                else if (ebnfRoot.Type == ANTLRParser.CLOSURE)
                {
                    decision = ((StarLoopEntryState)ebnfRoot.atnState).decision;
                }
                else
                {
                    decision = ((DecisionState)ebnfRoot.atnState).decision;
                }

                if (AnalysisPipeline.Disjoint(g.decisionLOOK[decision]))
                {
                    return GetLL1EBNFBlock(ebnfRoot, alts);
                }
            }

            return GetComplexEBNFBlock(ebnfRoot, alts);
        }

        public override Choice GetLL1ChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts)
        {
            return new LL1AltBlock(this, blkAST, alts);
        }

        public override Choice GetComplexChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts)
        {
            return new AltBlock(this, blkAST, alts);
        }

        public override Choice GetLL1EBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts)
        {
            int ebnf = 0;
            if (ebnfRoot != null)
                ebnf = ebnfRoot.Type;
            Choice c = null;
            switch (ebnf)
            {
            case ANTLRParser.OPTIONAL:
                if (alts.Count == 1)
                    c = new LL1OptionalBlockSingleAlt(this, ebnfRoot, alts);
                else
                    c = new LL1OptionalBlock(this, ebnfRoot, alts);
                break;
            case ANTLRParser.CLOSURE:
                if (alts.Count == 1)
                    c = new LL1StarBlockSingleAlt(this, ebnfRoot, alts);
                else
                    c = GetComplexEBNFBlock(ebnfRoot, alts);
                break;
            case ANTLRParser.POSITIVE_CLOSURE:
                if (alts.Count == 1)
                    c = new LL1PlusBlockSingleAlt(this, ebnfRoot, alts);
                else
                    c = GetComplexEBNFBlock(ebnfRoot, alts);
                break;
            }
            return c;
        }

        public override Choice GetComplexEBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts)
        {
            int ebnf = 0;
            if (ebnfRoot != null)
                ebnf = ebnfRoot.Type;
            Choice c = null;
            switch (ebnf)
            {
            case ANTLRParser.OPTIONAL:
                c = new OptionalBlock(this, ebnfRoot, alts);
                break;
            case ANTLRParser.CLOSURE:
                c = new StarBlock(this, ebnfRoot, alts);
                break;
            case ANTLRParser.POSITIVE_CLOSURE:
                c = new PlusBlock(this, ebnfRoot, alts);
                break;
            }
            return c;
        }

        public override IList<SrcOp> GetLL1Test(IntervalSet look, GrammarAST blkAST)
        {
            return List(new TestSetInline(this, blkAST, look, gen.GetTarget().GetInlineTestSetWordSize()));
        }

        public override bool NeedsImplicitLabel(GrammarAST ID, LabeledOp op)
        {
            Alternative currentOuterMostAlt = GetCurrentOuterMostAlt();
            bool actionRefsAsToken = currentOuterMostAlt.tokenRefsInActions.ContainsKey(ID.Text);
            bool actionRefsAsRule = currentOuterMostAlt.ruleRefsInActions.ContainsKey(ID.Text);
            return op.GetLabels().Count == 0 && (actionRefsAsToken || actionRefsAsRule);
        }

        // support

        public virtual void DefineImplicitLabel(GrammarAST ast, LabeledOp op)
        {
            Decl d;
            if (ast.Type == ANTLRParser.SET || ast.Type == ANTLRParser.WILDCARD)
            {
                string implLabel =
                    GetTarget().GetImplicitSetLabel(ast.Token.TokenIndex.ToString());
                d = GetTokenLabelDecl(implLabel);
                ((TokenDecl)d).isImplicit = true;
            }
            else if (ast.Type == ANTLRParser.RULE_REF)
            { // a rule reference?
                Rule r = g.GetRule(ast.Text);
                string implLabel = GetTarget().GetImplicitRuleLabel(ast.Text);
                string ctxName =
                    GetTarget().GetRuleFunctionContextStructName(r);
                d = new RuleContextDecl(this, implLabel, ctxName);
                ((RuleContextDecl)d).isImplicit = true;
            }
            else
            {
                string implLabel = GetTarget().GetImplicitTokenLabel(ast.Text);
                d = GetTokenLabelDecl(implLabel);
                ((TokenDecl)d).isImplicit = true;
            }
            op.GetLabels().Add(d);
            // all labels must be in scope struct in case we exec action out of context
            GetCurrentRuleFunction().AddContextDecl(ast.GetAltLabel(), d);
        }

        public virtual AddToLabelList GetAddToListOpIfListLabelPresent(LabeledOp op, GrammarAST label)
        {
            AddToLabelList labelOp = null;
            if (label != null && label.Parent.Type == ANTLRParser.PLUS_ASSIGN)
            {
                string listLabel = GetTarget().GetListLabel(label.Text);
                labelOp = new AddToLabelList(this, listLabel, op.GetLabels()[0]);
            }
            return labelOp;
        }
    }
}
