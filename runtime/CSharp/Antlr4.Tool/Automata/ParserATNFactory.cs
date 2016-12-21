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

namespace Antlr4.Automata
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Antlr4.Analysis;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Runtime.Atn;
    using Antlr4.Semantics;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using ArgumentNullException = System.ArgumentNullException;
    using CommonTreeNodeStream = Antlr.Runtime.Tree.CommonTreeNodeStream;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;
    using InvalidOperationException = System.InvalidOperationException;
    using ITree = Antlr.Runtime.Tree.ITree;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using RecognitionException = Antlr.Runtime.RecognitionException;
    using StringComparison = System.StringComparison;
    using TokenConstants = Antlr4.Runtime.TokenConstants;
    using Tuple = System.Tuple;

    /** ATN construction routines triggered by ATNBuilder.g.
     *
     *  No side-effects. It builds an {@link ATN} object and returns it.
     */
    public class ParserATNFactory : ATNFactory {
        [NotNull]
        public readonly Grammar g;

        [NotNull]
        public readonly ATN atn;

        public Rule currentRule;

        public int currentOuterAlt;

        [NotNull]
        protected readonly IList<System.Tuple<Rule, ATNState, ATNState>> preventEpsilonClosureBlocks =
            new List<System.Tuple<Rule, ATNState, ATNState>>();

        [NotNull]
        protected readonly IList<System.Tuple<Rule, ATNState, ATNState>> preventEpsilonOptionalBlocks =
            new List<System.Tuple<Rule, ATNState, ATNState>>();

        public ParserATNFactory([NotNull] Grammar g) {
            if (g == null) {
                throw new ArgumentNullException(nameof(g));
            }

            this.g = g;

            ATNType atnType = g is LexerGrammar ? ATNType.Lexer : ATNType.Parser;
            int maxTokenType = g.GetMaxTokenType();
            this.atn = new ATN(atnType, maxTokenType);
        }

        [return: NotNull]
        public virtual ATN CreateATN() {
            _CreateATN(g.rules.Values);
            Debug.Assert(atn.maxTokenType == g.GetMaxTokenType());
            AddRuleFollowLinks();
            AddEOFTransitionToStartRules();
            ATNOptimizer.Optimize(g, atn);

            foreach (System.Tuple<Rule, ATNState, ATNState> pair in preventEpsilonClosureBlocks) {
                LL1Analyzer analyzer = new LL1Analyzer(atn);
                if (analyzer.Look(pair.Item2, pair.Item3, PredictionContext.EmptyLocal).Contains(Antlr4.Runtime.TokenConstants.Epsilon)) {
                    ErrorType errorType = pair.Item1 is LeftRecursiveRule ? ErrorType.EPSILON_LR_FOLLOW : ErrorType.EPSILON_CLOSURE;
                    g.tool.errMgr.GrammarError(errorType, g.fileName, ((GrammarAST)pair.Item1.ast.GetChild(0)).Token, pair.Item1.name);
                }
            }

            foreach (System.Tuple<Rule, ATNState, ATNState> pair in preventEpsilonOptionalBlocks) {
                int bypassCount = 0;
                for (int i = 0; i < pair.Item2.NumberOfTransitions; i++) {
                    ATNState startState = pair.Item2.Transition(i).target;
                    if (startState == pair.Item3) {
                        bypassCount++;
                        continue;
                    }

                    LL1Analyzer analyzer = new LL1Analyzer(atn);
                    if (analyzer.Look(startState, pair.Item3, PredictionContext.EmptyLocal).Contains(Antlr4.Runtime.TokenConstants.Epsilon)) {
                        g.tool.errMgr.GrammarError(ErrorType.EPSILON_OPTIONAL, g.fileName, ((GrammarAST)pair.Item1.ast.GetChild(0)).Token, pair.Item1.name);
                        goto continueOptionalCheck;
                    }
                }

                if (bypassCount != 1) {
                    throw new InvalidOperationException("Expected optional block with exactly 1 bypass alternative.");
                }

                continueOptionalCheck:
                ;
            }

            return atn;
        }

        protected virtual void _CreateATN([NotNull] ICollection<Rule> rules) {
            CreateRuleStartAndStopATNStates();

            GrammarASTAdaptor adaptor = new GrammarASTAdaptor();
            foreach (Rule r in rules) {
                // find rule's block
                GrammarAST blk = (GrammarAST)r.ast.GetFirstChildWithType(ANTLRParser.BLOCK);
                CommonTreeNodeStream nodes = new CommonTreeNodeStream(adaptor, blk);
                ATNBuilder b = new ATNBuilder(nodes, this);
                try {
                    SetCurrentRuleName(r.name);
                    Handle h = b.ruleBlock(null);
                    Rule(r.ast, r.name, h);
                }
                catch (RecognitionException re) {
                    ErrorManager.FatalInternalError("bad grammar AST structure", re);
                }
            }
        }

        public virtual void SetCurrentRuleName([NotNull] string name) {
            this.currentRule = g.GetRule(name);
        }

        public virtual void SetCurrentOuterAlt(int alt) {
            currentOuterAlt = alt;
        }

        /* start->ruleblock->end */
        [return: NotNull]
        public virtual Handle Rule([NotNull] GrammarAST ruleAST, [NotNull] string name, [NotNull] Handle blk) {
            Rule r = g.GetRule(name);
            RuleStartState start = atn.ruleToStartState[r.index];
            Epsilon(start, blk.left);
            RuleStopState stop = atn.ruleToStopState[r.index];
            Epsilon(blk.right, stop);
            Handle h = new Handle(start, stop);
            //ATNPrinter ser = new ATNPrinter(g, h.left);
            //System.Console.WriteLine(ruleAST.ToStringTree() + ":\n" + ser.AsString());
            ruleAST.atnState = start;
            return h;
        }

        /** From label {@code A} build graph {@code o-A->o}. */
        [return: NotNull]
        public virtual Handle TokenRef([NotNull] TerminalAST node) {
            ATNState left = NewState(node);
            ATNState right = NewState(node);
            int ttype = g.GetTokenType(node.Text);
            left.AddTransition(new AtomTransition(right, ttype));
            node.atnState = left;
            return new Handle(left, right);
        }

        /** From set build single edge graph {@code o->o-set->o}.  To conform to
         *  what an alt block looks like, must have extra state on left.
         *  This also handles {@code ~A}, converted to {@code ~{A}} set.
         */
        [return: NotNull]
        public virtual Handle Set([NotNull] GrammarAST associatedAST, [NotNull] IList<GrammarAST> terminals, bool invert) {
            ATNState left = NewState(associatedAST);
            ATNState right = NewState(associatedAST);
            IntervalSet set = new IntervalSet();
            foreach (GrammarAST t in terminals) {
                int ttype = g.GetTokenType(t.Text);
                set.Add(ttype);
            }
            if (invert) {
                left.AddTransition(new NotSetTransition(right, set));
            }
            else {
                left.AddTransition(new SetTransition(right, set));
            }
            associatedAST.atnState = left;
            return new Handle(left, right);
        }

        /** Not valid for non-lexers. */
        [return: NotNull]
        public virtual Handle Range([NotNull] GrammarAST a, [NotNull] GrammarAST b) {
            throw new InvalidOperationException("This construct is not valid in parsers.");
        }

        protected virtual int GetTokenType([NotNull] GrammarAST atom) {
            int ttype;
            if (g.IsLexer()) {
                ttype = CharSupport.GetCharValueFromGrammarCharLiteral(atom.Text);
            }
            else {
                ttype = g.GetTokenType(atom.Text);
            }
            return ttype;
        }

        /** For a non-lexer, just build a simple token reference atom. */
        [return: NotNull]
        public virtual Handle StringLiteral([NotNull] TerminalAST stringLiteralAST) {
            return TokenRef(stringLiteralAST);
        }

        /** {@code [Aa]} char sets not allowed in parser */
        [return: NotNull]
        public virtual Handle CharSetLiteral([NotNull] GrammarAST charSetAST) {
            return null;
        }

        /**
         * For reference to rule {@code r}, build
         *
         * <pre>
         *  o-&gt;(r)  o
         * </pre>
         *
         * where {@code (r)} is the start of rule {@code r} and the trailing
         * {@code o} is not linked to from rule ref state directly (uses
         * {@link RuleTransition#followState}).
         */
        [return: NotNull]
        public virtual Handle RuleRef([NotNull] GrammarAST node) {
            Handle h = _RuleRef(node);
            return h;
        }

        [return: NotNull]
        public virtual Handle _RuleRef([NotNull] GrammarAST node) {
            Rule r = g.GetRule(node.Text);
            if (r == null) {
                g.tool.errMgr.GrammarError(ErrorType.INTERNAL_ERROR, g.fileName, node.Token, "Rule " + node.Text + " undefined");
                return null;
            }
            RuleStartState start = atn.ruleToStartState[r.index];
            ATNState left = NewState(node);
            ATNState right = NewState(node);
            int precedence = 0;
            if (((GrammarASTWithOptions)node).GetOptionString(LeftRecursiveRuleTransformer.PRECEDENCE_OPTION_NAME) != null) {
                precedence = int.Parse(((GrammarASTWithOptions)node).GetOptionString(LeftRecursiveRuleTransformer.PRECEDENCE_OPTION_NAME));
            }
            RuleTransition call = new RuleTransition(start, r.index, precedence, right);
            left.AddTransition(call);

            node.atnState = left;
            return new Handle(left, right);
        }

        public virtual void AddFollowLink(int ruleIndex, ATNState right) {
            // add follow edge from end of invoked rule
            RuleStopState stop = atn.ruleToStopState[ruleIndex];
            //        System.out.println("add follow link from "+ruleIndex+" to "+right);
            Epsilon(stop, right);
        }

        /** From an empty alternative build {@code o-e->o}. */
        [return: NotNull]
        public virtual Handle Epsilon([NotNull] GrammarAST node) {
            ATNState left = NewState(node);
            ATNState right = NewState(node);
            Epsilon(left, right);
            node.atnState = left;
            return new Handle(left, right);
        }

        /** Build what amounts to an epsilon transition with a semantic
         *  predicate action.  The {@code pred} is a pointer into the AST of
         *  the {@link ANTLRParser#SEMPRED} token.
         */
        [return: NotNull]
        public virtual Handle Sempred([NotNull] PredAST pred) {
            //System.out.println("sempred: "+ pred);
            ATNState left = NewState(pred);
            ATNState right = NewState(pred);

            AbstractPredicateTransition p;
            if (pred.GetOptionString(LeftRecursiveRuleTransformer.PRECEDENCE_OPTION_NAME) != null) {
                int precedence = int.Parse(pred.GetOptionString(LeftRecursiveRuleTransformer.PRECEDENCE_OPTION_NAME));
                p = new PrecedencePredicateTransition(right, precedence);
            }
            else {
                bool isCtxDependent = UseDefAnalyzer.ActionIsContextDependent(pred);
                p = new PredicateTransition(right, currentRule.index, g.sempreds[pred], isCtxDependent);
            }

            left.AddTransition(p);
            pred.atnState = left;
            return new Handle(left, right);
        }

        /** Build what amounts to an epsilon transition with an action.
         *  The action goes into ATN though it is ignored during prediction
         *  if {@link ActionTransition#actionIndex actionIndex}{@code &lt;0}.
         */
        [return: NotNull]
        public virtual Handle Action([NotNull] ActionAST action) {
            //System.out.println("action: "+action);
            ATNState left = NewState(action);
            ATNState right = NewState(action);
            ActionTransition a = new ActionTransition(right, currentRule.index);
            left.AddTransition(a);
            action.atnState = left;
            return new Handle(left, right);
        }

        [return: NotNull]
        public virtual Handle Action([NotNull] string action) {
            throw new InvalidOperationException("This element is not valid in parsers.");
        }

        /**
         * From {@code A|B|..|Z} alternative block build
         *
         * <pre>
         *  o-&gt;o-A-&gt;o-&gt;o (last ATNState is BlockEndState pointed to by all alts)
         *  |          ^
         *  |-&gt;o-B-&gt;o--|
         *  |          |
         *  ...        |
         *  |          |
         *  |-&gt;o-Z-&gt;o--|
         * </pre>
         *
         * So start node points at every alternative with epsilon transition and
         * every alt right side points at a block end ATNState.
         * <p>
         * Special case: only one alternative: don't make a block with alt
         * begin/end.</p>
         * <p>
         * Special case: if just a list of tokens/chars/sets, then collapse to a
         * single edged o-set-&gt;o graph.</p>
         * <p>
         * TODO: Set alt number (1..n) in the states?</p>
         */
        [return: NotNull]
        public virtual Handle Block([NotNull] BlockAST blkAST, [NotNull] GrammarAST ebnfRoot, [NotNull] IList<Handle> alts) {
            if (ebnfRoot == null) {
                if (alts.Count == 1) {
                    Handle h = alts[0];
                    blkAST.atnState = h.left;
                    return h;
                }
                BlockStartState start = NewState<BasicBlockStartState>(blkAST);
                if (alts.Count > 1) atn.DefineDecisionState(start);
                return MakeBlock(start, blkAST, alts);
            }
            switch (ebnfRoot.Type) {
            case ANTLRParser.OPTIONAL:
                BlockStartState start = NewState<BasicBlockStartState>(blkAST);
                atn.DefineDecisionState(start);
                Handle h = MakeBlock(start, blkAST, alts);
                return Optional(ebnfRoot, h);
            case ANTLRParser.CLOSURE:
                BlockStartState star = NewState<StarBlockStartState>(ebnfRoot);
                if (alts.Count > 1) atn.DefineDecisionState(star);
                h = MakeBlock(star, blkAST, alts);
                return Star(ebnfRoot, h);
            case ANTLRParser.POSITIVE_CLOSURE:
                PlusBlockStartState plus = NewState<PlusBlockStartState>(ebnfRoot);
                if (alts.Count > 1) atn.DefineDecisionState(plus);
                h = MakeBlock(plus, blkAST, alts);
                return Plus(ebnfRoot, h);
            }
            return null;
        }

        [return: NotNull]
        protected virtual Handle MakeBlock([NotNull] BlockStartState start, [NotNull] BlockAST blkAST, [NotNull] IList<Handle> alts) {
            start.sll = IsSLLDecision(blkAST);

            BlockEndState end = NewState<BlockEndState>(blkAST);
            start.endState = end;
            foreach (Handle alt in alts) {
                // hook alts up to decision block
                Epsilon(start, alt.left);
                Epsilon(alt.right, end);
                // no back link in ATN so must walk entire alt to see if we can
                // strip out the epsilon to 'end' state
                TailEpsilonRemover opt = new TailEpsilonRemover(atn);
                opt.Visit(alt.left);
            }
            Handle h = new Handle(start, end);
            //		FASerializer ser = new FASerializer(g, h.left);
            //		System.out.println(blkAST.toStringTree()+":\n"+ser);
            blkAST.atnState = start;

            return h;
        }

        [return: NotNull]
        public virtual Handle Alt([NotNull] IList<Handle> els) {
            return ElemList(els);
        }

        [return: NotNull]
        public virtual Handle ElemList([NotNull] IList<Handle> els) {
            int n = els.Count;
            for (int i = 0; i < n - 1; i++) {   // hook up elements (visit all but last)
                Handle el = els[i];
                // if el is of form o-x->o for x in {rule, action, pred, token, ...}
                // and not last in alt
                Transition tr = null;
                if (el.left.NumberOfTransitions == 1) tr = el.left.Transition(0);
                bool isRuleTrans = tr is RuleTransition;
                if (el.left.StateType == StateType.Basic &&
                    el.right.StateType == StateType.Basic &&
                    tr != null && (isRuleTrans && ((RuleTransition)tr).followState == el.right || tr.target == el.right))
                {
                    // we can avoid epsilon edge to next el
                    if (isRuleTrans) ((RuleTransition)tr).followState = els[i + 1].left;
                    else tr.target = els[i + 1].left;
                    atn.RemoveState(el.right); // we skipped over this state
                }
                else { // need epsilon if previous block's right end node is complicated
                    Epsilon(el.right, els[i + 1].left);
                }
            }
            Handle first = els[0];
            Handle last = els[n - 1];
            if (first == null || last == null) {
                g.tool.errMgr.ToolError(ErrorType.INTERNAL_ERROR, "element list has first|last == null");
            }
            return new Handle(first.left, last.right);
        }

        /**
         * From {@code (A)?} build either:
         *
         * <pre>
         *  o--A-&gt;o
         *  |     ^
         *  o----&gt;|
         * </pre>
         *
         * or, if {@code A} is a block, just add an empty alt to the end of the
         * block
         */
        [return: NotNull]
        public virtual Handle Optional([NotNull] GrammarAST optAST, [NotNull] Handle blk) {
            BlockStartState blkStart = (BlockStartState)blk.left;
            ATNState blkEnd = blk.right;
            preventEpsilonOptionalBlocks.Add(Tuple.Create<Rule, ATNState, ATNState>(currentRule, blkStart, blkEnd));

            bool greedy = ((QuantifierAST)optAST).GetGreedy();
            blkStart.sll = false; // no way to express SLL restriction
            blkStart.nonGreedy = !greedy;
            Epsilon(blkStart, blk.right, !greedy);

            optAST.atnState = blk.left;
            return blk;
        }

        /**
         * From {@code (blk)+} build
         *
         * <pre>
         *   |---------|
         *   v         |
         *  [o-blk-o]-&gt;o-&gt;o
         * </pre>
         *
         * We add a decision for loop back node to the existing one at {@code blk}
         * start.
         */
        [return: NotNull]
        public virtual Handle Plus([NotNull] GrammarAST plusAST, [NotNull] Handle blk) {
            PlusBlockStartState blkStart = (PlusBlockStartState)blk.left;
            BlockEndState blkEnd = (BlockEndState)blk.right;
            preventEpsilonClosureBlocks.Add(Tuple.Create<Rule, ATNState, ATNState>(currentRule, blkStart, blkEnd));

            PlusLoopbackState loop = NewState<PlusLoopbackState>(plusAST);
            loop.nonGreedy = !((QuantifierAST)plusAST).GetGreedy();
            loop.sll = false; // no way to express SLL restriction
            atn.DefineDecisionState(loop);
            LoopEndState end = NewState<LoopEndState>(plusAST);
            blkStart.loopBackState = loop;
            end.loopBackState = loop;

            plusAST.atnState = loop;
            Epsilon(blkEnd, loop);      // blk can see loop back

            BlockAST blkAST = (BlockAST)plusAST.GetChild(0);
            if (((QuantifierAST)plusAST).GetGreedy()) {
                if (ExpectNonGreedy(blkAST)) {
                    g.tool.errMgr.GrammarError(ErrorType.EXPECTED_NON_GREEDY_WILDCARD_BLOCK, g.fileName, plusAST.Token, plusAST.Token.Text);
                }

                Epsilon(loop, blkStart);    // loop back to start
                Epsilon(loop, end);         // or exit
            }
            else {
                // if not greedy, priority to exit branch; make it first
                Epsilon(loop, end);         // exit
                Epsilon(loop, blkStart);    // loop back to start
            }

            return new Handle(blkStart, end);
        }

        /**
         * From {@code (blk)*} build {@code ( blk+ )?} with *two* decisions, one for
         * entry and one for choosing alts of {@code blk}.
         *
         * <pre>
         *   |-------------|
         *   v             |
         *   o--[o-blk-o]-&gt;o  o
         *   |                ^
         *   -----------------|
         * </pre>
         *
         * Note that the optional bypass must jump outside the loop as
         * {@code (A|B)*} is not the same thing as {@code (A|B|)+}.
         */
        [return: NotNull]
        public virtual Handle Star([NotNull] GrammarAST starAST, [NotNull] Handle elem) {
            StarBlockStartState blkStart = (StarBlockStartState)elem.left;
            BlockEndState blkEnd = (BlockEndState)elem.right;
            preventEpsilonClosureBlocks.Add(Tuple.Create<Rule, ATNState, ATNState>(currentRule, blkStart, blkEnd));

            StarLoopEntryState entry = NewState<StarLoopEntryState>(starAST);
            entry.nonGreedy = !((QuantifierAST)starAST).GetGreedy();
            entry.sll = false; // no way to express SLL restriction
            atn.DefineDecisionState(entry);
            LoopEndState end = NewState<LoopEndState>(starAST);
            StarLoopbackState loop = NewState<StarLoopbackState>(starAST);
            entry.loopBackState = loop;
            end.loopBackState = loop;

            BlockAST blkAST = (BlockAST)starAST.GetChild(0);
            if (((QuantifierAST)starAST).GetGreedy()) {
                if (ExpectNonGreedy(blkAST)) {
                    g.tool.errMgr.GrammarError(ErrorType.EXPECTED_NON_GREEDY_WILDCARD_BLOCK, g.fileName, starAST.Token, starAST.Token.Text);
                }

                Epsilon(entry, blkStart);   // loop enter edge (alt 1)
                Epsilon(entry, end);        // bypass loop edge (alt 2)
            }
            else {
                // if not greedy, priority to exit branch; make it first
                Epsilon(entry, end);        // bypass loop edge (alt 1)
                Epsilon(entry, blkStart);   // loop enter edge (alt 2)
            }
            Epsilon(blkEnd, loop);      // block end hits loop back
            Epsilon(loop, entry);       // loop back to entry/exit decision

            starAST.atnState = entry;   // decision is to enter/exit; blk is its own decision
            return new Handle(entry, end);
        }

        /** Build an atom with all possible values in its label. */
        [return: NotNull]
        public virtual Handle Wildcard([NotNull] GrammarAST node) {
            ATNState left = NewState(node);
            ATNState right = NewState(node);
            left.AddTransition(new WildcardTransition(right));
            node.atnState = left;
            return new Handle(left, right);
        }

        protected virtual void Epsilon(ATNState a, [NotNull] ATNState b) {
            Epsilon(a, b, false);
        }

        protected virtual void Epsilon(ATNState a, [NotNull] ATNState b, bool prepend) {
            if (a != null) {
                int index = prepend ? 0 : a.NumberOfTransitions;
                a.AddTransition(index, new EpsilonTransition(b));
            }
        }

        /** Define all the rule begin/end ATNStates to solve forward reference
         *  issues.
         */
        internal virtual void CreateRuleStartAndStopATNStates() {
            atn.ruleToStartState = new RuleStartState[g.rules.Count];
            atn.ruleToStopState = new RuleStopState[g.rules.Count];
            foreach (Rule r in g.rules.Values) {
                RuleStartState start = NewState<RuleStartState>(r.ast);
                RuleStopState stop = NewState<RuleStopState>(r.ast);
                start.stopState = stop;
                start.isPrecedenceRule = r is LeftRecursiveRule;
                start.SetRuleIndex(r.index);
                stop.SetRuleIndex(r.index);
                atn.ruleToStartState[r.index] = start;
                atn.ruleToStopState[r.index] = stop;
            }
        }

        public virtual void AddRuleFollowLinks() {
            foreach (ATNState p in atn.states) {
                if (p != null &&
                     p.StateType == StateType.Basic && p.NumberOfTransitions == 1 &&
                     p.Transition(0) is RuleTransition)
                {
                    RuleTransition rt = (RuleTransition)p.Transition(0);
                    AddFollowLink(rt.ruleIndex, rt.followState);
                }
            }
        }

        /** Add an EOF transition to any rule end ATNState that points to nothing
         *  (i.e., for all those rules not invoked by another rule).  These
         *  are start symbols then.
         *
         *  Return the number of grammar entry points; i.e., how many rules are
         *  not invoked by another rule (they can only be invoked from outside).
         *  These are the start rules.
         */
        public virtual int AddEOFTransitionToStartRules() {
            int n = 0;
            ATNState eofTarget = NewState(null); // one unique EOF target for all rules
            foreach (Rule r in g.rules.Values) {
                ATNState stop = atn.ruleToStopState[r.index];
                if (stop.NumberOfTransitions > 0) continue;
                n++;
                Transition t = new AtomTransition(eofTarget, TokenConstants.Eof);
                stop.AddTransition(t);
            }
            return n;
        }

        [return: NotNull]
        public virtual Handle Label([NotNull] Handle t) {
            return t;
        }

        [return: NotNull]
        public virtual Handle ListLabel([NotNull] Handle t) {
            return t;
        }

        [return: NotNull]
        public virtual T NewState<T>(GrammarAST node)
                where T : ATNState, new()
        {
            T s = new T();
            if (currentRule == null) s.SetRuleIndex(-1);
            else s.SetRuleIndex(currentRule.index);
            atn.AddState(s);
            return s;
        }

        [return: NotNull]
        public virtual ATNState NewState([Nullable] GrammarAST node) {
            ATNState n = new BasicState();
            n.SetRuleIndex(currentRule.index);
            atn.AddState(n);
            return n;
        }

        [return: NotNull]
        public virtual ATNState NewState() { return NewState(null); }

        public virtual bool ExpectNonGreedy([NotNull] BlockAST blkAST) {
            if (BlockHasWildcardAlt(blkAST)) {
                return true;
            }

            return false;
        }

        public bool IsSLLDecision([NotNull] BlockAST blkAST) {
            return true.ToString().Equals(blkAST.GetOptionString("sll"), StringComparison.OrdinalIgnoreCase);
        }

        /**
         * {@code (BLOCK (ALT .))} or {@code (BLOCK (ALT 'a') (ALT .))}.
         */
        public static bool BlockHasWildcardAlt([NotNull] GrammarAST block) {
            foreach (object alt in block.Children) {
                if (!(alt is AltAST)) continue;
                AltAST altAST = (AltAST)alt;
                if (altAST.ChildCount == 1 || (altAST.ChildCount == 2 && altAST.GetChild(0).Type == ANTLRParser.ELEMENT_OPTIONS)) {
                    ITree e = altAST.GetChild(altAST.ChildCount - 1);
                    if (e.Type == ANTLRParser.WILDCARD) {
                        return true;
                    }
                }
            }
            return false;
        }

        [return: NotNull]
        public virtual Handle LexerAltCommands([NotNull] Handle alt, [NotNull] Handle cmds) {
            throw new InvalidOperationException("This element is not allowed in parsers.");
        }

        [return: NotNull]
        public virtual Handle LexerCallCommand([NotNull] GrammarAST ID, [NotNull] GrammarAST arg) {
            throw new InvalidOperationException("This element is not allowed in parsers.");
        }

        [return: NotNull]
        public virtual Handle LexerCommand([NotNull] GrammarAST ID) {
            throw new InvalidOperationException("This element is not allowed in parsers.");
        }
    }
}
