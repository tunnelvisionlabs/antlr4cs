// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Semantics
{
    using System.Collections.Generic;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using ANTLRStringStream = Antlr.Runtime.ANTLRStringStream;
    using IToken = Antlr.Runtime.IToken;

    /** Look for errors and deadcode stuff */
    public class UseDefAnalyzer
    {
        // side-effect: updates Alternative with refs in actions
        public static void TrackTokenRuleRefsInActions(Grammar g)
        {
            foreach (Rule r in g.rules.Values)
            {
                for (int i = 1; i <= r.numberOfAlts; i++)
                {
                    Alternative alt = r.alt[i];
                    foreach (ActionAST a in alt.actions)
                    {
                        ActionSniffer sniffer = new ActionSniffer(g, r, alt, a, a.Token);
                        sniffer.ExamineAction();
                    }
                }
            }
        }

        public static bool ActionIsContextDependent(ActionAST actionAST)
        {
            ANTLRStringStream @in = new ANTLRStringStream(actionAST.Token.Text);
            @in.Line = actionAST.Token.Line;
            @in.CharPositionInLine = actionAST.Token.CharPositionInLine;
            var listener = new ContextDependentListener();
            ActionSplitter splitter = new ActionSplitter(@in, listener);
            // forces eval, triggers listener methods
            splitter.GetActionTokens();
            return listener.dependent;
        }

        private class ContextDependentListener : BlankActionSplitterListener
        {
            public bool dependent;

            public override void NonLocalAttr(string expr, IToken x, IToken y)
            {
                dependent = true;
            }
            public override void QualifiedAttr(string expr, IToken x, IToken y)
            {
                dependent = true;
            }
            public override void SetAttr(string expr, IToken x, IToken rhs)
            {
                dependent = true;
            }
            public override void SetExprAttribute(string expr)
            {
                dependent = true;
            }
            public override void SetNonLocalAttr(string expr, IToken x, IToken y, IToken rhs)
            {
                dependent = true;
            }
            public override void Attr(string expr, IToken x)
            {
                dependent = true;
            }
        }

        /** Find all rules reachable from r directly or indirectly for all r in g */
        public static IDictionary<Rule, ISet<Rule>> GetRuleDependencies(Grammar g)
        {
            return GetRuleDependencies(g, g.rules.Values);
        }

        public static IDictionary<Rule, ISet<Rule>> GetRuleDependencies(LexerGrammar g, string modeName)
        {
            return GetRuleDependencies(g, g.modes[modeName]);
        }

        public static IDictionary<Rule, ISet<Rule>> GetRuleDependencies(Grammar g, ICollection<Rule> rules)
        {
            IDictionary<Rule, ISet<Rule>> dependencies = new Dictionary<Rule, ISet<Rule>>();

            foreach (Rule r in rules)
            {
                IList<GrammarAST> tokenRefs = r.ast.GetNodesWithType(ANTLRParser.TOKEN_REF);
                foreach (GrammarAST tref in tokenRefs)
                {
                    ISet<Rule> calls;
                    if (!dependencies.TryGetValue(r, out calls) || calls == null)
                    {
                        calls = new HashSet<Rule>();
                        dependencies[r] = calls;
                    }
                    calls.Add(g.GetRule(tref.Text));
                }
            }

            return dependencies;
        }
    }
}
