// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
