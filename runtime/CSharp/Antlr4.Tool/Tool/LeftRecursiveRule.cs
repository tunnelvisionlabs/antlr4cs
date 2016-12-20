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

namespace Antlr4.Tool
{
    using System.Collections.Generic;
    using Antlr4.Analysis;
    using Antlr4.Misc;
    using Antlr4.Tool.Ast;
    using Tuple = System.Tuple;

    public class LeftRecursiveRule : Rule
    {
        public IList<LeftRecursiveRuleAltInfo> recPrimaryAlts;
        public OrderedHashMap<int, LeftRecursiveRuleAltInfo> recOpAlts;
        public RuleAST originalAST;

        /** Did we delete any labels on direct left-recur refs? Points at ID of ^(= ID el) */
        public IList<System.Tuple<GrammarAST, string>> leftRecursiveRuleRefLabels =
            new List<System.Tuple<GrammarAST, string>>();

        public LeftRecursiveRule(Grammar g, string name, RuleAST ast)
            : base(g, name, ast, 1)
        {
            originalAST = ast;
            alt = new Alternative[numberOfAlts + 1]; // always just one
            for (int i = 1; i <= numberOfAlts; i++)
                alt[i] = new Alternative(this, i);
        }

        public override bool HasAltSpecificContexts()
        {
            return base.HasAltSpecificContexts() || GetAltLabels() != null;
        }

        public override int GetOriginalNumberOfAlts()
        {
            int n = 0;
            if (recPrimaryAlts != null)
                n += recPrimaryAlts.Count;
            if (recOpAlts != null)
                n += recOpAlts.Count;
            return n;
        }

        public RuleAST GetOriginalAST()
        {
            return originalAST;
        }

        public override IList<AltAST> GetUnlabeledAltASTs()
        {
            IList<AltAST> alts = new List<AltAST>();
            foreach (LeftRecursiveRuleAltInfo altInfo in recPrimaryAlts)
            {
                if (altInfo.altLabel == null)
                    alts.Add(altInfo.originalAltAST);
            }
            for (int i = 0; i < recOpAlts.Count; i++)
            {
                LeftRecursiveRuleAltInfo altInfo = recOpAlts.GetElement(i);
                if (altInfo.altLabel == null)
                    alts.Add(altInfo.originalAltAST);
            }
            if (alts.Count == 0)
                return null;
            return alts;
        }

        /** Return an array that maps predicted alt from primary decision
         *  to original alt of rule. For following rule, return [0, 2, 4]
         *
            e : e '*' e
              | INT
              | e '+' e
              | ID
              ;

         *  That maps predicted alt 1 to original alt 2 and predicted 2 to alt 4.
         *
         *  @since 4.5.1
         */
        public virtual int[] GetPrimaryAlts()
        {
            if (recPrimaryAlts.Count == 0)
                return null;
            int[] alts = new int[recPrimaryAlts.Count + 1];
            for (int i = 0; i < recPrimaryAlts.Count; i++)
            { // recPrimaryAlts is a List not Map like recOpAlts
                LeftRecursiveRuleAltInfo altInfo = recPrimaryAlts[i];
                alts[i + 1] = altInfo.altNum;
            }
            return alts;
        }

        /** Return an array that maps predicted alt from recursive op decision
         *  to original alt of rule. For following rule, return [0, 1, 3]
         *
            e : e '*' e
              | INT
              | e '+' e
              | ID
              ;

         *  That maps predicted alt 1 to original alt 1 and predicted 2 to alt 3.
         *
         *  @since 4.5.1
         */
        public virtual int[] GetRecursiveOpAlts()
        {
            if (recOpAlts.Count == 0)
                return null;
            int[] alts = new int[recOpAlts.Count + 1];
            int alt = 1;
            foreach (LeftRecursiveRuleAltInfo altInfo in recOpAlts.Values)
            {
                alts[alt] = altInfo.altNum;
                alt++; // recOpAlts has alts possibly with gaps
            }
            return alts;
        }

        /** Get -&gt; labels from those alts we deleted for left-recursive rules. */
        public override IDictionary<string, IList<System.Tuple<int, AltAST>>> GetAltLabels()
        {
            IDictionary<string, IList<System.Tuple<int, AltAST>>> labels = new Dictionary<string, IList<System.Tuple<int, AltAST>>>();
            IDictionary<string, IList<System.Tuple<int, AltAST>>> normalAltLabels = base.GetAltLabels();
            if (normalAltLabels != null)
            {
                foreach (var pair in normalAltLabels)
                    labels[pair.Key] = pair.Value;
            }

            if (recPrimaryAlts != null)
            {
                foreach (LeftRecursiveRuleAltInfo altInfo in recPrimaryAlts)
                {
                    if (altInfo.altLabel != null)
                    {
                        IList<System.Tuple<int, AltAST>> pairs;
                        if (!labels.TryGetValue(altInfo.altLabel, out pairs) || pairs == null)
                        {
                            pairs = new List<System.Tuple<int, AltAST>>();
                            labels[altInfo.altLabel] = pairs;
                        }

                        pairs.Add(Tuple.Create(altInfo.altNum, altInfo.originalAltAST));
                    }
                }
            }
            if (recOpAlts != null)
            {
                for (int i = 0; i < recOpAlts.Count; i++)
                {
                    LeftRecursiveRuleAltInfo altInfo = recOpAlts.GetElement(i);
                    if (altInfo.altLabel != null)
                    {
                        IList<System.Tuple<int, AltAST>> pairs;
                        if (!labels.TryGetValue(altInfo.altLabel, out pairs) || pairs == null)
                        {
                            pairs = new List<System.Tuple<int, AltAST>>();
                            labels[altInfo.altLabel] = pairs;
                        }

                        pairs.Add(Tuple.Create(altInfo.altNum, altInfo.originalAltAST));
                    }
                }
            }
            if (labels.Count == 0)
                return null;
            return labels;
        }
    }
}
