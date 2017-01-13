// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Atn;
    using Antlr4.Runtime.Tree;
    using Activator = System.Activator;
    using ArgumentException = System.ArgumentException;
    using BitSet = Antlr4.Runtime.Sharpen.BitSet;
    using Exception = System.Exception;
    using Interval = Antlr4.Runtime.Misc.Interval;
    using Type = System.Type;

    /** A heavier weight {@link ParserInterpreter} that creates parse trees
     *  that track alternative numbers for subtree roots.
     *
     * @since 4.5.1
     *
     */
    public class GrammarParserInterpreter : ParserInterpreter
    {
        /** The grammar associated with this interpreter. Unlike the
         *  {@link ParserInterpreter} from the standard distribution,
         *  this can reference Grammar, which is in the tools area not
         *  purely runtime.
         */
        protected readonly Grammar g;

        protected BitSet decisionStatesThatSetOuterAltNumInContext;

        /** Cache {@link LeftRecursiveRule#getPrimaryAlts()} and
         *  {@link LeftRecursiveRule#getRecursiveOpAlts()} for states in
         *  {@link #decisionStatesThatSetOuterAltNumInContext}. It only
         *  caches decisions in left-recursive rules.
         */
        protected int[][] stateToAltsMap;

        public GrammarParserInterpreter(Grammar g,
                                        string grammarFileName,
                                        IVocabulary vocabulary,
                                        ICollection<string> ruleNames,
                                        ATN atn,
                                        ITokenStream input)
            : base(grammarFileName, vocabulary, ruleNames, atn, input)
        {
            this.g = g;
        }

        public GrammarParserInterpreter(Grammar g, ATN atn, ITokenStream input)
            : base(g.fileName, g.GetVocabulary(),
                  g.GetRuleNames(),
                  atn, // must run ATN through serializer to set some state flags
                  input)
        {
            this.g = g;
            decisionStatesThatSetOuterAltNumInContext = FindOuterMostDecisionStates();
            stateToAltsMap = new int[g.atn.states.Count][];
        }

        protected override InterpreterRuleContext CreateInterpreterRuleContext(ParserRuleContext parent,
                                                                      int invokingStateNumber,
                                                                      int ruleIndex)
        {
            return new GrammarInterpreterRuleContext(parent, invokingStateNumber, ruleIndex);
        }

        public override void Reset()
        {
            base.Reset();
            overrideDecisionRoot = null;
        }

        /** identify the ATN states where we need to set the outer alt number.
         *  For regular rules, that's the block at the target to rule start state.
         *  For left-recursive rules, we track the primary block, which looks just
         *  like a regular rule's outer block, and the star loop block (always
         *  there even if 1 alt).
         */
        public virtual BitSet FindOuterMostDecisionStates()
        {
            BitSet track = new BitSet(atn.states.Count);
            int numberOfDecisions = atn.NumberOfDecisions;
            for (int i = 0; i < numberOfDecisions; i++)
            {
                DecisionState decisionState = atn.GetDecisionState(i);
                RuleStartState startState = atn.ruleToStartState[decisionState.ruleIndex];
                // Look for StarLoopEntryState that is in any left recursive rule
                if (decisionState is StarLoopEntryState)
                {
                    StarLoopEntryState loopEntry = (StarLoopEntryState)decisionState;
                    if (loopEntry.precedenceRuleDecision)
                    {
                        // Recursive alts always result in a (...)* in the transformed
                        // left recursive rule and that always has a BasicBlockStartState
                        // even if just 1 recursive alt exists.
                        ATNState blockStart = loopEntry.Transition(0).target;
                        // track the StarBlockStartState associated with the recursive alternatives
                        track.Set(blockStart.stateNumber);
                    }
                }
                else if (startState.Transition(0).target == decisionState)
                {
                    // always track outermost block for any rule if it exists
                    track.Set(decisionState.stateNumber);
                }
            }
            return track;
        }

        /** Override this method so that we can record which alternative
         *  was taken at each decision point. For non-left recursive rules,
         *  it's simple. Set decisionStatesThatSetOuterAltNumInContext
         *  indicates which decision states should set the outer alternative number.
         *
         *  <p>Left recursive rules are much more complicated to deal with:
         *  there is typically a decision for the primary alternatives and a
         *  decision to choose between the recursive operator alternatives.
         *  For example, the following left recursive rule has two primary and 2
         *  recursive alternatives.</p>
         *
             e : e '*' e
               | '-' INT
               | e '+' e
               | ID
               ;

         *  <p>ANTLR rewrites that rule to be</p>

             e[int precedence]
                 : ('-' INT | ID)
                 ( {...}? '*' e[5]
                 | {...}? '+' e[3]
                 )*
                ;

         *
         *  <p>So, there are two decisions associated with picking the outermost alt.
         *  This complicates our tracking significantly. The outermost alternative number
         *  is a function of the decision (ATN state) within a left recursive rule and the
         *  predicted alternative coming back from adaptivePredict().</p>
         *
         *  We use stateToAltsMap as a cache to avoid expensive calls to
         *  getRecursiveOpAlts().
         */
        protected override int VisitDecisionState(DecisionState p)
        {
            int predictedAlt = base.VisitDecisionState(p);
            if (p.NumberOfTransitions > 1)
            {
                //			System.out.println("decision "+p.decision+": "+predictedAlt);
                if (p.decision == this.overrideDecision &&
                    this._input.Index == this.overrideDecisionInputIndex)
                {
                    overrideDecisionRoot = (GrammarInterpreterRuleContext)Context;
                }
            }

            GrammarInterpreterRuleContext ctx = (GrammarInterpreterRuleContext)_ctx;
            if (decisionStatesThatSetOuterAltNumInContext.Get(p.stateNumber))
            {
                ctx.OuterAlternative = predictedAlt;
                Rule r = g.GetRule(p.ruleIndex);
                if (atn.ruleToStartState[r.index].isPrecedenceRule)
                {
                    int[] alts = stateToAltsMap[p.stateNumber];
                    LeftRecursiveRule lr = (LeftRecursiveRule)g.GetRule(p.ruleIndex);
                    if (p.StateType == StateType.BlockStart)
                    {
                        if (alts == null)
                        {
                            alts = lr.GetPrimaryAlts();
                            stateToAltsMap[p.stateNumber] = alts; // cache it
                        }
                    }
                    else if (p.StateType == StateType.StarBlockStart)
                    {
                        if (alts == null)
                        {
                            alts = lr.GetRecursiveOpAlts();
                            stateToAltsMap[p.stateNumber] = alts; // cache it
                        }
                    }
                    ctx.OuterAlternative = alts[predictedAlt];
                }
            }

            return predictedAlt;
        }

        /** Given an ambiguous parse information, return the list of ambiguous parse trees.
         *  An ambiguity occurs when a specific token sequence can be recognized
         *  in more than one way by the grammar. These ambiguities are detected only
         *  at decision points.
         *
         *  The list of trees includes the actual interpretation (that for
         *  the minimum alternative number) and all ambiguous alternatives.
         *  The actual interpretation is always first.
         *
         *  This method reuses the same physical input token stream used to
         *  detect the ambiguity by the original parser in the first place.
         *  This method resets/seeks within but does not alter originalParser.
         *
         *  The trees are rooted at the node whose start..stop token indices
         *  include the start and stop indices of this ambiguity event. That is,
         *  the trees returned will always include the complete ambiguous subphrase
         *  identified by the ambiguity event.  The subtrees returned will
         *  also always contain the node associated with the overridden decision.
         *
         *  Be aware that this method does NOT notify error or parse listeners as
         *  it would trigger duplicate or otherwise unwanted events.
         *
         *  This uses a temporary ParserATNSimulator and a ParserInterpreter
         *  so we don't mess up any statistics, event lists, etc...
         *  The parse tree constructed while identifying/making ambiguityInfo is
         *  not affected by this method as it creates a new parser interp to
         *  get the ambiguous interpretations.
         *
         *  Nodes in the returned ambig trees are independent of the original parse
         *  tree (constructed while identifying/creating ambiguityInfo).
         *
         *  @since 4.5.1
         *
         *  @param g              From which grammar should we drive alternative
         *                        numbers and alternative labels.
         *
         *  @param originalParser The parser used to create ambiguityInfo; it
         *                        is not modified by this routine and can be either
         *                        a generated or interpreted parser. It's token
         *                        stream *is* reset/seek()'d.
         *  @param tokens		  A stream of tokens to use with the temporary parser.
         *                        This will often be just the token stream within the
         *                        original parser but here it is for flexibility.
         *
         *  @param decision       Which decision to try different alternatives for.
         *
         *  @param alts           The set of alternatives to try while re-parsing.
         *
         *  @param startIndex	  The index of the first token of the ambiguous
         *                        input or other input of interest.
         *
         *  @param stopIndex      The index of the last token of the ambiguous input.
         *                        The start and stop indexes are used primarily to
         *                        identify how much of the resulting parse tree
         *                        to return.
         *
         *  @param startRuleIndex The start rule for the entire grammar, not
         *                        the ambiguous decision. We re-parse the entire input
         *                        and so we need the original start rule.
         *
         *  @return               The list of all possible interpretations of
         *                        the input for the decision in ambiguityInfo.
         *                        The actual interpretation chosen by the parser
         *                        is always given first because this method
         *                        retests the input in alternative order and
         *                        ANTLR always resolves ambiguities by choosing
         *                        the first alternative that matches the input.
         *                        The subtree returned
         *
         *  @throws RecognitionException Throws upon syntax error while matching
         *                               ambig input.
         */
        public static IList<ParserRuleContext> GetAllPossibleParseTrees(Grammar g,
                                                                       Parser originalParser,
                                                                       ITokenStream tokens,
                                                                       int decision,
                                                                       BitSet alts,
                                                                       int startIndex,
                                                                       int stopIndex,
                                                                       int startRuleIndex)
        {
            IList<ParserRuleContext> trees = new List<ParserRuleContext>();
            // Create a new parser interpreter to parse the ambiguous subphrase
            ParserInterpreter parser = DeriveTempParserInterpreter(g, originalParser, tokens);

            if (stopIndex >= (tokens.Size - 1))
            {
                // if we are pointing at EOF token
                // EOF is not in tree, so must be 1 less than last non-EOF token
                stopIndex = tokens.Size - 2;
            }

            // get ambig trees
            int alt = alts.NextSetBit(0);
            while (alt >= 0)
            {
                // re-parse entire input for all ambiguous alternatives
                // (don't have to do first as it's been parsed, but do again for simplicity
                //  using this temp parser.)
                parser.Reset();
                parser.AddDecisionOverride(decision, startIndex, alt);
                ParserRuleContext t = parser.Parse(startRuleIndex);
                GrammarInterpreterRuleContext ambigSubTree =
                    (GrammarInterpreterRuleContext)Trees.GetRootOfSubtreeEnclosingRegion(t, startIndex, stopIndex);
                // Use higher of overridden decision tree or tree enclosing all tokens
                if (Trees.IsAncestorOf(parser.OverrideDecisionRoot, ambigSubTree))
                {
                    ambigSubTree = (GrammarInterpreterRuleContext)parser.OverrideDecisionRoot;
                }
                trees.Add(ambigSubTree);
                alt = alts.NextSetBit(alt + 1);
            }

            return trees;
        }


        /** Return a list of parse trees, one for each alternative in a decision
         *  given the same input.
         *
         *  Very similar to {@link #getAllPossibleParseTrees} except
         *  that it re-parses the input for every alternative in a decision,
         *  not just the ambiguous ones (there is no alts parameter here).
         *  This method also tries to reduce the size of the parse trees
         *  by stripping away children of the tree that are completely out of range
         *  of startIndex..stopIndex. Also, because errors are expected, we
         *  use a specialized error handler that more or less bails out
         *  but that also consumes the first erroneous token at least. This
         *  ensures that an error node will be in the parse tree for display.
         *
         *  NOTES:
        // we must parse the entire input now with decision overrides
        // we cannot parse a subset because it could be that a decision
        // above our decision of interest needs to read way past
        // lookaheadInfo.stopIndex. It seems like there is no escaping
        // the use of a full and complete token stream if we are
        // resetting to token index 0 and re-parsing from the start symbol.
        // It's not easy to restart parsing somewhere in the middle like a
        // continuation because our call stack does not match the
        // tree stack because of left recursive rule rewriting. grrrr!
         *
         * @since 4.5.1
         */
        public static IList<ParserRuleContext> GetLookaheadParseTrees(Grammar g,
                                                                     ParserInterpreter originalParser,
                                                                     ITokenStream tokens,
                                                                     int startRuleIndex,
                                                                     int decision,
                                                                     int startIndex,
                                                                     int stopIndex)
        {
            IList<ParserRuleContext> trees = new List<ParserRuleContext>();
            // Create a new parser interpreter to parse the ambiguous subphrase
            ParserInterpreter parser = DeriveTempParserInterpreter(g, originalParser, tokens);

            DecisionState decisionState = originalParser.Atn.decisionToState[decision];

            for (int alt = 1; alt <= decisionState.Transitions.Length; alt++)
            {
                // re-parse entire input for all ambiguous alternatives
                // (don't have to do first as it's been parsed, but do again for simplicity
                //  using this temp parser.)
                BailButConsumeErrorStrategy errorHandler = new BailButConsumeErrorStrategy();
                parser.ErrorHandler = errorHandler;
                parser.Reset();
                parser.AddDecisionOverride(decision, startIndex, alt);
                ParserRuleContext tt = parser.Parse(startRuleIndex);
                int stopTreeAt = stopIndex;
                if (errorHandler.firstErrorTokenIndex >= 0)
                {
                    stopTreeAt = errorHandler.firstErrorTokenIndex; // cut off rest at first error
                }

                Interval overallRange = tt.SourceInterval;
                if (stopTreeAt > overallRange.b)
                {
                    // If we try to look beyond range of tree, stopTreeAt must be EOF
                    // for which there is no EOF ref in grammar. That means tree
                    // will not have node for stopTreeAt; limit to overallRange.b
                    stopTreeAt = overallRange.b;
                }

                ParserRuleContext subtree =
                    Trees.GetRootOfSubtreeEnclosingRegion(tt,
                                                          startIndex,
                                                          stopTreeAt);
                // Use higher of overridden decision tree or tree enclosing all tokens
                if (Trees.IsAncestorOf(parser.OverrideDecisionRoot, subtree))
                {
                    subtree = parser.OverrideDecisionRoot;
                }
                Trees.StripChildrenOutOfRange(subtree, parser.OverrideDecisionRoot, startIndex, stopTreeAt);
                trees.Add(subtree);
            }

            return trees;
        }

        /** Derive a new parser from an old one that has knowledge of the grammar.
         *  The Grammar object is used to correctly compute outer alternative
         *  numbers for parse tree nodes. A parser of the same type is created
         *  for subclasses of {@link ParserInterpreter}.
         */
        public static ParserInterpreter DeriveTempParserInterpreter(Grammar g, Parser originalParser, ITokenStream tokens)
        {
            ParserInterpreter parser;
            if (originalParser is ParserInterpreter)
            {
                Type c = originalParser.GetType();
                try
                {
                    parser = (ParserInterpreter)Activator.CreateInstance(originalParser.GetType(), g, originalParser.Atn, originalParser.InputStream);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("can't create parser to match incoming " + c.Name, e);
                }
            }
            else
            {
                // must've been a generated parser
                char[] serializedAtn = ATNSerializer.GetSerializedAsChars(originalParser.Atn, originalParser.RuleNames);
                ATN deserialized = new ATNDeserializer().Deserialize(serializedAtn);
                parser = new ParserInterpreter(originalParser.GrammarFileName,
                                               originalParser.Vocabulary,
                                               originalParser.RuleNames,
                                               deserialized,
                                               tokens);
            }

            parser.SetInputStream(tokens);

            // Make sure that we don't get any error messages from using this temporary parser
            parser.ErrorHandler = new BailErrorStrategy();
            parser.RemoveErrorListeners();
            parser.RemoveParseListeners();
            parser.Interpreter.PredictionMode = PredictionMode.LlExactAmbigDetection;
            return parser;
        }

        /** We want to stop and track the first error but we cannot bail out like
         *  {@link BailErrorStrategy} as consume() constructs trees. We make sure
         *  to create an error node during recovery with this strategy. We
         *  consume() 1 token during the "bail out of rule" mechanism in recover()
         *  and let it fall out of the rule to finish constructing trees. For
         *  recovery in line, we throw InputMismatchException to engage recover().
         */
        public class BailButConsumeErrorStrategy : DefaultErrorStrategy
        {
            public int firstErrorTokenIndex = -1;

            public override void Recover(Parser recognizer, RecognitionException e)
            {
                int errIndex = recognizer.InputStream.Index;
                if (firstErrorTokenIndex == -1)
                {
                    firstErrorTokenIndex = errIndex; // latch
                }
                //			System.err.println("recover: error at " + errIndex);
                IIntStream input = recognizer.InputStream;
                if (input.Index < input.Size - 1)
                { // don't consume() eof
                    recognizer.Consume(); // just kill this bad token and let it continue.
                }
            }

            public override IToken RecoverInline(Parser recognizer)
            {
                int errIndex = recognizer.InputStream.Index;
                if (firstErrorTokenIndex == -1)
                {
                    firstErrorTokenIndex = errIndex; // latch
                }
                //			System.err.println("recoverInline: error at " + errIndex);
                InputMismatchException e = new InputMismatchException(recognizer);
                //			TokenStream input = recognizer.getInputStream(); // seek EOF
                //			input.seek(input.size() - 1);
                throw e;
            }

            public override void Sync(Parser recognizer)
            {
            } // don't consume anything; let it fail later
        }
    }
}
