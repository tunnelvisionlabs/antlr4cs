// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
