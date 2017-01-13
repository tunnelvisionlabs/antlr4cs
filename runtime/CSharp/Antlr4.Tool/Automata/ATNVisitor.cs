// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Automata
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Atn;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

    /** A simple visitor that walks everywhere it can go starting from s,
     *  without going into an infinite cycle. Override and implement
     *  visitState() to provide functionality.
     */
    public class ATNVisitor
    {
        public virtual void Visit([NotNull] ATNState s)
        {
            Visit_(s, new HashSet<int>());
        }

        public virtual void Visit_([NotNull] ATNState s, [NotNull] ISet<int> visited)
        {
            if (!visited.Add(s.stateNumber))
                return;
            visited.Add(s.stateNumber);

            VisitState(s);
            int n = s.NumberOfTransitions;
            for (int i = 0; i < n; i++)
            {
                Transition t = s.Transition(i);
                Visit_(t.target, visited);
            }
        }

        public virtual void VisitState([NotNull] ATNState s)
        {
        }
    }
}
