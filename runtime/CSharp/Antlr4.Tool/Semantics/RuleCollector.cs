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
    using Antlr4.Analysis;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;

    public class RuleCollector : GrammarTreeVisitor
    {
        /** which grammar are we checking */
        public Grammar g;
        public ErrorManager errMgr;

        // stuff to collect. this is the output
        public OrderedHashMap<string, Rule> rules = new OrderedHashMap<string, Rule>();
        public Runtime.Misc.MultiMap<string, GrammarAST> ruleToAltLabels = new Runtime.Misc.MultiMap<string, GrammarAST>();
        public IDictionary<string, string> altLabelToRuleName = new Dictionary<string, string>();

        public RuleCollector(Grammar g)
        {
            this.g = g;
            this.errMgr = g.tool.errMgr;
        }

        public override ErrorManager GetErrorManager()
        {
            return errMgr;
        }

        public virtual void Process(GrammarAST ast)
        {
            VisitGrammar(ast);
        }

        public override void DiscoverRule(RuleAST rule, GrammarAST ID,
                                 IList<GrammarAST> modifiers, ActionAST arg,
                                 ActionAST returns, GrammarAST thrws,
                                 GrammarAST options, ActionAST locals,
                                 IList<GrammarAST> actions,
                                 GrammarAST block)
        {
            int numAlts = block.ChildCount;
            Rule r;
            if (LeftRecursiveRuleAnalyzer.HasImmediateRecursiveRuleRefs(rule, ID.Text))
            {
                r = new LeftRecursiveRule(g, ID.Text, rule);
            }
            else
            {
                r = new Rule(g, ID.Text, rule, numAlts);
            }
            rules[r.name] = r;

            if (arg != null)
            {
                r.args = ScopeParser.ParseTypedArgList(arg, arg.Text, g);
                r.args.type = AttributeDict.DictType.ARG;
                r.args.ast = arg;
                arg.resolver = r.alt[currentOuterAltNumber];
            }

            if (returns != null)
            {
                r.retvals = ScopeParser.ParseTypedArgList(returns, returns.Text, g);
                r.retvals.type = AttributeDict.DictType.RET;
                r.retvals.ast = returns;
            }

            if (locals != null)
            {
                r.locals = ScopeParser.ParseTypedArgList(locals, locals.Text, g);
                r.locals.type = AttributeDict.DictType.LOCAL;
                r.locals.ast = locals;
            }

            foreach (GrammarAST a in actions)
            {
                // a = ^(AT ID ACTION)
                ActionAST action = (ActionAST)a.GetChild(1);
                r.namedActions[a.GetChild(0).Text] = action;
                action.resolver = r;
            }
        }

        public override void DiscoverOuterAlt(AltAST alt)
        {
            if (alt.altLabel != null)
            {
                ruleToAltLabels.Map(currentRuleName, alt.altLabel);
                string altLabel = alt.altLabel.Text;
                altLabelToRuleName[Utils.Capitalize(altLabel)] = currentRuleName;
                altLabelToRuleName[Utils.Decapitalize(altLabel)] = currentRuleName;
            }
        }

        public override void DiscoverLexerRule(RuleAST rule, GrammarAST ID, IList<GrammarAST> modifiers,
                                      GrammarAST block)
        {
            int numAlts = block.ChildCount;
            Rule r = new Rule(g, ID.Text, rule, numAlts);
            r.mode = currentModeName;
            if (modifiers.Count > 0)
                r.modifiers = modifiers;
            rules[r.name] = r;
        }
    }
}
