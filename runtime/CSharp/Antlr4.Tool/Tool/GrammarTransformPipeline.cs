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
    using System.Linq;
    using Antlr4.Analysis;
    using Antlr4.Parse;
    using Antlr4.Tool.Ast;
    using CommonToken = Antlr.Runtime.CommonToken;
    using CommonTree = Antlr.Runtime.Tree.CommonTree;
    using CommonTreeNodeStream = Antlr.Runtime.Tree.CommonTreeNodeStream;
    using ITree = Antlr.Runtime.Tree.ITree;
    using TreeVisitor = Antlr.Runtime.Tree.TreeVisitor;
    using TreeVisitorAction = Antlr.Runtime.Tree.TreeVisitorAction;
    using Tuple = System.Tuple;

    /** Handle left-recursion and block-set transforms */
    public class GrammarTransformPipeline
    {
        public Grammar g;
        public AntlrTool tool;

        public GrammarTransformPipeline(Grammar g, AntlrTool tool)
        {
            this.g = g;
            this.tool = tool;
        }

        public virtual void Process()
        {
            GrammarRootAST root = g.ast;
            if (root == null)
                return;
            tool.Log("grammar", "before: " + root.ToStringTree());

            IntegrateImportedGrammars(g);
            ReduceBlocksToSets(root);
            ExpandParameterizedLoops(root);

            tool.Log("grammar", "after: " + root.ToStringTree());
        }

        public virtual void ReduceBlocksToSets(GrammarAST root)
        {
            CommonTreeNodeStream nodes = new CommonTreeNodeStream(new GrammarASTAdaptor(), root);
            GrammarASTAdaptor adaptor = new GrammarASTAdaptor();
            BlockSetTransformer transformer = new BlockSetTransformer(nodes, g);
            transformer.TreeAdaptor = adaptor;
            transformer.Downup(root);
        }

        /** Find and replace
         *      ID*[','] with ID (',' ID)*
         *      ID+[','] with ID (',' ID)+
         *      (x {action} y)+[','] with x {action} y (',' x {action} y)+
         *
         *  Parameter must be a token.
         *  todo: do we want?
         */
        public virtual void ExpandParameterizedLoops(GrammarAST root)
        {
            TreeVisitor v = new TreeVisitor(new GrammarASTAdaptor());
            Antlr.Runtime.Misc.Func<object, object> preAction =
                t =>
                {
                    if (((GrammarAST)t).Type == 3)
                    {
                        return ExpandParameterizedLoop((GrammarAST)t);
                    }
                    return t;
                };
            Antlr.Runtime.Misc.Func<object, object> postAction = t => t;
            v.Visit(root, new TreeVisitorAction(preAction, postAction));
        }

        public virtual GrammarAST ExpandParameterizedLoop(GrammarAST t)
        {
            // todo: update grammar, alter AST
            return t;
        }

        /** Utility visitor that sets grammar ptr in each node */
        public static void SetGrammarPtr(Grammar g, GrammarAST tree)
        {
            if (tree == null)
                return;
            // ensure each node has pointer to surrounding grammar
            Antlr.Runtime.Misc.Func<object, object> preAction =
                t =>
                {
                    ((GrammarAST)t).g = g;
                    return t;
                };
            Antlr.Runtime.Misc.Func<object, object> postAction = t => t;
            TreeVisitor v = new TreeVisitor(new GrammarASTAdaptor());
            v.Visit(tree, new TreeVisitorAction(preAction, postAction));
        }

        public static void AugmentTokensWithOriginalPosition(Grammar g, GrammarAST tree)
        {
            if (tree == null)
                return;

            IList<GrammarAST> optionsSubTrees = tree.GetNodesWithType(ANTLRParser.ELEMENT_OPTIONS);
            for (int i = 0; i < optionsSubTrees.Count; i++)
            {
                GrammarAST t = optionsSubTrees[i];
                CommonTree elWithOpt = (CommonTree)t.Parent;
                if (elWithOpt is GrammarASTWithOptions)
                {
                    IDictionary<string, GrammarAST> options = ((GrammarASTWithOptions)elWithOpt).GetOptions();
                    if (options.ContainsKey(LeftRecursiveRuleTransformer.TOKENINDEX_OPTION_NAME))
                    {
                        GrammarToken newTok = new GrammarToken(g, elWithOpt.Token);
                        newTok.originalTokenIndex = int.Parse(options[LeftRecursiveRuleTransformer.TOKENINDEX_OPTION_NAME].Text);
                        elWithOpt.Token = newTok;

                        GrammarAST originalNode = g.ast.GetNodeWithTokenIndex(newTok.TokenIndex);
                        if (originalNode != null)
                        {
                            // update the AST node start/stop index to match the values
                            // of the corresponding node in the original parse tree.
                            elWithOpt.TokenStartIndex = originalNode.TokenStartIndex;
                            elWithOpt.TokenStopIndex = originalNode.TokenStopIndex;
                        }
                        else
                        {
                            // the original AST node could not be located by index;
                            // make sure to assign valid values for the start/stop
                            // index so toTokenString will not throw exceptions.
                            elWithOpt.TokenStartIndex = newTok.TokenIndex;
                            elWithOpt.TokenStopIndex = newTok.TokenIndex;
                        }
                    }
                }
            }
        }

        /** Merge all the rules, token definitions, and named actions from
            imported grammars into the root grammar tree.  Perform:

            (tokens { X (= Y 'y')) + (tokens { Z )	-&gt;	(tokens { X (= Y 'y') Z)

            (@ members {foo}) + (@ members {bar})	-&gt;	(@ members {foobar})

            (RULES (RULE x y)) + (RULES (RULE z))	-&gt;	(RULES (RULE x y z))

            Rules in root prevent same rule from being appended to RULES node.

            The goal is a complete combined grammar so we can ignore subordinate
            grammars.
         */
        public virtual void IntegrateImportedGrammars(Grammar rootGrammar)
        {
            IList<Grammar> imports = rootGrammar.GetAllImportedGrammars();
            if (imports == null)
                return;

            GrammarAST root = rootGrammar.ast;
            GrammarAST id = (GrammarAST)root.GetChild(0);
            GrammarASTAdaptor adaptor = new GrammarASTAdaptor(id.Token.InputStream);

            GrammarAST tokensRoot = (GrammarAST)root.GetFirstChildWithType(ANTLRParser.TOKENS_SPEC);

            IList<GrammarAST> actionRoots = root.GetNodesWithType(ANTLRParser.AT);

            // Compute list of rules in root grammar and ensure we have a RULES node
            GrammarAST RULES = (GrammarAST)root.GetFirstChildWithType(ANTLRParser.RULES);
            ISet<string> rootRuleNames = new HashSet<string>();
            // make list of rules we have in root grammar
            IList<GrammarAST> rootRules = RULES.GetNodesWithType(ANTLRParser.RULE);
            foreach (GrammarAST r in rootRules)
                rootRuleNames.Add(r.GetChild(0).Text);

            foreach (Grammar imp in imports)
            {
                // COPY TOKENS
                GrammarAST imp_tokensRoot = (GrammarAST)imp.ast.GetFirstChildWithType(ANTLRParser.TOKENS_SPEC);
                if (imp_tokensRoot != null)
                {
                    rootGrammar.tool.Log("grammar", "imported tokens: " + imp_tokensRoot.Children);
                    if (tokensRoot == null)
                    {
                        tokensRoot = (GrammarAST)adaptor.Create(ANTLRParser.TOKENS_SPEC, "TOKENS");
                        tokensRoot.g = rootGrammar;
                        root.InsertChild(1, tokensRoot); // ^(GRAMMAR ID TOKENS...)
                    }
                    tokensRoot.AddChildren(imp_tokensRoot.Children);
                }

                IList<GrammarAST> all_actionRoots = new List<GrammarAST>();
                IList<GrammarAST> imp_actionRoots = imp.ast.GetAllChildrenWithType(ANTLRParser.AT);
                if (actionRoots != null)
                {
                    foreach (var actionRoot in actionRoots)
                        all_actionRoots.Add(actionRoot);
                }

                foreach (var actionRoot in imp_actionRoots)
                    all_actionRoots.Add(actionRoot);

                // COPY ACTIONS
                if (imp_actionRoots != null)
                {
                    IDictionary<System.Tuple<string, string>, GrammarAST> namedActions =
                        new Dictionary<System.Tuple<string, string>, GrammarAST>();

                    rootGrammar.tool.Log("grammar", "imported actions: " + imp_actionRoots);
                    foreach (GrammarAST at in all_actionRoots)
                    {
                        string scopeName = rootGrammar.GetDefaultActionScope();
                        GrammarAST scope, name, action;
                        if (at.ChildCount > 2)
                        { // must have a scope
                            scope = (GrammarAST)at.GetChild(0);
                            scopeName = scope.Text;
                            name = (GrammarAST)at.GetChild(1);
                            action = (GrammarAST)at.GetChild(2);
                        }
                        else
                        {
                            name = (GrammarAST)at.GetChild(0);
                            action = (GrammarAST)at.GetChild(1);
                        }
                        GrammarAST prevAction;
                        if (!namedActions.TryGetValue(Tuple.Create(scopeName, name.Text), out prevAction) || prevAction == null)
                        {
                            namedActions[Tuple.Create(scopeName, name.Text)] = action;
                        }
                        else
                        {
                            if (prevAction.g == at.g)
                            {
                                rootGrammar.tool.errMgr.GrammarError(ErrorType.ACTION_REDEFINITION,
                                                    at.g.fileName, name.Token, name.Text);
                            }
                            else
                            {
                                string s1 = prevAction.Text;
                                s1 = s1.Substring(1, s1.Length - 2);
                                string s2 = action.Text;
                                s2 = s2.Substring(1, s2.Length - 2);
                                string combinedAction = "{" + s1 + '\n' + s2 + "}";
                                prevAction.Token.Text = combinedAction;
                            }
                        }
                    }
                    // at this point, we have complete list of combined actions,
                    // some of which are already living in root grammar.
                    // Merge in any actions not in root grammar into root's tree.
                    foreach (string scopeName in namedActions.Keys.Select(i => i.Item1).Distinct())
                    {
                        foreach (string name in namedActions.Keys.Where(i => i.Item1 == scopeName).Select(i => i.Item2))
                        {
                            GrammarAST action = namedActions[Tuple.Create(scopeName, name)];
                            rootGrammar.tool.Log("grammar", action.g.name + " " + scopeName + ":" + name + "=" + action.Text);
                            if (action.g != rootGrammar)
                            {
                                root.InsertChild(1, action.Parent);
                            }
                        }
                    }
                }

                // COPY RULES
                IList<GrammarAST> rules = imp.ast.GetNodesWithType(ANTLRParser.RULE);
                if (rules != null)
                {
                    foreach (GrammarAST r in rules)
                    {
                        rootGrammar.tool.Log("grammar", "imported rule: " + r.ToStringTree());
                        string name = r.GetChild(0).Text;
                        bool rootAlreadyHasRule = rootRuleNames.Contains(name);
                        if (!rootAlreadyHasRule)
                        {
                            RULES.AddChild(r); // merge in if not overridden
                            rootRuleNames.Add(name);
                        }
                    }
                }

                GrammarAST optionsRoot = (GrammarAST)imp.ast.GetFirstChildWithType(ANTLRParser.OPTIONS);
                if (optionsRoot != null)
                {
                    // suppress the warning if the options match the options specified
                    // in the root grammar
                    // https://github.com/antlr/antlr4/issues/707

                    bool hasNewOption = false;
                    foreach (KeyValuePair<string, GrammarAST> option in imp.ast.GetOptions())
                    {
                        string importOption = imp.ast.GetOptionString(option.Key);
                        if (importOption == null)
                        {
                            continue;
                        }

                        string rootOption = rootGrammar.ast.GetOptionString(option.Key);
                        if (!importOption.Equals(rootOption))
                        {
                            hasNewOption = true;
                            break;
                        }
                    }

                    if (hasNewOption)
                    {
                        rootGrammar.tool.errMgr.GrammarError(ErrorType.OPTIONS_IN_DELEGATE,
                                            optionsRoot.g.fileName, optionsRoot.Token, imp.name);
                    }
                }
            }
            rootGrammar.tool.Log("grammar", "Grammar: " + rootGrammar.ast.ToStringTree());
        }

        /** Build lexer grammar from combined grammar that looks like:
         *
         *  (COMBINED_GRAMMAR A
         *      (tokens { X (= Y 'y'))
         *      (OPTIONS (= x 'y'))
         *      (@ members {foo})
         *      (@ lexer header {package jj;})
         *      (RULES (RULE .+)))
         *
         *  Move rules and actions to new tree, don't dup. Split AST apart.
         *  We'll have this Grammar share token symbols later; don't generate
         *  tokenVocab or tokens{} section.  Copy over named actions.
         *
         *  Side-effects: it removes children from GRAMMAR &amp; RULES nodes
         *                in combined AST.  Anything cut out is dup'd before
         *                adding to lexer to avoid "who's ur daddy" issues
         */
        public virtual GrammarRootAST ExtractImplicitLexer(Grammar combinedGrammar)
        {
            GrammarRootAST combinedAST = combinedGrammar.ast;
            //tool.log("grammar", "before="+combinedAST.toStringTree());
            GrammarASTAdaptor adaptor = new GrammarASTAdaptor(combinedAST.Token.InputStream);
            GrammarAST[] elements = combinedAST.GetChildrenAsArray();

            // MAKE A GRAMMAR ROOT and ID
            string lexerName = combinedAST.GetChild(0).Text + "Lexer";
            GrammarRootAST lexerAST =
                new GrammarRootAST(new CommonToken(ANTLRParser.GRAMMAR, "LEXER_GRAMMAR"), combinedGrammar.ast.tokenStream);
            lexerAST.grammarType = ANTLRParser.LEXER;
            lexerAST.Token.InputStream = combinedAST.Token.InputStream;
            lexerAST.AddChild((ITree)adaptor.Create(ANTLRParser.ID, lexerName));

            // COPY OPTIONS
            GrammarAST optionsRoot =
                (GrammarAST)combinedAST.GetFirstChildWithType(ANTLRParser.OPTIONS);
            if (optionsRoot != null && optionsRoot.ChildCount != 0)
            {
                GrammarAST lexerOptionsRoot = (GrammarAST)adaptor.DupNode(optionsRoot);
                lexerAST.AddChild(lexerOptionsRoot);
                GrammarAST[] options = optionsRoot.GetChildrenAsArray();
                foreach (GrammarAST o in options)
                {
                    string optionName = o.GetChild(0).Text;
                    if (Grammar.lexerOptions.Contains(optionName) &&
                         !Grammar.doNotCopyOptionsToLexer.Contains(optionName))
                    {
                        GrammarAST optionTree = (GrammarAST)adaptor.DupTree(o);
                        lexerOptionsRoot.AddChild(optionTree);
                        lexerAST.SetOption(optionName, (GrammarAST)optionTree.GetChild(1));
                    }
                }
            }

            // COPY all named actions, but only move those with lexer:: scope
            IList<GrammarAST> actionsWeMoved = new List<GrammarAST>();
            foreach (GrammarAST e in elements)
            {
                if (e.Type == ANTLRParser.AT)
                {
                    lexerAST.AddChild((ITree)adaptor.DupTree(e));
                    if (e.GetChild(0).Text.Equals("lexer"))
                    {
                        actionsWeMoved.Add(e);
                    }
                }
            }

            foreach (GrammarAST r in actionsWeMoved)
            {
                combinedAST.DeleteChild(r);
            }

            GrammarAST combinedRulesRoot =
                (GrammarAST)combinedAST.GetFirstChildWithType(ANTLRParser.RULES);
            if (combinedRulesRoot == null)
                return lexerAST;

            // MOVE lexer rules

            GrammarAST lexerRulesRoot = (GrammarAST)adaptor.Create(ANTLRParser.RULES, "RULES");
            lexerAST.AddChild(lexerRulesRoot);
            IList<GrammarAST> rulesWeMoved = new List<GrammarAST>();
            GrammarASTWithOptions[] rules;
            if (combinedRulesRoot.ChildCount > 0)
            {
                rules = combinedRulesRoot.Children.Cast<GrammarASTWithOptions>().ToArray();
            }
            else
            {
                rules = new GrammarASTWithOptions[0];
            }

            foreach (GrammarASTWithOptions r in rules)
            {
                string ruleName = r.GetChild(0).Text;
                if (Grammar.IsTokenName(ruleName))
                {
                    lexerRulesRoot.AddChild((ITree)adaptor.DupTree(r));
                    rulesWeMoved.Add(r);
                }
            }

            foreach (GrammarAST r in rulesWeMoved)
            {
                combinedRulesRoot.DeleteChild(r);
            }

            // Will track 'if' from IF : 'if' ; rules to avoid defining new token for 'if'
            IList<System.Tuple<GrammarAST, GrammarAST>> litAliases =
                Grammar.GetStringLiteralAliasesFromLexerRules(lexerAST);

            ISet<string> stringLiterals = combinedGrammar.GetStringLiterals();
            // add strings from combined grammar (and imported grammars) into lexer
            // put them first as they are keywords; must resolve ambigs to these rules
            //		tool.log("grammar", "strings from parser: "+stringLiterals);
            int insertIndex = 0;
            foreach (string lit in stringLiterals)
            {
                // if lexer already has a rule for literal, continue
                if (litAliases != null)
                {
                    foreach (System.Tuple<GrammarAST, GrammarAST> pair in litAliases)
                    {
                        GrammarAST litAST = pair.Item2;
                        if (lit.Equals(litAST.Text))
                            goto continueNextLit;
                    }
                }
                // create for each literal: (RULE <uniquename> (BLOCK (ALT <lit>))
                string rname = combinedGrammar.GetStringLiteralLexerRuleName(lit);
                // can't use wizard; need special node types
                GrammarAST litRule = new RuleAST(ANTLRParser.RULE);
                BlockAST blk = new BlockAST(ANTLRParser.BLOCK);
                AltAST alt = new AltAST(ANTLRParser.ALT);
                TerminalAST slit = new TerminalAST(new CommonToken(ANTLRParser.STRING_LITERAL, lit));
                alt.AddChild(slit);
                blk.AddChild(alt);
                CommonToken idToken = new CommonToken(ANTLRParser.TOKEN_REF, rname);
                litRule.AddChild(new TerminalAST(idToken));
                litRule.AddChild(blk);
                lexerRulesRoot.InsertChild(insertIndex, litRule);
                //			lexerRulesRoot.getChildren().add(0, litRule);
                lexerRulesRoot.FreshenParentAndChildIndexes(); // reset indexes and set litRule parent

                // next literal will be added after the one just added
                insertIndex++;

                continueNextLit:
                ;
            }

            // TODO: take out after stable if slow
            lexerAST.SanityCheckParentAndChildIndexes();
            combinedAST.SanityCheckParentAndChildIndexes();
            //		tool.log("grammar", combinedAST.toTokenString());

            combinedGrammar.tool.Log("grammar", "after extract implicit lexer =" + combinedAST.ToStringTree());
            combinedGrammar.tool.Log("grammar", "lexer =" + lexerAST.ToStringTree());

            if (lexerRulesRoot.ChildCount == 0)
                return null;
            return lexerAST;
        }
    }
}
