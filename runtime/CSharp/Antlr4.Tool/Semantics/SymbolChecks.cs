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
    using System.Linq;
    using Antlr4.Automata;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using CommonTree = Antlr.Runtime.Tree.CommonTree;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using TokenConstants = Antlr4.Runtime.TokenConstants;

    /** Check for symbol problems; no side-effects.  Inefficient to walk rules
     *  and such multiple times, but I like isolating all error checking outside
     *  of code that actually defines symbols etc...
     *
     *  Side-effect: strip away redef'd rules.
     */
    public class SymbolChecks
    {
        internal Grammar g;
        internal SymbolCollector collector;
        internal IDictionary<string, Rule> nameToRuleMap = new Dictionary<string, Rule>();
        internal ISet<string> tokenIDs = new HashSet<string>();
        internal IDictionary<string, ISet<string>> actionScopeToActionNames = new Dictionary<string, ISet<string>>();
        //DoubleKeyMap<string, string, GrammarAST> namedActions =
        //    new DoubleKeyMap<string, string, GrammarAST>();

        public ErrorManager errMgr;

        protected readonly ISet<string> reservedNames = new HashSet<string>(LexerATNFactory.GetCommonConstants());

        public SymbolChecks(Grammar g, SymbolCollector collector)
        {
            this.g = g;
            this.collector = collector;
            this.errMgr = g.tool.errMgr;

            foreach (GrammarAST tokenId in collector.tokenIDRefs)
            {
                tokenIDs.Add(tokenId.Text);
            }

            //System.Console.WriteLine("rules="+collector.rules);
            //System.Console.WriteLine("rulerefs="+collector.rulerefs);
            //System.Console.WriteLine("tokenIDRefs="+collector.tokenIDRefs);
            //System.Console.WriteLine("terminals="+collector.terminals);
            //System.Console.WriteLine("strings="+collector.strings);
            //System.Console.WriteLine("tokensDef="+collector.tokensDefs);
            //System.Console.WriteLine("actions="+collector.actions);
            //System.Console.WriteLine("scopes="+collector.scopes);
        }

        public virtual void Process()
        {
            // methods affect fields, but no side-effects outside this object
            // So, call order sensitive
            // First collect all rules for later use in checkForLabelConflict()
            if (g.rules != null)
            {
                foreach (Rule r in g.rules.Values)
                    nameToRuleMap[r.name] = r;
            }

            CheckReservedNames(g.rules.Values);
            CheckActionRedefinitions(collector.namedActions);
            CheckForTokenConflicts(collector.tokenIDRefs);  // sets tokenIDs
            CheckForLabelConflicts(g.rules.Values);
        }

        public virtual void CheckActionRedefinitions(IList<GrammarAST> actions)
        {
            if (actions == null)
                return;
            string scope = g.GetDefaultActionScope();
            string name;
            GrammarAST nameNode;
            foreach (GrammarAST ampersandAST in actions)
            {
                nameNode = (GrammarAST)ampersandAST.GetChild(0);
                if (ampersandAST.ChildCount == 2)
                {
                    name = nameNode.Text;
                }
                else
                {
                    scope = nameNode.Text;
                    name = ampersandAST.GetChild(1).Text;
                }
                ISet<string> scopeActions;
                if (!actionScopeToActionNames.TryGetValue(scope, out scopeActions) || scopeActions == null)
                {
                    // init scope
                    scopeActions = new HashSet<string>();
                    actionScopeToActionNames[scope] = scopeActions;
                }
                if (!scopeActions.Contains(name))
                {
                    scopeActions.Add(name);
                }
                else
                {
                    errMgr.GrammarError(ErrorType.ACTION_REDEFINITION,
                                              g.fileName, nameNode.Token, name);
                }
            }
        }

        public virtual void CheckForTokenConflicts(IList<GrammarAST> tokenIDRefs)
        {
            //foreach (GrammarAST a in tokenIDRefs)
            //{
            //    var t = a.Token;
            //    string ID = t.Text;
            //    tokenIDs.Add(ID);
            //}
        }

        /** Make sure a label doesn't conflict with another symbol.
         *  Labels must not conflict with: rules, tokens, scope names,
         *  return values, parameters, and rule-scope dynamic attributes
         *  defined in surrounding rule.  Also they must have same type
         *  for repeated defs.
         */
        public virtual void CheckForLabelConflicts(ICollection<Rule> rules)
        {
            foreach (Rule r in rules)
            {
                CheckForAttributeConflicts(r);
                IDictionary<string, LabelElementPair> labelNameSpace =
                    new Dictionary<string, LabelElementPair>();
                for (int i = 1; i <= r.numberOfAlts; i++)
                {
                    if (r.HasAltSpecificContexts())
                    {
                        labelNameSpace.Clear();
                    }

                    Alternative a = r.alt[i];
                    foreach (IList<LabelElementPair> pairs in a.labelDefs.Values)
                    {
                        foreach (LabelElementPair p in pairs)
                        {
                            CheckForLabelConflict(r, p.label);
                            string name = p.label.Text;
                            LabelElementPair prev;
                            if (!labelNameSpace.TryGetValue(name, out prev) || prev == null)
                                labelNameSpace[name] = p;
                            else
                                CheckForTypeMismatch(prev, p);
                        }
                    }
                }
            }
        }

        internal virtual void CheckForTypeMismatch(LabelElementPair prevLabelPair,
                                            LabelElementPair labelPair)
        {
            // label already defined; if same type, no problem
            if (prevLabelPair.type != labelPair.type)
            {
                string typeMismatchExpr = labelPair.type + "!=" + prevLabelPair.type;
                errMgr.GrammarError(
                    ErrorType.LABEL_TYPE_CONFLICT,
                    g.fileName,
                    labelPair.label.Token,
                    labelPair.label.Text,
                    typeMismatchExpr);
            }
        }

        public virtual void CheckForLabelConflict(Rule r, GrammarAST labelID)
        {
            string name = labelID.Text;
            if (nameToRuleMap.ContainsKey(name))
            {
                ErrorType etype = ErrorType.LABEL_CONFLICTS_WITH_RULE;
                errMgr.GrammarError(etype, g.fileName, labelID.Token, name, r.name);
            }

            if (tokenIDs.Contains(name))
            {
                ErrorType etype = ErrorType.LABEL_CONFLICTS_WITH_TOKEN;
                errMgr.GrammarError(etype, g.fileName, labelID.Token, name, r.name);
            }

            if (r.args != null && r.args.Get(name) != null)
            {
                ErrorType etype = ErrorType.LABEL_CONFLICTS_WITH_ARG;
                errMgr.GrammarError(etype, g.fileName, labelID.Token, name, r.name);
            }

            if (r.retvals != null && r.retvals.Get(name) != null)
            {
                ErrorType etype = ErrorType.LABEL_CONFLICTS_WITH_RETVAL;
                errMgr.GrammarError(etype, g.fileName, labelID.Token, name, r.name);
            }

            if (r.locals != null && r.locals.Get(name) != null)
            {
                ErrorType etype = ErrorType.LABEL_CONFLICTS_WITH_LOCAL;
                errMgr.GrammarError(etype, g.fileName, labelID.Token, name, r.name);
            }
        }

        public virtual void CheckForAttributeConflicts(Rule r)
        {
            CheckDeclarationRuleConflicts(r, r.args, nameToRuleMap.Keys, ErrorType.ARG_CONFLICTS_WITH_RULE);
            CheckDeclarationRuleConflicts(r, r.args, tokenIDs, ErrorType.ARG_CONFLICTS_WITH_TOKEN);

            CheckDeclarationRuleConflicts(r, r.retvals, nameToRuleMap.Keys, ErrorType.RETVAL_CONFLICTS_WITH_RULE);
            CheckDeclarationRuleConflicts(r, r.retvals, tokenIDs, ErrorType.RETVAL_CONFLICTS_WITH_TOKEN);

            CheckDeclarationRuleConflicts(r, r.locals, nameToRuleMap.Keys, ErrorType.LOCAL_CONFLICTS_WITH_RULE);
            CheckDeclarationRuleConflicts(r, r.locals, tokenIDs, ErrorType.LOCAL_CONFLICTS_WITH_TOKEN);

            CheckLocalConflictingDeclarations(r, r.retvals, r.args, ErrorType.RETVAL_CONFLICTS_WITH_ARG);
            CheckLocalConflictingDeclarations(r, r.locals, r.args, ErrorType.LOCAL_CONFLICTS_WITH_ARG);
            CheckLocalConflictingDeclarations(r, r.locals, r.retvals, ErrorType.LOCAL_CONFLICTS_WITH_RETVAL);
        }

        protected virtual void CheckDeclarationRuleConflicts([NotNull] Rule r, [Nullable] AttributeDict attributes, [NotNull] ICollection<string> ruleNames, [NotNull] ErrorType errorType)
        {
            if (attributes == null)
            {
                return;
            }

            foreach (Attribute attribute in attributes.attributes.Values)
            {
                if (ruleNames.Contains(attribute.name))
                {
                    errMgr.GrammarError(
                        errorType,
                        g.fileName,
                        attribute.token != null ? attribute.token : ((GrammarAST)r.ast.GetChild(0)).Token,
                        attribute.name,
                        r.name);
                }
            }
        }

        protected virtual void CheckLocalConflictingDeclarations([NotNull] Rule r, [Nullable] AttributeDict attributes, [Nullable] AttributeDict referenceAttributes, [NotNull] ErrorType errorType)
        {
            if (attributes == null || referenceAttributes == null)
            {
                return;
            }

            ISet<string> conflictingKeys = attributes.Intersection(referenceAttributes);
            foreach (string key in conflictingKeys)
            {
                errMgr.GrammarError(
                    errorType,
                    g.fileName,
                    attributes.Get(key).token != null ? attributes.Get(key).token : ((GrammarAST)r.ast.GetChild(0)).Token,
                    key,
                    r.name);
            }
        }

        protected virtual void CheckReservedNames([NotNull] ICollection<Rule> rules)
        {
            foreach (Rule rule in rules)
            {
                if (reservedNames.Contains(rule.name))
                {
                    errMgr.GrammarError(ErrorType.RESERVED_RULE_NAME, g.fileName, ((GrammarAST)rule.ast.GetChild(0)).Token, rule.name);
                }
            }
        }

        public virtual void CheckForModeConflicts(Grammar g)
        {
            if (g.IsLexer())
            {
                LexerGrammar lexerGrammar = (LexerGrammar)g;
                foreach (string modeName in lexerGrammar.modes.Keys)
                {
                    if (!modeName.Equals("DEFAULT_MODE") && reservedNames.Contains(modeName))
                    {
                        Rule rule = lexerGrammar.modes[modeName].First();
                        g.tool.errMgr.GrammarError(ErrorType.MODE_CONFLICTS_WITH_COMMON_CONSTANTS, g.fileName, ((CommonTree)rule.ast.Parent).Token, modeName);
                    }

                    if (g.GetTokenType(modeName) != TokenConstants.InvalidType)
                    {
                        Rule rule = lexerGrammar.modes[modeName].First();
                        g.tool.errMgr.GrammarError(ErrorType.MODE_CONFLICTS_WITH_TOKEN, g.fileName, ((CommonTree)rule.ast.Parent).Token, modeName);
                    }
                }
            }
        }

        // CAN ONLY CALL THE TWO NEXT METHODS AFTER GRAMMAR HAS RULE DEFS (see semanticpipeline)

        public virtual void CheckRuleArgs(Grammar g, IList<GrammarAST> rulerefs)
        {
            if (rulerefs == null)
                return;
            foreach (GrammarAST @ref in rulerefs)
            {
                string ruleName = @ref.Text;
                Rule r = g.GetRule(ruleName);
                GrammarAST arg = (GrammarAST)@ref.GetFirstChildWithType(ANTLRParser.ARG_ACTION);
                if (arg != null && (r == null || r.args == null))
                {
                    errMgr.GrammarError(ErrorType.RULE_HAS_NO_ARGS,
                                              g.fileName, @ref.Token, ruleName);

                }
                else if (arg == null && (r != null && r.args != null))
                {
                    errMgr.GrammarError(ErrorType.MISSING_RULE_ARGS,
                                              g.fileName, @ref.Token, ruleName);
                }
            }
        }

        public virtual void CheckForQualifiedRuleIssues(Grammar g, IList<GrammarAST> qualifiedRuleRefs)
        {
            foreach (GrammarAST dot in qualifiedRuleRefs)
            {
                GrammarAST grammar = (GrammarAST)dot.GetChild(0);
                GrammarAST rule = (GrammarAST)dot.GetChild(1);
                g.tool.Log("semantics", grammar.Text + "." + rule.Text);
                Grammar @delegate = g.GetImportedGrammar(grammar.Text);
                if (@delegate == null)
                {
                    errMgr.GrammarError(ErrorType.NO_SUCH_GRAMMAR_SCOPE,
                                              g.fileName, grammar.Token, grammar.Text,
                                              rule.Text);
                }
                else
                {
                    if (g.GetRule(grammar.Text, rule.Text) == null)
                    {
                        errMgr.GrammarError(ErrorType.NO_SUCH_RULE_IN_SCOPE,
                                                  g.fileName, rule.Token, grammar.Text,
                                                  rule.Text);
                    }
                }
            }
        }
    }
}
