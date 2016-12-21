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

namespace Antlr4.Analysis
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Antlr4.Misc;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;
    using TokenConstants = Antlr4.Runtime.TokenConstants;

    public class AnalysisPipeline
    {
        public Grammar g;

        public AnalysisPipeline(Grammar g)
        {
            this.g = g;
        }

        public virtual void Process()
        {
            // LEFT-RECURSION CHECK
            LeftRecursionDetector lr = new LeftRecursionDetector(g, g.atn);
            lr.Check();
            if (lr.listOfRecursiveCycles.Count > 0)
                return; // bail out

            if (g.IsLexer())
            {
                ProcessLexer();
            }
            else
            {
                // BUILD DFA FOR EACH DECISION
                ProcessParser();
            }
        }

        protected virtual void ProcessLexer()
        {
            // make sure all non-fragment lexer rules must match at least one symbol
            foreach (Rule rule in g.rules.Values)
            {
                if (rule.IsFragment())
                {
                    continue;
                }

                LL1Analyzer analyzer = new LL1Analyzer(g.atn);
                IntervalSet look = analyzer.Look(g.atn.ruleToStartState[rule.index], PredictionContext.EmptyLocal);
                if (look.Contains(TokenConstants.Epsilon))
                {
                    g.tool.errMgr.GrammarError(ErrorType.EPSILON_TOKEN, g.fileName, ((GrammarAST)rule.ast.GetChild(0)).Token, rule.name);
                }
            }
        }

        protected virtual void ProcessParser()
        {
            g.decisionLOOK = new List<IntervalSet[]>(g.atn.NumberOfDecisions + 1);
            foreach (DecisionState s in g.atn.decisionToState)
            {
                g.tool.Log("LL1", "\nDECISION " + s.decision + " in rule " + g.GetRule(s.ruleIndex).name);
                IntervalSet[] look;
                if (s.nonGreedy)
                { // nongreedy decisions can't be LL(1)
                    look = new IntervalSet[s.NumberOfTransitions + 1];
                }
                else
                {
                    LL1Analyzer anal = new LL1Analyzer(g.atn);
                    look = anal.GetDecisionLookahead(s);
                    g.tool.Log("LL1", "look=[" + string.Join(", ", look.AsEnumerable()) + "]");
                }

                Debug.Assert(s.decision + 1 >= g.decisionLOOK.Count);
                Utils.SetSize(g.decisionLOOK, s.decision + 1);
                g.decisionLOOK[s.decision] = look;
                g.tool.Log("LL1", "LL(1)? " + Disjoint(look));
            }
        }

        /** Return whether lookahead sets are disjoint; no lookahead ⇒ not disjoint */
        public static bool Disjoint(IntervalSet[] altLook)
        {
            bool collision = false;
            IntervalSet combined = new IntervalSet();
            if (altLook == null)
                return false;
            foreach (IntervalSet look in altLook)
            {
                if (look == null)
                    return false; // lookahead must've computation failed

                if (!look.And(combined).IsNil)
                {
                    collision = true;
                    break;
                }

                combined.AddAll(look);
            }
            return !collision;
        }
    }
}
