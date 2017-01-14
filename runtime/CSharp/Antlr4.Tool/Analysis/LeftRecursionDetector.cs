// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Analysis
{
    using System.Collections.Generic;
    using Antlr4.Misc;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool;

    public class LeftRecursionDetector
    {
        internal Grammar g;
        public ATN atn;

        /** Holds a list of cycles (sets of rule names). */
        public IList<ISet<Rule>> listOfRecursiveCycles = new List<ISet<Rule>>();

        /** Which rule start states have we visited while looking for a single
         * 	left-recursion check?
         */
        ISet<RuleStartState> rulesVisitedPerRuleCheck = new HashSet<RuleStartState>();

        public LeftRecursionDetector(Grammar g, ATN atn)
        {
            this.g = g;
            this.atn = atn;
        }

        public virtual void Check()
        {
            foreach (RuleStartState start in atn.ruleToStartState)
            {
                //System.out.print("check "+start.rule.name);
                rulesVisitedPerRuleCheck.Clear();
                rulesVisitedPerRuleCheck.Add(start);
                //FASerializer ser = new FASerializer(atn.g, start);
                //System.out.print(":\n"+ser+"\n");

                Check(g.GetRule(start.ruleIndex), start, new HashSet<ATNState>());
            }
            //System.out.println("cycles="+listOfRecursiveCycles);
            if (listOfRecursiveCycles.Count > 0)
            {
                g.tool.errMgr.LeftRecursionCycles(g.fileName, listOfRecursiveCycles);
            }
        }

        /** From state s, look for any transition to a rule that is currently
         *  being traced.  When tracing r, visitedPerRuleCheck has r
         *  initially.  If you reach a rule stop state, return but notify the
         *  invoking rule that the called rule is nullable. This implies that
         *  invoking rule must look at follow transition for that invoking state.
         *
         *  The visitedStates tracks visited states within a single rule so
         *  we can avoid epsilon-loop-induced infinite recursion here.  Keep
         *  filling the cycles in listOfRecursiveCycles and also, as a
         *  side-effect, set leftRecursiveRules.
         */
        public virtual bool Check(Rule enclosingRule, ATNState s, ISet<ATNState> visitedStates)
        {
            if (s is RuleStopState)
                return true;
            if (visitedStates.Contains(s))
                return false;
            visitedStates.Add(s);

            //System.out.println("visit "+s);
            int n = s.NumberOfTransitions;
            bool stateReachesStopState = false;
            for (int i = 0; i < n; i++)
            {
                Transition t = s.Transition(i);
                if (t is RuleTransition)
                {
                    RuleTransition rt = (RuleTransition)t;
                    Rule r = g.GetRule(rt.ruleIndex);
                    if (rulesVisitedPerRuleCheck.Contains((RuleStartState)t.target))
                    {
                        AddRulesToCycle(enclosingRule, r);
                    }
                    else
                    {
                        // must visit if not already visited; mark target, pop when done
                        rulesVisitedPerRuleCheck.Add((RuleStartState)t.target);
                        // send new visitedStates set per rule invocation
                        bool nullable = Check(r, t.target, new HashSet<ATNState>());
                        // we're back from visiting that rule
                        rulesVisitedPerRuleCheck.Remove((RuleStartState)t.target);
                        if (nullable)
                        {
                            stateReachesStopState |= Check(enclosingRule, rt.followState, visitedStates);
                        }
                    }
                }
                else if (t.IsEpsilon)
                {
                    stateReachesStopState |= Check(enclosingRule, t.target, visitedStates);
                }
                // else ignore non-epsilon transitions
            }
            return stateReachesStopState;
        }

        /** enclosingRule calls targetRule. Find the cycle containing
         *  the target and add the caller.  Find the cycle containing the caller
         *  and add the target.  If no cycles contain either, then create a new
         *  cycle.
         */
        protected virtual void AddRulesToCycle(Rule enclosingRule, Rule targetRule)
        {
            //System.err.println("left-recursion to "+targetRule.name+" from "+enclosingRule.name);
            bool foundCycle = false;
            foreach (ISet<Rule> rulesInCycle in listOfRecursiveCycles)
            {
                // ensure both rules are in same cycle
                if (rulesInCycle.Contains(targetRule))
                {
                    rulesInCycle.Add(enclosingRule);
                    foundCycle = true;
                }
                if (rulesInCycle.Contains(enclosingRule))
                {
                    rulesInCycle.Add(targetRule);
                    foundCycle = true;
                }
            }
            if (!foundCycle)
            {
                ISet<Rule> cycle = new OrderedHashSet<Rule>();
                cycle.Add(targetRule);
                cycle.Add(enclosingRule);
                listOfRecursiveCycles.Add(cycle);
            }
        }
    }
}
