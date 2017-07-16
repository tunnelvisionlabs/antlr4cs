// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using Antlr4.Misc;
    using Antlr4.Runtime.Atn;
    using Antlr4.Runtime.Dfa;
    using Antlr4.StringTemplate;
    using Path = System.IO.Path;
    using Uri = System.Uri;

    /** The DOT (part of graphviz) generation aspect. */
    public class DOTGenerator
    {
        public static readonly bool STRIP_NONREDUCED_STATES = false;

        protected string arrowhead = "normal";
        protected string rankdir = "LR";

        /** Library of output templates; use {@code &lt;attrname&gt;} format. */
        public static TemplateGroup stlib = new TemplateGroupFile(
            Path.Combine(
                Path.GetDirectoryName(new Uri(typeof(AntlrTool).GetTypeInfo().Assembly.CodeBase).LocalPath),
                Path.Combine("Tool", "Templates", "Dot", "graphs.stg")),
            Encoding.UTF8);

        protected Grammar grammar;

        /** This aspect is associated with a grammar */
        public DOTGenerator(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public virtual string GetDOT(DFA dfa, bool isLexer)
        {
            if (dfa.s0.Get() == null)
                return null;

            Template dot = stlib.GetInstanceOf("dfa");
            dot.Add("name", "DFA" + dfa.decision);
            dot.Add("startState", dfa.s0.Get().stateNumber);
            //		dot.add("useBox", Tool.internalOption_ShowATNConfigsInDFA);
            dot.Add("rankdir", rankdir);

            // define stop states first; seems to be a bug in DOT where doublecircle
            foreach (DFAState d in dfa.states.Keys)
            {
                if (!d.IsAcceptState)
                    continue;
                Template st = stlib.GetInstanceOf("stopstate");
                st.Add("name", "s" + d.stateNumber);
                st.Add("label", GetStateLabel(d));
                dot.Add("states", st);
            }

            foreach (DFAState d in dfa.states.Keys)
            {
                if (d.IsAcceptState)
                    continue;
                if (d.stateNumber == int.MaxValue)
                    continue;
                Template st = stlib.GetInstanceOf("state");
                st.Add("name", "s" + d.stateNumber);
                st.Add("label", GetStateLabel(d));
                dot.Add("states", st);
            }

            foreach (DFAState d in dfa.states.Keys)
            {
                IDictionary<int, DFAState> edges = d.EdgeMap;
                foreach (KeyValuePair<int, DFAState> entry in edges)
                {
                    DFAState target = entry.Value;
                    if (target == null)
                        continue;
                    if (target.stateNumber == int.MaxValue)
                        continue;
                    int ttype = entry.Key;
                    string label = ttype.ToString();
                    if (isLexer)
                        label = "'" + GetEdgeLabel(((char)ttype).ToString()) + "'";
                    else if (grammar != null)
                        label = grammar.GetTokenDisplayName(ttype);
                    Template st = stlib.GetInstanceOf("edge");
                    st.Add("label", label);
                    st.Add("src", "s" + d.stateNumber);
                    st.Add("target", "s" + target.stateNumber);
                    st.Add("arrowhead", arrowhead);
                    dot.Add("edges", st);
                }
            }

            string output = dot.Render();
            return Utils.SortLinesInString(output);
        }

        protected virtual string GetStateLabel(DFAState s)
        {
            if (s == null)
                return "null";
            StringBuilder buf = new StringBuilder(250);
            buf.Append('s');
            buf.Append(s.stateNumber);
            if (s.IsAcceptState)
            {
                buf.Append("=>").Append(s.Prediction);
            }
            if (grammar != null)
            {
                Runtime.Sharpen.BitSet alts = s.configs.RepresentedAlternatives;
                buf.Append("\\n");
                IEnumerable<ATNConfig> configurations = s.configs;
                for (int alt = alts.NextSetBit(0); alt >= 0; alt = alts.NextSetBit(alt + 1))
                {
                    if (alt > alts.NextSetBit(0))
                    {
                        buf.Append("\\n");
                    }
                    buf.Append("alt");
                    buf.Append(alt);
                    buf.Append(':');
                    // get a list of configs for just this alt
                    // it will help us print better later
                    IList<ATNConfig> configsInAlt = new List<ATNConfig>();
                    foreach (ATNConfig c in configurations)
                    {
                        if (c.Alt != alt)
                            continue;
                        configsInAlt.Add(c);
                    }
                    int n = 0;
                    for (int cIndex = 0; cIndex < configsInAlt.Count; cIndex++)
                    {
                        ATNConfig c = configsInAlt[cIndex];
                        n++;
                        buf.Append(c.ToString(null, false));
                        if ((cIndex + 1) < configsInAlt.Count)
                        {
                            buf.Append(", ");
                        }
                        if (n % 5 == 0 && (configsInAlt.Count - cIndex) > 3)
                        {
                            buf.Append("\\n");
                        }
                    }
                }
            }
            string stateLabel = buf.ToString();
            return stateLabel;
        }

        public virtual string GetDOT(ATNState startState)
        {
            return GetDOT(startState, false);
        }

        public virtual string GetDOT(ATNState startState, bool isLexer)
        {
            ICollection<string> ruleNames = grammar.rules.Keys;
            string[] names = new string[ruleNames.Count + 1];
            int i = 0;
            foreach (string s in ruleNames)
                names[i++] = s;
            return GetDOT(startState, names, isLexer);
        }

        /** Return a String containing a DOT description that, when displayed,
         *  will show the incoming state machine visually.  All nodes reachable
         *  from startState will be included.
         */
        public virtual string GetDOT(ATNState startState, string[] ruleNames, bool isLexer)
        {
            if (startState == null)
                return null;

            // The output DOT graph for visualization
            ISet<ATNState> markedStates = new HashSet<ATNState>();
            Template dot = stlib.GetInstanceOf("atn");
            dot.Add("startState", startState.stateNumber);
            dot.Add("rankdir", rankdir);

            Queue<ATNState> work = new Queue<ATNState>();

            work.Enqueue(startState);
            while (work.Count > 0)
            {
                ATNState s = work.Peek();
                if (markedStates.Contains(s))
                {
                    work.Dequeue();
                    continue;
                }
                markedStates.Add(s);

                // don't go past end of rule node to the follow states
                if (s is RuleStopState)
                    continue;

                // special case: if decision point, then line up the alt start states
                // unless it's an end of block
                //			if ( s instanceof BlockStartState ) {
                //				ST rankST = stlib.getInstanceOf("decision-rank");
                //				DecisionState alt = (DecisionState)s;
                //				for (int i=0; i<alt.getNumberOfTransitions(); i++) {
                //					ATNState target = alt.transition(i).target;
                //					if ( target!=null ) {
                //						rankST.add("states", target.stateNumber);
                //					}
                //				}
                //				dot.add("decisionRanks", rankST);
                //			}

                // make a DOT edge for each transition
                Template edgeST;
                for (int i = 0; i < s.NumberOfTransitions; i++)
                {
                    Transition edge = s.Transition(i);
                    if (edge is RuleTransition)
                    {
                        RuleTransition rr = ((RuleTransition)edge);
                        // don't jump to other rules, but display edge to follow node
                        edgeST = stlib.GetInstanceOf("edge");

                        string label = "<" + ruleNames[rr.ruleIndex];
                        if (((RuleStartState)rr.target).isPrecedenceRule)
                        {
                            label += "[" + rr.precedence + "]";
                        }
                        label += ">";

                        edgeST.Add("label", label);
                        edgeST.Add("src", "s" + s.stateNumber);
                        edgeST.Add("target", "s" + rr.followState.stateNumber);
                        edgeST.Add("arrowhead", arrowhead);
                        dot.Add("edges", edgeST);
                        work.Enqueue(rr.followState);
                        continue;
                    }
                    if (edge is ActionTransition)
                    {
                        edgeST = stlib.GetInstanceOf("action-edge");
                        edgeST.Add("label", GetEdgeLabel(edge.ToString()));
                    }
                    else if (edge is AbstractPredicateTransition)
                    {
                        edgeST = stlib.GetInstanceOf("edge");
                        edgeST.Add("label", GetEdgeLabel(edge.ToString()));
                    }
                    else if (edge.IsEpsilon)
                    {
                        edgeST = stlib.GetInstanceOf("epsilon-edge");
                        edgeST.Add("label", GetEdgeLabel(edge.ToString()));
                        bool loopback = false;
                        if (edge.target is PlusBlockStartState)
                        {
                            loopback = s.Equals(((PlusBlockStartState)edge.target).loopBackState);
                        }
                        else if (edge.target is StarLoopEntryState)
                        {
                            loopback = s.Equals(((StarLoopEntryState)edge.target).loopBackState);
                        }
                        edgeST.Add("loopback", loopback);
                    }
                    else if (edge is AtomTransition)
                    {
                        edgeST = stlib.GetInstanceOf("edge");
                        AtomTransition atom = (AtomTransition)edge;
                        string label = atom.label.ToString();
                        if (isLexer)
                            label = "'" + GetEdgeLabel(((char)atom.label).ToString()) + "'";
                        else if (grammar != null)
                            label = grammar.GetTokenDisplayName(atom.label);
                        edgeST.Add("label", GetEdgeLabel(label));
                    }
                    else if (edge is SetTransition)
                    {
                        edgeST = stlib.GetInstanceOf("edge");
                        SetTransition set = (SetTransition)edge;
                        string label = set.Label.ToString();
                        if (isLexer)
                            label = set.Label.ToString(true);
                        else if (grammar != null)
                            label = set.Label.ToString(grammar.GetVocabulary());
                        if (edge is NotSetTransition)
                            label = "~" + label;
                        edgeST.Add("label", GetEdgeLabel(label));
                    }
                    else if (edge is RangeTransition)
                    {
                        edgeST = stlib.GetInstanceOf("edge");
                        RangeTransition range = (RangeTransition)edge;
                        string label = range.Label.ToString();
                        if (isLexer)
                            label = range.ToString();
                        else if (grammar != null)
                            label = range.Label.ToString(grammar.GetVocabulary());
                        edgeST.Add("label", GetEdgeLabel(label));
                    }
                    else
                    {
                        edgeST = stlib.GetInstanceOf("edge");
                        edgeST.Add("label", GetEdgeLabel(edge.ToString()));
                    }
                    edgeST.Add("src", "s" + s.stateNumber);
                    edgeST.Add("target", "s" + edge.target.stateNumber);
                    edgeST.Add("arrowhead", arrowhead);
                    if (s.NumberOfTransitions > 1)
                    {
                        edgeST.Add("transitionIndex", i);
                    }
                    else
                    {
                        edgeST.Add("transitionIndex", false);
                    }
                    dot.Add("edges", edgeST);
                    work.Enqueue(edge.target);
                }
            }

            // define nodes we visited (they will appear first in DOT output)
            // this is an example of ST's lazy eval :)
            // define stop state first; seems to be a bug in DOT where doublecircle
            // shape only works if we define them first. weird.
            //		ATNState stopState = startState.atn.ruleToStopState.get(startState.rule);
            //		if ( stopState!=null ) {
            //			ST st = stlib.getInstanceOf("stopstate");
            //			st.add("name", "s"+stopState.stateNumber);
            //			st.add("label", getStateLabel(stopState));
            //			dot.add("states", st);
            //		}
            foreach (ATNState s in markedStates)
            {
                if (!(s is RuleStopState))
                    continue;
                Template st = stlib.GetInstanceOf("stopstate");
                st.Add("name", "s" + s.stateNumber);
                st.Add("label", GetStateLabel(s));
                dot.Add("states", st);
            }

            foreach (ATNState s in markedStates)
            {
                if (s is RuleStopState)
                    continue;
                Template st = stlib.GetInstanceOf("state");
                st.Add("name", "s" + s.stateNumber);
                st.Add("label", GetStateLabel(s));
                st.Add("transitions", s.Transitions);
                dot.Add("states", st);
            }

            return dot.Render();
        }


        /////** Do a depth-first walk of the state machine graph and
        //// *  fill a DOT description template.  Keep filling the
        //// *  states and edges attributes.  We know this is an ATN
        //// *  for a rule so don't traverse edges to other rules and
        //// *  don't go past rule end state.
        //// */
        //    protected void walkRuleATNCreatingDOT(ST dot,
        //                                          ATNState s)
        //    {
        //        if ( markedStates.contains(s) ) {
        //            return; // already visited this node
        //        }
        //
        //        markedStates.add(s.stateNumber); // mark this node as completed.
        //
        //        // first add this node
        //        ST stateST;
        //        if ( s instanceof RuleStopState ) {
        //            stateST = stlib.getInstanceOf("stopstate");
        //        }
        //        else {
        //            stateST = stlib.getInstanceOf("state");
        //        }
        //        stateST.add("name", getStateLabel(s));
        //        dot.add("states", stateST);
        //
        //        if ( s instanceof RuleStopState )  {
        //            return; // don't go past end of rule node to the follow states
        //        }
        //
        //        // special case: if decision point, then line up the alt start states
        //        // unless it's an end of block
        //		if ( s instanceof DecisionState ) {
        //			GrammarAST n = ((ATNState)s).ast;
        //			if ( n!=null && s instanceof BlockEndState ) {
        //				ST rankST = stlib.getInstanceOf("decision-rank");
        //				ATNState alt = (ATNState)s;
        //				while ( alt!=null ) {
        //					rankST.add("states", getStateLabel(alt));
        //					if ( alt.transition(1) !=null ) {
        //						alt = (ATNState)alt.transition(1).target;
        //					}
        //					else {
        //						alt=null;
        //					}
        //				}
        //				dot.add("decisionRanks", rankST);
        //			}
        //		}
        //
        //        // make a DOT edge for each transition
        //		ST edgeST = null;
        //		for (int i = 0; i < s.getNumberOfTransitions(); i++) {
        //            Transition edge = (Transition) s.transition(i);
        //            if ( edge instanceof RuleTransition ) {
        //                RuleTransition rr = ((RuleTransition)edge);
        //                // don't jump to other rules, but display edge to follow node
        //                edgeST = stlib.getInstanceOf("edge");
        //				if ( rr.rule.g != grammar ) {
        //					edgeST.add("label", "<"+rr.rule.g.name+"."+rr.rule.name+">");
        //				}
        //				else {
        //					edgeST.add("label", "<"+rr.rule.name+">");
        //				}
        //				edgeST.add("src", getStateLabel(s));
        //				edgeST.add("target", getStateLabel(rr.followState));
        //				edgeST.add("arrowhead", arrowhead);
        //                dot.add("edges", edgeST);
        //				walkRuleATNCreatingDOT(dot, rr.followState);
        //                continue;
        //            }
        //			if ( edge instanceof ActionTransition ) {
        //				edgeST = stlib.getInstanceOf("action-edge");
        //			}
        //			else if ( edge instanceof PredicateTransition ) {
        //				edgeST = stlib.getInstanceOf("edge");
        //			}
        //			else if ( edge.isEpsilon() ) {
        //				edgeST = stlib.getInstanceOf("epsilon-edge");
        //			}
        //			else {
        //				edgeST = stlib.getInstanceOf("edge");
        //			}
        //			edgeST.add("label", getEdgeLabel(edge.toString(grammar)));
        //            edgeST.add("src", getStateLabel(s));
        //			edgeST.add("target", getStateLabel(edge.target));
        //			edgeST.add("arrowhead", arrowhead);
        //            dot.add("edges", edgeST);
        //            walkRuleATNCreatingDOT(dot, edge.target); // keep walkin'
        //        }
        //    }

        /** Fix edge strings so they print out in DOT properly;
         *  generate any gated predicates on edge too.
         */
        protected virtual string GetEdgeLabel(string label)
        {
            label = label.Replace("\\", "\\\\");
            label = label.Replace("\"", "\\\"");
            label = label.Replace("\n", "\\\\n");
            label = label.Replace("\r", "");
            return label;
        }

        protected virtual string GetStateLabel(ATNState s)
        {
            if (s == null)
                return "null";
            string stateLabel = "";

            if (s is BlockStartState)
            {
                stateLabel += "&rarr;\\n";
            }
            else if (s is BlockEndState)
            {
                stateLabel += "&larr;\\n";
            }

            stateLabel += s.stateNumber.ToString();

            if (s is PlusBlockStartState || s is PlusLoopbackState)
            {
                stateLabel += "+";
            }
            else if (s is StarBlockStartState || s is StarLoopEntryState || s is StarLoopbackState)
            {
                stateLabel += "*";
            }

            if (s is DecisionState && ((DecisionState)s).decision >= 0)
            {
                stateLabel = stateLabel + "\\nd=" + ((DecisionState)s).decision;
            }

            return stateLabel;
        }
    }
}
