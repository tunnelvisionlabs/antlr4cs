// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Semantics
{
    using System.Collections.Generic;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using CommonTree = Antlr.Runtime.Tree.CommonTree;
    using IToken = Antlr.Runtime.IToken;
    using ITree = Antlr.Runtime.Tree.ITree;
    using Path = System.IO.Path;

    /** No side-effects except for setting options into the appropriate node.
     *  TODO:  make the side effects into a separate pass this
     *
     * Invokes check rules for these:
     *
     * FILE_AND_GRAMMAR_NAME_DIFFER
     * LEXER_RULES_NOT_ALLOWED
     * PARSER_RULES_NOT_ALLOWED
     * CANNOT_ALIAS_TOKENS
     * ARGS_ON_TOKEN_REF
     * ILLEGAL_OPTION
     * REWRITE_OR_OP_WITH_NO_OUTPUT_OPTION
     * NO_RULES
     * REWRITE_FOR_MULTI_ELEMENT_ALT
     * HETERO_ILLEGAL_IN_REWRITE_ALT
     * AST_OP_WITH_NON_AST_OUTPUT_OPTION
     * AST_OP_IN_ALT_WITH_REWRITE
     * CONFLICTING_OPTION_IN_TREE_FILTER
     * WILDCARD_AS_ROOT
     * INVALID_IMPORT
     * TOKEN_VOCAB_IN_DELEGATE
     * IMPORT_NAME_CLASH
     * REPEATED_PREQUEL
     * TOKEN_NAMES_MUST_START_UPPER
     */
    public class BasicSemanticChecks : GrammarTreeVisitor
    {
        /** Set of valid imports.  Maps delegate to set of delegator grammar types.
         *  validDelegations.get(LEXER) gives list of the kinds of delegators
         *  that can import lexers.
         */
        public static Runtime.Misc.MultiMap<int, int> validImportTypes = new Runtime.Misc.MultiMap<int, int>();
        static BasicSemanticChecks()
        {
            validImportTypes.Map(ANTLRParser.LEXER, ANTLRParser.LEXER);
            validImportTypes.Map(ANTLRParser.LEXER, ANTLRParser.COMBINED);

            validImportTypes.Map(ANTLRParser.PARSER, ANTLRParser.PARSER);
            validImportTypes.Map(ANTLRParser.PARSER, ANTLRParser.COMBINED);

            validImportTypes.Map(ANTLRParser.COMBINED, ANTLRParser.COMBINED);
        }

        public Grammar g;
        public RuleCollector ruleCollector;
        public ErrorManager errMgr;

        /**
         * When this is {@code true}, the semantic checks will report
         * {@link ErrorType#UNRECOGNIZED_ASSOC_OPTION} where appropriate. This may
         * be set to {@code false} to disable this specific check.
         *
         * <p>The default value is {@code true}.</p>
         */
        public bool checkAssocElementOption = true;

        /**
         * This field is used for reporting the {@link ErrorType#MODE_WITHOUT_RULES}
         * error when necessary.
         */
        protected int nonFragmentRuleCount;

        /**
         * This is {@code true} from the time {@link #discoverLexerRule} is called
         * for a lexer rule with the {@code fragment} modifier until
         * {@link #exitLexerRule} is called.
         */
        private bool inFragmentRule;

        public BasicSemanticChecks(Grammar g, RuleCollector ruleCollector)
        {
            this.g = g;
            this.ruleCollector = ruleCollector;
            this.errMgr = g.tool.errMgr;
        }

        public override ErrorManager GetErrorManager()
        {
            return errMgr;
        }

        public virtual void Process()
        {
            VisitGrammar(g.ast);
        }

        // Routines to route visitor traffic to the checking routines

        public override void DiscoverGrammar(GrammarRootAST root, GrammarAST ID)
        {
            CheckGrammarName(ID.Token);
        }

        public override void FinishPrequels(GrammarAST firstPrequel)
        {
            if (firstPrequel == null)
                return;
            GrammarAST parent = (GrammarAST)firstPrequel.Parent;
            IList<GrammarAST> options = parent.GetAllChildrenWithType(OPTIONS);
            IList<GrammarAST> imports = parent.GetAllChildrenWithType(IMPORT);
            IList<GrammarAST> tokens = parent.GetAllChildrenWithType(TOKENS_SPEC);
            CheckNumPrequels(options, imports, tokens);
        }

        public override void ImportGrammar(GrammarAST label, GrammarAST ID)
        {
            CheckImport(ID.Token);
        }

        public override void DiscoverRules(GrammarAST rules)
        {
            CheckNumRules(rules);
        }

        protected override void EnterMode(GrammarAST tree)
        {
            nonFragmentRuleCount = 0;
        }

        protected override void ExitMode(GrammarAST tree)
        {
            if (nonFragmentRuleCount == 0)
            {
                IToken token = tree.Token;
                string name = "?";
                if (tree.ChildCount > 0)
                {
                    name = tree.GetChild(0).Text;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "?";
                    }

                    token = ((GrammarAST)tree.GetChild(0)).Token;
                }

                g.tool.errMgr.GrammarError(ErrorType.MODE_WITHOUT_RULES, g.fileName, token, name, g);
            }
        }

        public override void ModeDef(GrammarAST m, GrammarAST ID)
        {
            if (!g.IsLexer())
            {
                g.tool.errMgr.GrammarError(ErrorType.MODE_NOT_IN_LEXER, g.fileName,
                                           ID.Token, ID.Token.Text, g);
            }
        }

        public override void DiscoverRule(RuleAST rule, GrammarAST ID,
                                 IList<GrammarAST> modifiers,
                                 ActionAST arg, ActionAST returns,
                                 GrammarAST thrws, GrammarAST options,
                                 ActionAST locals,
                                 IList<GrammarAST> actions, GrammarAST block)
        {
            // TODO: chk that all or no alts have "# label"
            CheckInvalidRuleDef(ID.Token);
        }

        public override void DiscoverLexerRule(RuleAST rule, GrammarAST ID, IList<GrammarAST> modifiers,
                                      GrammarAST block)
        {
            CheckInvalidRuleDef(ID.Token);

            if (modifiers != null)
            {
                foreach (GrammarAST tree in modifiers)
                {
                    if (tree.Type == ANTLRParser.FRAGMENT)
                    {
                        inFragmentRule = true;
                    }
                }
            }

            if (!inFragmentRule)
            {
                nonFragmentRuleCount++;
            }
        }

        protected override void ExitLexerRule(GrammarAST tree)
        {
            inFragmentRule = false;
        }

        public override void RuleRef(GrammarAST @ref, ActionAST arg)
        {
            CheckInvalidRuleRef(@ref.Token);
        }

        public override void RuleOption(GrammarAST ID, GrammarAST valueAST)
        {
            CheckOptions((GrammarAST)ID.GetAncestor(RULE), ID.Token, valueAST);
        }

        public override void BlockOption(GrammarAST ID, GrammarAST valueAST)
        {
            CheckOptions((GrammarAST)ID.GetAncestor(BLOCK), ID.Token, valueAST);
        }

        public override void GrammarOption(GrammarAST ID, GrammarAST valueAST)
        {
            bool ok = CheckOptions(g.ast, ID.Token, valueAST);
            //if (ok)
            //    g.ast.SetOption(ID.Text, value);
        }

        public override void DefineToken(GrammarAST ID)
        {
            CheckTokenDefinition(ID.Token);
        }

        protected override void EnterChannelsSpec(GrammarAST tree)
        {
            if (g.IsParser())
            {
                g.tool.errMgr.GrammarError(ErrorType.CHANNELS_BLOCK_IN_PARSER_GRAMMAR, g.fileName, tree.Token);
            }
            else if (g.IsCombined())
            {
                g.tool.errMgr.GrammarError(ErrorType.CHANNELS_BLOCK_IN_COMBINED_GRAMMAR, g.fileName, tree.Token);
            }
        }

        public override void DefineChannel(GrammarAST ID)
        {
            CheckChannelDefinition(ID.Token);
        }

        public override void ElementOption(GrammarASTWithOptions elem, GrammarAST ID, GrammarAST valueAST)
        {
            //string v = null;
            bool ok = CheckElementOptions(elem, ID, valueAST);
            //if (ok)
            //{
            //    if (v != null)
            //    {
            //        t.setOption(ID.Text, v);
            //    }
            //    else
            //    {
            //        t.setOption(TerminalAST.defaultTokenOption, v);
            //    }
            //}
        }

        /**
         * This method detects the following errors, which require analysis across
         * the whole grammar for rules according to their base context.
         *
         * <ul>
         * <li>{@link ErrorType#RULE_WITH_TOO_FEW_ALT_LABELS_GROUP}</li>
         * <li>{@link ErrorType#BASE_CONTEXT_MUST_BE_RULE_NAME}</li>
         * <li>{@link ErrorType#BASE_CONTEXT_CANNOT_BE_TRANSITIVE}</li>
         * <li>{@link ErrorType#LEXER_RULE_CANNOT_HAVE_BASE_CONTEXT}</li>
         * </ul>
         */
        public override void FinishGrammar(GrammarRootAST root, GrammarAST ID)
        {
            Runtime.Misc.MultiMap<string, Rule> baseContexts = new Runtime.Misc.MultiMap<string, Rule>();
            foreach (Rule r in ruleCollector.rules.Values)
            {
                GrammarAST optionAST = r.ast.GetOptionAST("baseContext");

                if (r.ast.IsLexerRule())
                {
                    if (optionAST != null)
                    {
                        IToken errorToken = optionAST.Token;
                        g.tool.errMgr.GrammarError(ErrorType.LEXER_RULE_CANNOT_HAVE_BASE_CONTEXT,
                                                   g.fileName, errorToken, r.name);
                    }

                    continue;
                }

                baseContexts.Map(r.GetBaseContext(), r);

                if (optionAST != null)
                {
                    Rule targetRule;
                    ruleCollector.rules.TryGetValue(r.GetBaseContext(), out targetRule);
                    bool targetSpecifiesBaseContext =
                        targetRule != null
                        && targetRule.ast != null
                        && (targetRule.ast.GetOptionAST("baseContext") != null
                            || !targetRule.name.Equals(targetRule.GetBaseContext()));

                    if (targetSpecifiesBaseContext)
                    {
                        IToken errorToken = optionAST.Token;
                        g.tool.errMgr.GrammarError(ErrorType.BASE_CONTEXT_CANNOT_BE_TRANSITIVE,
                                                   g.fileName, errorToken, r.name);
                    }
                }

                // It's unlikely for this to occur when optionAST is null, but checking
                // anyway means it can detect certain errors within the logic of the
                // Tool itself.
                if (!ruleCollector.rules.ContainsKey(r.GetBaseContext()))
                {
                    IToken errorToken;
                    if (optionAST != null)
                    {
                        errorToken = optionAST.Token;
                    }
                    else
                    {
                        errorToken = ((CommonTree)r.ast.GetChild(0)).Token;
                    }

                    g.tool.errMgr.GrammarError(ErrorType.BASE_CONTEXT_MUST_BE_RULE_NAME,
                                               g.fileName, errorToken, r.name);
                }
            }

            foreach (KeyValuePair<string, IList<Rule>> entry in baseContexts)
            {
                // suppress RULE_WITH_TOO_FEW_ALT_LABELS_GROUP if RULE_WITH_TOO_FEW_ALT_LABELS
                // would already have been reported for at least one rule with this
                // base context.
                bool suppressError = false;
                int altLabelCount = 0;
                int outerAltCount = 0;
                foreach (Rule rule in entry.Value)
                {
                    outerAltCount += rule.numberOfAlts;
                    IList<GrammarAST> altLabels;
                    if (ruleCollector.ruleToAltLabels.TryGetValue(rule.name, out altLabels) && altLabels != null && altLabels.Count > 0)
                    {
                        if (altLabels.Count != rule.numberOfAlts)
                        {
                            suppressError = true;
                            break;
                        }

                        altLabelCount += altLabels.Count;
                    }
                }

                if (suppressError)
                {
                    continue;
                }

                if (altLabelCount != 0 && altLabelCount != outerAltCount)
                {
                    Rule errorRule = entry.Value[0];
                    g.tool.errMgr.GrammarError(ErrorType.RULE_WITH_TOO_FEW_ALT_LABELS_GROUP,
                                               g.fileName, ((CommonTree)errorRule.ast.GetChild(0)).Token, errorRule.name);
                }
            }
        }

        public override void FinishRule(RuleAST rule, GrammarAST ID, GrammarAST block)
        {
            if (rule.IsLexerRule())
                return;
            BlockAST blk = (BlockAST)rule.GetFirstChildWithType(BLOCK);
            int nalts = blk.ChildCount;
            GrammarAST idAST = (GrammarAST)rule.GetChild(0);
            for (int i = 0; i < nalts; i++)
            {
                AltAST altAST = (AltAST)blk.GetChild(i);
                if (altAST.altLabel != null)
                {
                    string altLabel = altAST.altLabel.Text;
                    // first check that label doesn't conflict with a rule
                    // label X or x can't be rule x.
                    Rule r;
                    if (ruleCollector.rules.TryGetValue(Utils.Decapitalize(altLabel), out r) && r != null)
                    {
                        g.tool.errMgr.GrammarError(ErrorType.ALT_LABEL_CONFLICTS_WITH_RULE,
                                                   g.fileName, altAST.altLabel.Token,
                                                   altLabel,
                                                   r.name);
                    }
                    // Now verify that label X or x doesn't conflict with label
                    // in another rule. altLabelToRuleName has both X and x mapped.
                    string prevRuleForLabel;
                    if (ruleCollector.altLabelToRuleName.TryGetValue(altLabel, out prevRuleForLabel) && prevRuleForLabel != null && !prevRuleForLabel.Equals(rule.GetRuleName()))
                    {
                        g.tool.errMgr.GrammarError(ErrorType.ALT_LABEL_REDEF,
                                                   g.fileName, altAST.altLabel.Token,
                                                   altLabel,
                                                   rule.GetRuleName(),
                                                   prevRuleForLabel);
                    }
                }
            }
            IList<GrammarAST> altLabels;
            int numAltLabels = 0;
            if (ruleCollector.ruleToAltLabels.TryGetValue(rule.GetRuleName(), out altLabels) && altLabels != null)
                numAltLabels = altLabels.Count;
            if (numAltLabels > 0 && nalts != numAltLabels)
            {
                g.tool.errMgr.GrammarError(ErrorType.RULE_WITH_TOO_FEW_ALT_LABELS,
                                           g.fileName, idAST.Token, rule.GetRuleName());
            }
        }

        // Routines to do the actual work of checking issues with a grammar.
        // They are triggered by the visitor methods above.

        internal virtual void CheckGrammarName(IToken nameToken)
        {
            string fullyQualifiedName = nameToken.InputStream.SourceName;
            if (fullyQualifiedName == null)
            {
                // This wasn't read from a file.
                return;
            }

            string f = fullyQualifiedName;
            string fileName = Path.GetFileName(f);
            if (g.originalGrammar != null)
                return; // don't warn about diff if this is implicit lexer
            if (!Utils.StripFileExtension(fileName).Equals(nameToken.Text) &&
                 !fileName.Equals(Grammar.GRAMMAR_FROM_STRING_NAME))
            {
                g.tool.errMgr.GrammarError(ErrorType.FILE_AND_GRAMMAR_NAME_DIFFER,
                                           fileName, nameToken, nameToken.Text, fileName);
            }
        }

        internal virtual void CheckNumRules(GrammarAST rulesNode)
        {
            if (rulesNode.ChildCount == 0)
            {
                GrammarAST root = (GrammarAST)rulesNode.Parent;
                GrammarAST IDNode = (GrammarAST)root.GetChild(0);
                g.tool.errMgr.GrammarError(ErrorType.NO_RULES, g.fileName,
                        null, IDNode.Text, g);
            }
        }

        internal virtual void CheckNumPrequels(IList<GrammarAST> options,
                              IList<GrammarAST> imports,
                              IList<GrammarAST> tokens)
        {
            IList<IToken> secondOptionTokens = new List<IToken>();
            if (options != null && options.Count > 1)
            {
                secondOptionTokens.Add(options[1].Token);
            }
            if (imports != null && imports.Count > 1)
            {
                secondOptionTokens.Add(imports[1].Token);
            }
            if (tokens != null && tokens.Count > 1)
            {
                secondOptionTokens.Add(tokens[1].Token);
            }
            foreach (IToken t in secondOptionTokens)
            {
                string fileName = t.InputStream.SourceName;
                g.tool.errMgr.GrammarError(ErrorType.REPEATED_PREQUEL,
                                           fileName, t);
            }
        }

        internal virtual void CheckInvalidRuleDef(IToken ruleID)
        {
            string fileName = null;
            if (ruleID.InputStream != null)
            {
                fileName = ruleID.InputStream.SourceName;
            }
            if (g.IsLexer() && char.IsLower(ruleID.Text[0]))
            {
                g.tool.errMgr.GrammarError(ErrorType.PARSER_RULES_NOT_ALLOWED,
                                           fileName, ruleID, ruleID.Text);
            }
            if (g.IsParser() &&
                Grammar.IsTokenName(ruleID.Text))
            {
                g.tool.errMgr.GrammarError(ErrorType.LEXER_RULES_NOT_ALLOWED,
                                           fileName, ruleID, ruleID.Text);
            }
        }

        internal virtual void CheckInvalidRuleRef(IToken ruleID)
        {
            string fileName = ruleID.InputStream.SourceName;
            if (g.IsLexer() && char.IsLower(ruleID.Text[0]))
            {
                g.tool.errMgr.GrammarError(ErrorType.PARSER_RULE_REF_IN_LEXER_RULE,
                                           fileName, ruleID, ruleID.Text, currentRuleName);
            }
        }

        internal virtual void CheckTokenDefinition(IToken tokenID)
        {
            string fileName = tokenID.InputStream.SourceName;
            if (!Grammar.IsTokenName(tokenID.Text))
            {
                g.tool.errMgr.GrammarError(ErrorType.TOKEN_NAMES_MUST_START_UPPER,
                                           fileName,
                                           tokenID,
                                           tokenID.Text);
            }
        }

        internal virtual void CheckChannelDefinition(IToken tokenID)
        {
        }

        protected override void EnterLexerElement(GrammarAST tree)
        {
        }

        protected override void EnterLexerCommand(GrammarAST tree)
        {
            CheckElementIsOuterMostInSingleAlt(tree);

            if (inFragmentRule)
            {
                string fileName = tree.Token.InputStream.SourceName;
                string ruleName = currentRuleName;
                g.tool.errMgr.GrammarError(ErrorType.FRAGMENT_ACTION_IGNORED, fileName, tree.Token, ruleName);
            }
        }

        public override void ActionInAlt(ActionAST action)
        {
            if (inFragmentRule)
            {
                string fileName = action.Token.InputStream.SourceName;
                string ruleName = currentRuleName;
                g.tool.errMgr.GrammarError(ErrorType.FRAGMENT_ACTION_IGNORED, fileName, action.Token, ruleName);
            }
        }

        /**
         Make sure that action is last element in outer alt; here action,
         a2, z, and zz are bad, but a3 is ok:
         (RULE A (BLOCK (ALT {action} 'a')))
         (RULE B (BLOCK (ALT (BLOCK (ALT {a2} 'x') (ALT 'y')) {a3})))
         (RULE C (BLOCK (ALT 'd' {z}) (ALT 'e' {zz})))
         */
        protected virtual void CheckElementIsOuterMostInSingleAlt(GrammarAST tree)
        {
            CommonTree alt = (CommonTree)tree.Parent;
            CommonTree blk = (CommonTree)alt.Parent;
            bool outerMostAlt = blk.Parent.Type == RULE;
            ITree rule = tree.GetAncestor(RULE);
            string fileName = tree.Token.InputStream.SourceName;
            if (!outerMostAlt || blk.ChildCount > 1)
            {
                ErrorType e = ErrorType.LEXER_COMMAND_PLACEMENT_ISSUE;
                g.tool.errMgr.GrammarError(e,
                                           fileName,
                                           tree.Token,
                                           rule.GetChild(0).Text);

            }
        }

        public override void Label(GrammarAST op, GrammarAST ID, GrammarAST element)
        {
            switch (element.Type)
            {
            // token atoms
            case TOKEN_REF:
            case STRING_LITERAL:
            case RANGE:
            // token sets
            case SET:
            case NOT:
            // rule atoms
            case RULE_REF:
            case WILDCARD:
                return;

            default:
                string fileName = ID.Token.InputStream.SourceName;
                g.tool.errMgr.GrammarError(ErrorType.LABEL_BLOCK_NOT_A_SET, fileName, ID.Token, ID.Text);
                break;
            }
        }

        protected override void EnterLabeledLexerElement(GrammarAST tree)
        {
            IToken label = ((GrammarAST)tree.GetChild(0)).Token;
            g.tool.errMgr.GrammarError(ErrorType.V3_LEXER_LABEL,
                                       g.fileName,
                                       label,
                                       label.Text);
        }

        protected override void EnterTerminal(GrammarAST tree)
        {
            string text = tree.Text;
            if (text.Equals("''"))
            {
                g.tool.errMgr.GrammarError(ErrorType.EMPTY_STRINGS_AND_SETS_NOT_ALLOWED, g.fileName, tree.Token);
            }
        }

        /** Check option is appropriate for grammar, rule, subrule */
        internal virtual bool CheckOptions(GrammarAST parent,
                             IToken optionID,
                             GrammarAST valueAST)
        {
            bool ok = true;
            if (parent.Type == ANTLRParser.BLOCK)
            {
                if (g.IsLexer() && !Grammar.LexerBlockOptions.Contains(optionID.Text))
                { // block
                    g.tool.errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                               g.fileName,
                                               optionID,
                                               optionID.Text);
                    ok = false;
                }
                if (!g.IsLexer() && !Grammar.ParserBlockOptions.Contains(optionID.Text))
                { // block
                    g.tool.errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                               g.fileName,
                                               optionID,
                                               optionID.Text);
                    ok = false;
                }
            }
            else if (parent.Type == ANTLRParser.RULE)
            {
                if (!Grammar.ruleOptions.Contains(optionID.Text))
                { // rule
                    g.tool.errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                               g.fileName,
                                               optionID,
                                               optionID.Text);
                    ok = false;
                }
            }
            else if (parent.Type == ANTLRParser.GRAMMAR &&
                      !LegalGrammarOption(optionID.Text))
            { // grammar
                g.tool.errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                           g.fileName,
                                           optionID,
                                           optionID.Text);
                ok = false;
            }

            return ok;
        }

        /** Check option is appropriate for elem; parent of ID is ELEMENT_OPTIONS */
        internal virtual bool CheckElementOptions(GrammarASTWithOptions elem,
                                    GrammarAST ID,
                                    GrammarAST valueAST)
        {
            if (checkAssocElementOption && ID != null && "assoc".Equals(ID.Text))
            {
                if (elem.Type != ANTLRParser.ALT)
                {
                    IToken optionID = ID.Token;
                    string fileName = optionID.InputStream.SourceName;
                    g.tool.errMgr.GrammarError(ErrorType.UNRECOGNIZED_ASSOC_OPTION,
                                               fileName,
                                               optionID,
                                               currentRuleName);
                }
            }

            if (elem is RuleRefAST)
            {
                return CheckRuleRefOptions((RuleRefAST)elem, ID, valueAST);
            }
            if (elem is TerminalAST)
            {
                return CheckTokenOptions((TerminalAST)elem, ID, valueAST);
            }
            if (elem.Type == ANTLRParser.ACTION)
            {
                return false;
            }
            if (elem.Type == ANTLRParser.SEMPRED)
            {
                IToken optionID = ID.Token;
                string fileName = optionID.InputStream.SourceName;
                if (valueAST != null && !Grammar.semPredOptions.Contains(optionID.Text))
                {
                    g.tool.errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                               fileName,
                                               optionID,
                                               optionID.Text);
                    return false;
                }
            }
            return false;
        }

        internal virtual bool CheckRuleRefOptions(RuleRefAST elem, GrammarAST ID, GrammarAST valueAST)
        {
            IToken optionID = ID.Token;
            string fileName = optionID.InputStream.SourceName;
            // don't care about id<SimpleValue> options
            if (valueAST != null && !Grammar.ruleRefOptions.Contains(optionID.Text))
            {
                g.tool.errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                           fileName,
                                           optionID,
                                           optionID.Text);
                return false;
            }
            // TODO: extra checks depending on rule kind?
            return true;
        }

        internal virtual bool CheckTokenOptions(TerminalAST elem, GrammarAST ID, GrammarAST valueAST)
        {
            IToken optionID = ID.Token;
            string fileName = optionID.InputStream.SourceName;
            // don't care about ID<ASTNodeName> options
            if (valueAST != null && !Grammar.tokenOptions.Contains(optionID.Text))
            {
                g.tool.errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                           fileName,
                                           optionID,
                                           optionID.Text);
                return false;
            }
            // TODO: extra checks depending on terminal kind?
            return true;
        }

        internal virtual bool LegalGrammarOption(string key)
        {
            switch (g.Type)
            {
            case ANTLRParser.LEXER:
                return Grammar.lexerOptions.Contains(key);
            case ANTLRParser.PARSER:
                return Grammar.parserOptions.Contains(key);
            default:
                return Grammar.parserOptions.Contains(key);
            }
        }

        internal virtual void CheckImport(IToken importID)
        {
            Grammar @delegate = g.GetImportedGrammar(importID.Text);
            if (@delegate == null)
                return;
            IList<int> validDelegators;
            if (validImportTypes.TryGetValue(@delegate.Type, out validDelegators) && validDelegators != null && !validDelegators.Contains(g.Type))
            {
                g.tool.errMgr.GrammarError(ErrorType.INVALID_IMPORT,
                                           g.fileName,
                                           importID,
                                           g, @delegate);
            }
            if (g.IsCombined() &&
                 (@delegate.name.Equals(g.name + Grammar.GetGrammarTypeToFileNameSuffix(ANTLRParser.LEXER)) ||
                  @delegate.name.Equals(g.name + Grammar.GetGrammarTypeToFileNameSuffix(ANTLRParser.PARSER))))
            {
                g.tool.errMgr.GrammarError(ErrorType.IMPORT_NAME_CLASH,
                                           g.fileName,
                                           importID,
                                           g, @delegate);
            }
        }
    }
}
