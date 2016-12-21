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
    using System.Text;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool;

    /** An ATN walker that knows how to dump them to serialized strings. */
    public class ATNPrinter
    {
        IList<ATNState> work;
        ISet<ATNState> marked;
        Grammar g;
        ATNState start;

        public ATNPrinter(Grammar g, ATNState start)
        {
            this.g = g;
            this.start = start;
        }

        public virtual string AsString()
        {
            if (start == null)
                return null;
            marked = new HashSet<ATNState>();

            work = new List<ATNState>();
            work.Add(start);

            StringBuilder buf = new StringBuilder();
            ATNState s;

            while (work.Count > 0)
            {
                s = work[0];
                work.RemoveAt(0);
                if (marked.Contains(s))
                    continue;
                int n = s.NumberOfTransitions;
                //System.Console.WriteLine("visit " + s + "; edges=" + n);
                marked.Add(s);
                for (int i = 0; i < n; i++)
                {
                    Transition t = s.Transition(i);
                    if (!(s is RuleStopState))
                    { // don't add follow states to work
                        if (t is RuleTransition)
                            work.Add(((RuleTransition)t).followState);
                        else
                            work.Add(t.target);
                    }
                    buf.Append(GetStateString(s));
                    if (t is EpsilonTransition)
                    {
                        buf.Append("->").Append(GetStateString(t.target)).Append('\n');
                    }
                    else if (t is RuleTransition)
                    {
                        buf.Append("-").Append(g.GetRule(((RuleTransition)t).ruleIndex).name).Append("->").Append(GetStateString(t.target)).Append('\n');
                    }
                    else if (t is ActionTransition)
                    {
                        ActionTransition a = (ActionTransition)t;
                        buf.Append("-").Append(a.ToString()).Append("->").Append(GetStateString(t.target)).Append('\n');
                    }
                    else if (t is SetTransition)
                    {
                        SetTransition st = (SetTransition)t;
                        bool not = st is NotSetTransition;
                        if (g.IsLexer())
                        {
                            buf.Append("-").Append(not ? "~" : "").Append(st.ToString()).Append("->").Append(GetStateString(t.target)).Append('\n');
                        }
                        else
                        {
                            buf.Append("-").Append(not ? "~" : "").Append(st.Label.ToString(g.GetVocabulary())).Append("->").Append(GetStateString(t.target)).Append('\n');
                        }
                    }
                    else if (t is AtomTransition)
                    {
                        AtomTransition a = (AtomTransition)t;
                        string label = g.GetTokenDisplayName(a.label);
                        buf.Append("-").Append(label).Append("->").Append(GetStateString(t.target)).Append('\n');
                    }
                    else
                    {
                        buf.Append("-").Append(t.ToString()).Append("->").Append(GetStateString(t.target)).Append('\n');
                    }
                }
            }
            return buf.ToString();
        }

        internal virtual string GetStateString(ATNState s)
        {
            int n = s.stateNumber;
            string stateStr = "s" + n;
            if (s is StarBlockStartState)
                stateStr = "StarBlockStart_" + n;
            else if (s is PlusBlockStartState)
                stateStr = "PlusBlockStart_" + n;
            else if (s is BlockStartState)
                stateStr = "BlockStart_" + n;
            else if (s is BlockEndState)
                stateStr = "BlockEnd_" + n;
            else if (s is RuleStartState)
                stateStr = "RuleStart_" + g.GetRule(s.ruleIndex).name + "_" + n;
            else if (s is RuleStopState)
                stateStr = "RuleStop_" + g.GetRule(s.ruleIndex).name + "_" + n;
            else if (s is PlusLoopbackState)
                stateStr = "PlusLoopBack_" + n;
            else if (s is StarLoopbackState)
                stateStr = "StarLoopBack_" + n;
            else if (s is StarLoopEntryState)
                stateStr = "StarLoopEntry_" + n;
            return stateStr;
        }
    }
}
