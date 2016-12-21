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
