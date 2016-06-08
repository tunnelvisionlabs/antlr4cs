// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* [The "BSD license"]
* Copyright (c) 2013 Terence Parr
* Copyright (c) 2013 Sam Harwell
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions
* are met:
*
* 1. Redistributions of source code must retain the above copyright
*    notice, this list of conditions and the following disclaimer.
* 2. Redistributions in binary form must reproduce the above copyright
*    notice, this list of conditions and the following disclaimer in the
*    documentation and/or other materials provided with the distribution.
* 3. The name of the author may not be used to endorse or promote products
*    derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
* OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
* IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
* INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
* NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
* DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
* THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
* THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>
    /// A parser simulator that mimics what ANTLR's generated
    /// parser code does.
    /// </summary>
    /// <remarks>
    /// A parser simulator that mimics what ANTLR's generated
    /// parser code does. A ParserATNSimulator is used to make
    /// predictions via adaptivePredict but this class moves a pointer through the
    /// ATN to simulate parsing. ParserATNSimulator just
    /// makes us efficient rather than having to backtrack, for example.
    /// This properly creates parse trees even for left recursive rules.
    /// We rely on the left recursive rule invocation and special predicate
    /// transitions to make left recursive rules work.
    /// See TestParserInterpreter for examples.
    /// </remarks>
    public class ParserInterpreter : Parser
    {
        protected internal readonly string grammarFileName;

        protected internal readonly ATN atn;

        /// <summary>
        /// This identifies StarLoopEntryState's that begin the (...)
        /// precedence loops of left recursive rules.
        /// </summary>
        protected internal readonly BitSet pushRecursionContextStates;

        [Obsolete]
        protected internal readonly string[] tokenNames;

        protected internal readonly string[] ruleNames;

        [NotNull]
        private readonly IVocabulary vocabulary;

        /// <summary>
        /// This stack corresponds to the _parentctx, _parentState pair of locals
        /// that would exist on call stack frames with a recursive descent parser;
        /// in the generated function for a left-recursive rule you'd see:
        /// private EContext e(int _p) throws RecognitionException {
        /// ParserRuleContext _parentctx = _ctx;    // Pair.a
        /// int _parentState = getState();          // Pair.b
        /// ...
        /// </summary>
        /// <remarks>
        /// This stack corresponds to the _parentctx, _parentState pair of locals
        /// that would exist on call stack frames with a recursive descent parser;
        /// in the generated function for a left-recursive rule you'd see:
        /// private EContext e(int _p) throws RecognitionException {
        /// ParserRuleContext _parentctx = _ctx;    // Pair.a
        /// int _parentState = getState();          // Pair.b
        /// ...
        /// }
        /// Those values are used to create new recursive rule invocation contexts
        /// associated with left operand of an alt like "expr '*' expr".
        /// </remarks>
        protected internal readonly Stack<Tuple<ParserRuleContext, int>> _parentContextStack = new Stack<Tuple<ParserRuleContext, int>>();

        /// <summary>
        /// We need a map from (decision,inputIndex)-&gt;forced alt for computing ambiguous
        /// parse trees.
        /// </summary>
        /// <remarks>
        /// We need a map from (decision,inputIndex)-&gt;forced alt for computing ambiguous
        /// parse trees. For now, we allow exactly one override.
        /// </remarks>
        protected internal int overrideDecision = -1;

        protected internal int overrideDecisionInputIndex = -1;

        protected internal int overrideDecisionAlt = -1;

        protected internal bool overrideDecisionReached = false;

        /// <summary>
        /// What is the current context when we override a decisions?  This tells
        /// us what the root of the parse tree is when using override
        /// for an ambiguity/lookahead check.
        /// </summary>
        protected internal InterpreterRuleContext overrideDecisionRoot = null;

        protected internal InterpreterRuleContext rootContext;

        /// <summary>
        /// A copy constructor that creates a new parser interpreter by reusing
        /// the fields of a previous interpreter.
        /// </summary>
        /// <param name="old">The interpreter to copy</param>
        /// <since>4.5</since>
        public ParserInterpreter(Antlr4.Runtime.ParserInterpreter old)
            : base(((ITokenStream)old.InputStream))
        {
            // latch and only override once; error might trigger infinite loop
            this.grammarFileName = old.grammarFileName;
            this.atn = old.atn;
            this.pushRecursionContextStates = old.pushRecursionContextStates;
            this.tokenNames = old.tokenNames;
            this.ruleNames = old.ruleNames;
            this.vocabulary = old.vocabulary;
            Interpreter = new ParserATNSimulator(this, atn);
        }

        [System.ObsoleteAttribute(@"Use ParserInterpreter(string, IVocabulary, System.Collections.Generic.ICollection{E}, Antlr4.Runtime.Atn.ATN, ITokenStream) instead.")]
        public ParserInterpreter(string grammarFileName, ICollection<string> tokenNames, ICollection<string> ruleNames, ATN atn, ITokenStream input)
            : this(grammarFileName, Antlr4.Runtime.Vocabulary.FromTokenNames(Sharpen.Collections.ToArray(tokenNames, new string[tokenNames.Count])), ruleNames, atn, input)
        {
        }

        public ParserInterpreter(string grammarFileName, IVocabulary vocabulary, ICollection<string> ruleNames, ATN atn, ITokenStream input)
            : base(input)
        {
            this.grammarFileName = grammarFileName;
            this.atn = atn;
            this.tokenNames = new string[atn.maxTokenType];
            for (int i = 0; i < tokenNames.Length; i++)
            {
                tokenNames[i] = vocabulary.GetDisplayName(i);
            }
            this.ruleNames = Sharpen.Collections.ToArray(ruleNames, new string[ruleNames.Count]);
            this.vocabulary = vocabulary;
            // identify the ATN states where pushNewRecursionContext() must be called
            this.pushRecursionContextStates = new BitSet(atn.states.Count);
            foreach (ATNState state in atn.states)
            {
                if (!(state is StarLoopEntryState))
                {
                    continue;
                }
                if (((StarLoopEntryState)state).precedenceRuleDecision)
                {
                    this.pushRecursionContextStates.Set(state.stateNumber);
                }
            }
            // get atn simulator that knows how to do predictions
            Interpreter = new ParserATNSimulator(this, atn);
        }

        public override void Reset()
        {
            base.Reset();
            overrideDecisionReached = false;
            overrideDecisionRoot = null;
        }

        public override ATN Atn
        {
            get
            {
                return atn;
            }
        }

        public override string[] TokenNames
        {
            get
            {
                return tokenNames;
            }
        }

        public override IVocabulary Vocabulary
        {
            get
            {
                return vocabulary;
            }
        }

        public override string[] RuleNames
        {
            get
            {
                return ruleNames;
            }
        }

        public override string GrammarFileName
        {
            get
            {
                return grammarFileName;
            }
        }

        /// <summary>Begin parsing at startRuleIndex</summary>
        public virtual ParserRuleContext Parse(int startRuleIndex)
        {
            RuleStartState startRuleStartState = atn.ruleToStartState[startRuleIndex];
            rootContext = CreateInterpreterRuleContext(null, ATNState.InvalidStateNumber, startRuleIndex);
            if (startRuleStartState.isPrecedenceRule)
            {
                EnterRecursionRule(rootContext, startRuleStartState.stateNumber, startRuleIndex, 0);
            }
            else
            {
                EnterRule(rootContext, startRuleStartState.stateNumber, startRuleIndex);
            }
            while (true)
            {
                ATNState p = AtnState;
                switch (p.StateType)
                {
                    case StateType.RuleStop:
                    {
                        // pop; return from rule
                        if (_ctx.IsEmpty)
                        {
                            if (startRuleStartState.isPrecedenceRule)
                            {
                                ParserRuleContext result = _ctx;
                                Tuple<ParserRuleContext, int> parentContext = _parentContextStack.Pop();
                                UnrollRecursionContexts(parentContext.Item1);
                                return result;
                            }
                            else
                            {
                                ExitRule();
                                return rootContext;
                            }
                        }
                        VisitRuleStopState(p);
                        break;
                    }

                    default:
                    {
                        try
                        {
                            VisitState(p);
                        }
                        catch (RecognitionException e)
                        {
                            State = atn.ruleToStopState[p.ruleIndex].stateNumber;
                            Context.exception = e;
                            ErrorHandler.ReportError(this, e);
                            Recover(e);
                        }
                        break;
                    }
                }
            }
        }

        public override void EnterRecursionRule(ParserRuleContext localctx, int state, int ruleIndex, int precedence)
        {
            _parentContextStack.Push(Tuple.Create(_ctx, localctx.invokingState));
            base.EnterRecursionRule(localctx, state, ruleIndex, precedence);
        }

        protected internal virtual ATNState AtnState
        {
            get
            {
                return atn.states[State];
            }
        }

        protected internal virtual void VisitState(ATNState p)
        {
            int predictedAlt = 1;
            if (p.NumberOfTransitions > 1)
            {
                predictedAlt = VisitDecisionState((DecisionState)p);
            }
            Transition transition = p.Transition(predictedAlt - 1);
            switch (transition.TransitionType)
            {
                case TransitionType.Epsilon:
                {
                    if (pushRecursionContextStates.Get(p.stateNumber) && !(transition.target is LoopEndState))
                    {
                        // We are at the start of a left recursive rule's (...)* loop
                        // and we're not taking the exit branch of loop.
                        InterpreterRuleContext localctx = CreateInterpreterRuleContext(_parentContextStack.Peek().Item1, _parentContextStack.Peek().Item2, _ctx.RuleIndex);
                        PushNewRecursionContext(localctx, atn.ruleToStartState[p.ruleIndex].stateNumber, _ctx.RuleIndex);
                    }
                    break;
                }

                case TransitionType.Atom:
                {
                    Match(((AtomTransition)transition).label);
                    break;
                }

                case TransitionType.Range:
                case TransitionType.Set:
                case TransitionType.NotSet:
                {
                    if (!transition.Matches(_input.La(1), TokenConstants.MinUserTokenType, 65535))
                    {
                        RecoverInline();
                    }
                    MatchWildcard();
                    break;
                }

                case TransitionType.Wildcard:
                {
                    MatchWildcard();
                    break;
                }

                case TransitionType.Rule:
                {
                    RuleStartState ruleStartState = (RuleStartState)transition.target;
                    int ruleIndex = ruleStartState.ruleIndex;
                    InterpreterRuleContext newctx = CreateInterpreterRuleContext(_ctx, p.stateNumber, ruleIndex);
                    if (ruleStartState.isPrecedenceRule)
                    {
                        EnterRecursionRule(newctx, ruleStartState.stateNumber, ruleIndex, ((RuleTransition)transition).precedence);
                    }
                    else
                    {
                        EnterRule(newctx, transition.target.stateNumber, ruleIndex);
                    }
                    break;
                }

                case TransitionType.Predicate:
                {
                    PredicateTransition predicateTransition = (PredicateTransition)transition;
                    if (!Sempred(_ctx, predicateTransition.ruleIndex, predicateTransition.predIndex))
                    {
                        throw new FailedPredicateException(this);
                    }
                    break;
                }

                case TransitionType.Action:
                {
                    ActionTransition actionTransition = (ActionTransition)transition;
                    Action(_ctx, actionTransition.ruleIndex, actionTransition.actionIndex);
                    break;
                }

                case TransitionType.Precedence:
                {
                    if (!Precpred(_ctx, ((PrecedencePredicateTransition)transition).precedence))
                    {
                        throw new FailedPredicateException(this, string.Format("precpred(_ctx, %d)", ((PrecedencePredicateTransition)transition).precedence));
                    }
                    break;
                }

                default:
                {
                    throw new NotSupportedException("Unrecognized ATN transition type.");
                }
            }
            State = transition.target.stateNumber;
        }

        /// <summary>
        /// Method visitDecisionState() is called when the interpreter reaches
        /// a decision state (instance of DecisionState).
        /// </summary>
        /// <remarks>
        /// Method visitDecisionState() is called when the interpreter reaches
        /// a decision state (instance of DecisionState). It gives an opportunity
        /// for subclasses to track interesting things.
        /// </remarks>
        protected internal virtual int VisitDecisionState(DecisionState p)
        {
            int edge = 1;
            int predictedAlt;
            ErrorHandler.Sync(this);
            int decision = p.decision;
            if (decision == overrideDecision && _input.Index == overrideDecisionInputIndex && !overrideDecisionReached)
            {
                predictedAlt = overrideDecisionAlt;
                overrideDecisionReached = true;
            }
            else
            {
                predictedAlt = Interpreter.AdaptivePredict(_input, decision, _ctx);
            }
            return predictedAlt;
        }

        /// <summary>Provide simple "factory" for InterpreterRuleContext's.</summary>
        /// <since>4.5.1</since>
        protected internal virtual InterpreterRuleContext CreateInterpreterRuleContext(ParserRuleContext parent, int invokingStateNumber, int ruleIndex)
        {
            return new InterpreterRuleContext(parent, invokingStateNumber, ruleIndex);
        }

        protected internal virtual void VisitRuleStopState(ATNState p)
        {
            RuleStartState ruleStartState = atn.ruleToStartState[p.ruleIndex];
            if (ruleStartState.isPrecedenceRule)
            {
                Tuple<ParserRuleContext, int> parentContext = _parentContextStack.Pop();
                UnrollRecursionContexts(parentContext.Item1);
                State = parentContext.Item2;
            }
            else
            {
                ExitRule();
            }
            RuleTransition ruleTransition = (RuleTransition)atn.states[State].Transition(0);
            State = ruleTransition.followState.stateNumber;
        }

        /// <summary>
        /// Override this parser interpreters normal decision-making process
        /// at a particular decision and input token index.
        /// </summary>
        /// <remarks>
        /// Override this parser interpreters normal decision-making process
        /// at a particular decision and input token index. Instead of
        /// allowing the adaptive prediction mechanism to choose the
        /// first alternative within a block that leads to a successful parse,
        /// force it to take the alternative, 1..n for n alternatives.
        /// As an implementation limitation right now, you can only specify one
        /// override. This is sufficient to allow construction of different
        /// parse trees for ambiguous input. It means re-parsing the entire input
        /// in general because you're never sure where an ambiguous sequence would
        /// live in the various parse trees. For example, in one interpretation,
        /// an ambiguous input sequence would be matched completely in expression
        /// but in another it could match all the way back to the root.
        /// s : e '!'? ;
        /// e : ID
        /// | ID '!'
        /// ;
        /// Here, x! can be matched as (s (e ID) !) or (s (e ID !)). In the first
        /// case, the ambiguous sequence is fully contained only by the root.
        /// In the second case, the ambiguous sequences fully contained within just
        /// e, as in: (e ID !).
        /// Rather than trying to optimize this and make
        /// some intelligent decisions for optimization purposes, I settled on
        /// just re-parsing the whole input and then using
        /// {link Trees#getRootOfSubtreeEnclosingRegion} to find the minimal
        /// subtree that contains the ambiguous sequence. I originally tried to
        /// record the call stack at the point the parser detected and ambiguity but
        /// left recursive rules create a parse tree stack that does not reflect
        /// the actual call stack. That impedance mismatch was enough to make
        /// it it challenging to restart the parser at a deeply nested rule
        /// invocation.
        /// Only parser interpreters can override decisions so as to avoid inserting
        /// override checking code in the critical ALL(*) prediction execution path.
        /// </remarks>
        /// <since>4.5</since>
        public virtual void AddDecisionOverride(int decision, int tokenIndex, int forcedAlt)
        {
            overrideDecision = decision;
            overrideDecisionInputIndex = tokenIndex;
            overrideDecisionAlt = forcedAlt;
        }

        public virtual InterpreterRuleContext OverrideDecisionRoot
        {
            get
            {
                return overrideDecisionRoot;
            }
        }

        /// <summary>
        /// Rely on the error handler for this parser but, if no tokens are consumed
        /// to recover, add an error node.
        /// </summary>
        /// <remarks>
        /// Rely on the error handler for this parser but, if no tokens are consumed
        /// to recover, add an error node. Otherwise, nothing is seen in the parse
        /// tree.
        /// </remarks>
        protected internal virtual void Recover(RecognitionException e)
        {
            int i = _input.Index;
            ErrorHandler.Recover(this, e);
            if (_input.Index == i)
            {
                // no input consumed, better add an error node
                if (e is InputMismatchException)
                {
                    InputMismatchException ime = (InputMismatchException)e;
                    IToken tok = e.OffendingToken;
                    int expectedTokenType = ime.GetExpectedTokens().MinElement;
                    // get any element
                    IToken errToken = TokenFactory.Create(Tuple.Create(tok.TokenSource, tok.TokenSource.InputStream), expectedTokenType, tok.Text, TokenConstants.DefaultChannel, -1, -1, tok.Line, tok.Column);
                    // invalid start/stop
                    _ctx.AddErrorNode(errToken);
                }
                else
                {
                    // NoViableAlt
                    IToken tok = e.OffendingToken;
                    IToken errToken = TokenFactory.Create(Tuple.Create(tok.TokenSource, tok.TokenSource.InputStream), TokenConstants.InvalidType, tok.Text, TokenConstants.DefaultChannel, -1, -1, tok.Line, tok.Column);
                    // invalid start/stop
                    _ctx.AddErrorNode(errToken);
                }
            }
        }

        protected internal virtual IToken RecoverInline()
        {
            return _errHandler.RecoverInline(this);
        }

        /// <summary>
        /// Return the root of the parse, which can be useful if the parser
        /// bails out.
        /// </summary>
        /// <remarks>
        /// Return the root of the parse, which can be useful if the parser
        /// bails out. You still can access the top node. Note that,
        /// because of the way left recursive rules add children, it's possible
        /// that the root will not have any children if the start rule immediately
        /// called and left recursive rule that fails.
        /// </remarks>
        /// <since>4.5.1</since>
        public virtual InterpreterRuleContext RootContext
        {
            get
            {
                return rootContext;
            }
        }
    }
}
