// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Automata
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool;
    using Interval = Antlr4.Runtime.Misc.Interval;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;
    using NotImplementedException = System.NotImplementedException;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

    /**
     *
     * @author Sam Harwell
     */
    public static class ATNOptimizer
    {

        public static void Optimize([NotNull] Grammar g, [NotNull] ATN atn)
        {
            OptimizeSets(g, atn);
            OptimizeStates(atn);
        }

        private static void OptimizeSets(Grammar g, ATN atn)
        {
            if (g.IsParser())
            {
                // parser codegen doesn't currently support SetTransition
                return;
            }

            int removedStates = 0;
            IList<DecisionState> decisions = atn.decisionToState;
            foreach (DecisionState decision in decisions)
            {
                if (decision.ruleIndex >= 0)
                {
                    Rule rule = g.GetRule(decision.ruleIndex);
                    if (char.IsLower(rule.name[0]))
                    {
                        // parser codegen doesn't currently support SetTransition
                        continue;
                    }
                }

                IntervalSet setTransitions = new IntervalSet();
                for (int i = 0; i < decision.NumberOfTransitions; i++)
                {
                    Transition epsTransition = decision.Transition(i);
                    if (!(epsTransition is EpsilonTransition))
                    {
                        continue;
                    }

                    if (epsTransition.target.NumberOfTransitions != 1)
                    {
                        continue;
                    }

                    Transition transition = epsTransition.target.Transition(0);
                    if (!(transition.target is BlockEndState))
                    {
                        continue;
                    }

                    if (transition is NotSetTransition)
                    {
                        // TODO: not yet implemented
                        continue;
                    }

                    if (transition is AtomTransition
                        || transition is RangeTransition
                        || transition is SetTransition)
                    {
                        setTransitions.Add(i);
                    }
                }

                // due to min alt resolution policies, can only collapse sequential alts
                for (int i = setTransitions.GetIntervals().Count - 1; i >= 0; i--)
                {
                    Interval interval = setTransitions.GetIntervals()[i];
                    if (interval.Length <= 1)
                    {
                        continue;
                    }

                    ATNState blockEndState = decision.Transition(interval.a).target.Transition(0).target;
                    IntervalSet matchSet = new IntervalSet();
                    for (int j = interval.a; j <= interval.b; j++)
                    {
                        Transition matchTransition = decision.Transition(j).target.Transition(0);
                        if (matchTransition is NotSetTransition)
                        {
                            throw new NotImplementedException();
                        }

                        IntervalSet set = matchTransition.Label;
                        int minElem = set.MinElement;
                        int maxElem = set.MaxElement;
                        for (int k = minElem; k <= maxElem; k++)
                        {
                            if (matchSet.Contains(k))
                            {
                                char setMin = (char)set.MinElement;
                                char setMax = (char)set.MaxElement;
                                // TODO: Token is missing (i.e. position in source will not be displayed).
                                g.tool.errMgr.GrammarError(ErrorType.CHARACTERS_COLLISION_IN_SET, g.fileName,
                                                           null, (char)minElem + "-" + (char)maxElem, "[" + setMin + "-" + setMax + "]");
                                break;
                            }
                        }

                        matchSet.AddAll(set);
                    }

                    Transition newTransition;
                    if (matchSet.GetIntervals().Count == 1)
                    {
                        if (matchSet.Count == 1)
                        {
                            newTransition = new AtomTransition(blockEndState, matchSet.MinElement);
                        }
                        else
                        {
                            Interval matchInterval = matchSet.GetIntervals()[0];
                            newTransition = new RangeTransition(blockEndState, matchInterval.a, matchInterval.b);
                        }
                    }
                    else
                    {
                        newTransition = new SetTransition(blockEndState, matchSet);
                    }

                    decision.Transition(interval.a).target.SetTransition(0, newTransition);
                    for (int j = interval.a + 1; j <= interval.b; j++)
                    {
                        Transition removed = decision.Transition(interval.a + 1);
                        decision.RemoveTransition(interval.a + 1);
                        atn.RemoveState(removed.target);
                        removedStates++;
                    }
                }
            }

            //System.Console.WriteLine("ATN optimizer removed " + removedStates + " states by collapsing sets.");
        }

        private static void OptimizeStates(ATN atn)
        {
            IList<ATNState> states = atn.states;

            int current = 0;
            for (int i = 0; i < states.Count; i++)
            {
                ATNState state = states[i];
                if (state == null)
                {
                    continue;
                }

                if (i != current)
                {
                    state.stateNumber = current;
                    states[current] = state;
                    states[i] = null;
                }

                current++;
            }

            //System.Console.WriteLine("ATN optimizer removed " + (states.Count - current) + " null states.");
            while (states.Count > current)
                states.RemoveAt(states.Count - 1);
        }
    }
}
