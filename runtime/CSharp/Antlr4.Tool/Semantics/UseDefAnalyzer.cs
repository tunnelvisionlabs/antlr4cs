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
