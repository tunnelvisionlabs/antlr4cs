// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Automata
{
    using Antlr4.Runtime.Atn;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

    /**
     *
     * @author Terence Parr
     */
    public class TailEpsilonRemover : ATNVisitor
    {
        [NotNull]
        private readonly ATN _atn;

        public TailEpsilonRemover([NotNull] ATN atn)
        {
            this._atn = atn;
        }

        public override void VisitState([NotNull] ATNState p)
        {
            if (p.StateType == StateType.Basic && p.NumberOfTransitions == 1)
            {
                ATNState q = p.Transition(0).target;
                if (p.Transition(0) is RuleTransition)
                {
                    q = ((RuleTransition)p.Transition(0)).followState;
                }
                if (q.StateType == StateType.Basic)
                {
                    // we have p-x->q for x in {rule, action, pred, token, ...}
                    // if edge out of q is single epsilon to block end
                    // we can strip epsilon p-x->q-eps->r
                    Transition trans = q.Transition(0);
                    if (q.NumberOfTransitions == 1 && trans is EpsilonTransition)
                    {
                        ATNState r = trans.target;
                        if (r is BlockEndState || r is PlusLoopbackState || r is StarLoopbackState)
                        {
                            // skip over q
                            if (p.Transition(0) is RuleTransition)
                            {
                                ((RuleTransition)p.Transition(0)).followState = r;
                            }
                            else
                            {
                                p.Transition(0).target = r;
                            }
                            _atn.RemoveState(q);
                        }
                    }
                }
            }
        }
    }
}
