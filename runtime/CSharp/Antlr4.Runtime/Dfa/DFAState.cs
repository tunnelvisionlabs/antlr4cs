// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa
{
    /// <summary>A DFA state represents a set of possible ATN configurations.</summary>
    /// <remarks>
    /// A DFA state represents a set of possible ATN configurations.
    /// As Aho, Sethi, Ullman p. 117 says "The DFA uses its state
    /// to keep track of all possible states the ATN can be in after
    /// reading each input symbol.  That is to say, after reading
    /// input a1a2..an, the DFA is in a state that represents the
    /// subset T of the states of the ATN that are reachable from the
    /// ATN's start state along some path labeled a1a2..an."
    /// In conventional NFA&rarr;DFA conversion, therefore, the subset T
    /// would be a bitset representing the set of states the
    /// ATN could be in.  We need to track the alt predicted by each
    /// state as well, however.  More importantly, we need to maintain
    /// a stack of states, tracking the closure operations as they
    /// jump from rule to rule, emulating rule invocations (method calls).
    /// I have to add a stack to simulate the proper lookahead sequences for
    /// the underlying LL grammar from which the ATN was derived.
    /// <p>I use a set of ATNConfig objects not simple states.  An ATNConfig
    /// is both a state (ala normal conversion) and a RuleContext describing
    /// the chain of rules (if any) followed to arrive at that state.</p>
    /// <p>A DFA state may have multiple references to a particular state,
    /// but with different ATN contexts (with same or different alts)
    /// meaning that state was reached via a different set of rule invocations.</p>
    /// </remarks>
    public class DFAState
    {
        public int stateNumber = -1;

        [NotNull]
        public readonly ATNConfigSet configs;

        /// <summary>
        /// <c>edges.get(symbol)</c>
        /// points to target of symbol.
        /// </summary>
        [NotNull]
        private volatile AbstractEdgeMap<Antlr4.Runtime.Dfa.DFAState> edges;

        private Antlr4.Runtime.Dfa.AcceptStateInfo acceptStateInfo;

        /// <summary>These keys for these edges are the top level element of the global context.</summary>
        [NotNull]
        private volatile AbstractEdgeMap<Antlr4.Runtime.Dfa.DFAState> contextEdges;

        /// <summary>Symbols in this set require a global context transition before matching an input symbol.</summary>
        [Nullable]
        private BitSet contextSymbols;

        /// <summary>
        /// This list is computed by
        /// <see cref="Antlr4.Runtime.Atn.ParserATNSimulator.PredicateDFAState(DFAState, Antlr4.Runtime.Atn.ATNConfigSet, int)"/>
        /// .
        /// </summary>
        [Nullable]
        public DFAState.PredPrediction[] predicates;

        /// <summary>Map a predicate to a predicted alternative.</summary>
        public class PredPrediction
        {
            [NotNull]
            public SemanticContext pred;

            public int alt;

            public PredPrediction([NotNull] SemanticContext pred, int alt)
            {
                // never null; at least SemanticContext.NONE
                this.alt = alt;
                this.pred = pred;
            }

            public override string ToString()
            {
                return "(" + pred + ", " + alt + ")";
            }
        }

        public DFAState([NotNull] DFA dfa, [NotNull] ATNConfigSet configs)
            : this(dfa.EmptyEdgeMap, dfa.EmptyContextEdgeMap, configs)
        {
        }

        public DFAState([NotNull] EmptyEdgeMap<DFAState> emptyEdges, [NotNull] EmptyEdgeMap<DFAState> emptyContextEdges, [NotNull] ATNConfigSet configs)
        {
            this.configs = configs;
            this.edges = emptyEdges;
            this.contextEdges = emptyContextEdges;
        }

        public bool IsContextSensitive
        {
            get
            {
                return contextSymbols != null;
            }
        }

        public bool IsContextSymbol(int symbol)
        {
            if (!IsContextSensitive || symbol < edges.minIndex)
            {
                return false;
            }
            return contextSymbols.Get(symbol - edges.minIndex);
        }

        public void SetContextSymbol(int symbol)
        {
            System.Diagnostics.Debug.Assert(IsContextSensitive);
            if (symbol < edges.minIndex)
            {
                return;
            }
            contextSymbols.Set(symbol - edges.minIndex);
        }

        public virtual void SetContextSensitive(ATN atn)
        {
            System.Diagnostics.Debug.Assert(!configs.IsOutermostConfigSet);
            if (IsContextSensitive)
            {
                return;
            }
            lock (this)
            {
                if (contextSymbols == null)
                {
                    contextSymbols = new BitSet();
                }
            }
        }

        public AcceptStateInfo AcceptStateInfo
        {
            get
            {
                return acceptStateInfo;
            }
            set
            {
                AcceptStateInfo acceptStateInfo = value;
                this.acceptStateInfo = acceptStateInfo;
            }
        }

        public bool IsAcceptState
        {
            get
            {
                return acceptStateInfo != null;
            }
        }

        public int Prediction
        {
            get
            {
                if (acceptStateInfo == null)
                {
                    return ATN.InvalidAltNumber;
                }
                return acceptStateInfo.Prediction;
            }
        }

        public LexerActionExecutor LexerActionExecutor
        {
            get
            {
                if (acceptStateInfo == null)
                {
                    return null;
                }
                return acceptStateInfo.LexerActionExecutor;
            }
        }

        public virtual DFAState GetTarget(int symbol)
        {
            return edges[symbol];
        }

        public virtual void SetTarget(int symbol, DFAState target)
        {
            edges = edges.Put(symbol, target);
        }

        public virtual IDictionary<int, DFAState> EdgeMap
        {
            get
            {
                return edges.ToMap();
            }
        }

        public virtual DFAState GetContextTarget(int invokingState)
        {
            lock (this)
            {
                if (invokingState == PredictionContext.EmptyFullStateKey)
                {
                    invokingState = -1;
                }
                return contextEdges[invokingState];
            }
        }

        public virtual void SetContextTarget(int invokingState, DFAState target)
        {
            lock (this)
            {
                if (!IsContextSensitive)
                {
                    throw new InvalidOperationException("The state is not context sensitive.");
                }
                if (invokingState == PredictionContext.EmptyFullStateKey)
                {
                    invokingState = -1;
                }
                contextEdges = contextEdges.Put(invokingState, target);
            }
        }

        public virtual IDictionary<int, DFAState> ContextEdgeMap
        {
            get
            {
                IDictionary<int, DFAState> map = contextEdges.ToMap();
                if (map.Contains(-1))
                {
                    if (map.Count == 1)
                    {
                        return Antlr4.Runtime.Sharpen.Collections.SingletonMap(PredictionContext.EmptyFullStateKey, map[-1]);
                    }
                    else
                    {
                        try
                        {
                            map[PredictionContext.EmptyFullStateKey] = Sharpen.Collections.Remove(map, -1);
                        }
                        catch (NotSupportedException)
                        {
                            // handles read only, non-singleton maps
                            map = new LinkedHashMap<int, DFAState>(map);
                            map[PredictionContext.EmptyFullStateKey] = Sharpen.Collections.Remove(map, -1);
                        }
                    }
                }
                return map;
            }
        }

        public override int GetHashCode()
        {
            int hash = MurmurHash.Initialize(7);
            hash = MurmurHash.Update(hash, configs.GetHashCode());
            hash = MurmurHash.Finish(hash, 1);
            return hash;
        }

        /// <summary>
        /// Two
        /// <see cref="DFAState"/>
        /// instances are equal if their ATN configuration sets
        /// are the same. This method is used to see if a state already exists.
        /// <p>Because the number of alternatives and number of ATN configurations are
        /// finite, there is a finite number of DFA states that can be processed.
        /// This is necessary to show that the algorithm terminates.</p>
        /// <p>Cannot test the DFA state numbers here because in
        /// <see cref="Antlr4.Runtime.Atn.ParserATNSimulator.AddDFAState(DFA, Antlr4.Runtime.Atn.ATNConfigSet, Antlr4.Runtime.Atn.PredictionContextCache)"/>
        /// we need to know if any other state
        /// exists that has this exact set of ATN configurations. The
        /// <see cref="stateNumber"/>
        /// is irrelevant.</p>
        /// </summary>
        public override bool Equals(object o)
        {
            // compare set of ATN configurations in this set with other
            if (this == o)
            {
                return true;
            }
            if (!(o is DFAState))
            {
                return false;
            }
            DFAState other = (DFAState)o;
            bool sameSet = this.configs.Equals(other.configs);
            //		System.out.println("DFAState.equals: "+configs+(sameSet?"==":"!=")+other.configs);
            return sameSet;
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(stateNumber).Append(":").Append(configs);
            if (IsAcceptState)
            {
                buf.Append("=>");
                if (predicates != null)
                {
                    buf.Append(Arrays.ToString(predicates));
                }
                else
                {
                    buf.Append(Prediction);
                }
            }
            return buf.ToString();
        }
    }
}
