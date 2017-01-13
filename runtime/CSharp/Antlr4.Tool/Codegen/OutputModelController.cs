// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using System.Linq;
    using Antlr4.Analysis;
    using Antlr4.Codegen.Model;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.StringTemplate;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using CommonTreeNodeStream = Antlr.Runtime.Tree.CommonTreeNodeStream;
    using Console = System.Console;

    /** This receives events from SourceGenTriggers.g and asks factory to do work.
     *  Then runs extensions in order on resulting SrcOps to get final list.
     **/
    public class OutputModelController
    {
        /** Who does the work? Doesn't have to be CoreOutputModelFactory. */
        public OutputModelFactory @delegate;

        /** Post-processing CodeGeneratorExtension objects; done in order given. */
        public IList<CodeGeneratorExtension> extensions = new List<CodeGeneratorExtension>();

        /** While walking code in rules, this is set to the tree walker that
         *  triggers actions.
         */
        public SourceGenTriggers walker;

        /** Context set by the SourceGenTriggers.g */
        public int codeBlockLevel = -1;
        public int treeLevel = -1;
        public OutputModelObject root; // normally ParserFile, LexerFile, ...
        public Stack<RuleFunction> currentRule = new Stack<RuleFunction>();
        public Alternative currentOuterMostAlt;
        public CodeBlock currentBlock;
        public CodeBlockForOuterMostAlt currentOuterMostAlternativeBlock;

        public OutputModelController(OutputModelFactory factory)
        {
            this.@delegate = factory;
        }

        public virtual void AddExtension(CodeGeneratorExtension ext)
        {
            extensions.Add(ext);
        }

        /** Build a file with a parser containing rule functions. Use the
         *  controller as factory in SourceGenTriggers so it triggers code generation
         *  extensions too, not just the factory functions in this factory.
         */
        public virtual OutputModelObject BuildParserOutputModel()
        {
            Grammar g = @delegate.GetGrammar();
            CodeGenerator gen = @delegate.GetGenerator();
            ParserFile file = ParserFile(gen.GetRecognizerFileName());
            SetRoot(file);
            Parser parser = Parser(file);
            file.parser = parser;

            foreach (Rule r in g.rules.Values)
            {
                BuildRuleFunction(parser, r);
            }

            return file;
        }

        public virtual OutputModelObject BuildLexerOutputModel()
        {
            CodeGenerator gen = @delegate.GetGenerator();
            LexerFile file = LexerFile(gen.GetRecognizerFileName());
            SetRoot(file);
            file.lexer = Lexer(file);

            Grammar g = @delegate.GetGrammar();
            foreach (Rule r in g.rules.Values)
            {
                BuildLexerRuleActions(file.lexer, r);
            }

            return file;
        }

        public virtual OutputModelObject BuildListenerOutputModel()
        {
            CodeGenerator gen = @delegate.GetGenerator();
            return new ListenerFile(@delegate, gen.GetListenerFileName());
        }

        public virtual OutputModelObject BuildBaseListenerOutputModel()
        {
            CodeGenerator gen = @delegate.GetGenerator();
            return new BaseListenerFile(@delegate, gen.GetBaseListenerFileName());
        }

        public virtual OutputModelObject BuildVisitorOutputModel()
        {
            CodeGenerator gen = @delegate.GetGenerator();
            return new VisitorFile(@delegate, gen.GetVisitorFileName());
        }

        public virtual OutputModelObject BuildBaseVisitorOutputModel()
        {
            CodeGenerator gen = @delegate.GetGenerator();
            return new BaseVisitorFile(@delegate, gen.GetBaseVisitorFileName());
        }

        public virtual ParserFile ParserFile(string fileName)
        {
            ParserFile f = @delegate.ParserFile(fileName);
            foreach (CodeGeneratorExtension ext in extensions)
                f = ext.ParserFile(f);
            return f;
        }

        public virtual Parser Parser(ParserFile file)
        {
            Parser p = @delegate.Parser(file);
            foreach (CodeGeneratorExtension ext in extensions)
                p = ext.Parser(p);
            return p;
        }

        public virtual LexerFile LexerFile(string fileName)
        {
            return new LexerFile(@delegate, fileName);
        }

        public virtual Lexer Lexer(LexerFile file)
        {
            return new Lexer(@delegate, file);
        }

        /** Create RuleFunction per rule and update semantic predicates, actions of parser
         *  output object with stuff found in r.
         */
        public virtual void BuildRuleFunction(Parser parser, Rule r)
        {
            RuleFunction function = Rule(r);
            parser.funcs.Add(function);
            PushCurrentRule(function);
            function.FillNamedActions(@delegate, r);

            if (r is LeftRecursiveRule)
            {
                BuildLeftRecursiveRuleFunction((LeftRecursiveRule)r,
                                               (LeftRecursiveRuleFunction)function);
            }
            else
            {
                BuildNormalRuleFunction(r, function);
            }

            Grammar g = GetGrammar();
            foreach (ActionAST a in r.actions)
            {
                if (a is PredAST)
                {
                    PredAST p = (PredAST)a;
                    RuleSempredFunction rsf;
                    if (!parser.sempredFuncs.TryGetValue(r, out rsf) || rsf == null)
                    {
                        rsf = new RuleSempredFunction(@delegate, r, function.ctxType);
                        parser.sempredFuncs[r] = rsf;
                    }
                    rsf.actions[g.sempreds[p]] = new Action(@delegate, p);
                }
            }

            PopCurrentRule();
        }

        public virtual void BuildLeftRecursiveRuleFunction(LeftRecursiveRule r, LeftRecursiveRuleFunction function)
        {
            BuildNormalRuleFunction(r, function);

            // now inject code to start alts
            AbstractTarget target = @delegate.GetTarget();
            TemplateGroup codegenTemplates = target.GetTemplates();

            // pick out alt(s) for primaries
            CodeBlockForOuterMostAlt outerAlt = (CodeBlockForOuterMostAlt)function.code[0];
            IList<CodeBlockForAlt> primaryAltsCode = new List<CodeBlockForAlt>();
            SrcOp primaryStuff = outerAlt.ops[0];
            if (primaryStuff is Choice)
            {
                Choice primaryAltBlock = (Choice)primaryStuff;
                foreach (var alt in primaryAltBlock.alts)
                    primaryAltsCode.Add(alt);
            }
            else
            { // just a single alt I guess; no block
                primaryAltsCode.Add((CodeBlockForAlt)primaryStuff);
            }

            // pick out alt(s) for op alts
            StarBlock opAltStarBlock = (StarBlock)outerAlt.ops[1];
            CodeBlockForAlt altForOpAltBlock = opAltStarBlock.alts[0];
            IList<CodeBlockForAlt> opAltsCode = new List<CodeBlockForAlt>();
            SrcOp opStuff = altForOpAltBlock.ops[0];
            if (opStuff is AltBlock)
            {
                AltBlock opAltBlock = (AltBlock)opStuff;
                foreach (var alt in opAltBlock.alts)
                    opAltsCode.Add(alt);
            }
            else
            { // just a single alt I guess; no block
                opAltsCode.Add((CodeBlockForAlt)opStuff);
            }

            // Insert code in front of each primary alt to create specialized context if there was a label
            for (int i = 0; i < primaryAltsCode.Count; i++)
            {
                LeftRecursiveRuleAltInfo altInfo = r.recPrimaryAlts[i];
                if (altInfo.altLabel == null)
                    continue;
                Template altActionST = codegenTemplates.GetInstanceOf("recRuleReplaceContext");
                altActionST.Add("ctxName", Utils.Capitalize(altInfo.altLabel));
                AltLabelStructDecl ctx = null;
                if (altInfo.altLabel != null)
                    function.altLabelCtxs.TryGetValue(altInfo.altLabel, out ctx);
                Action altAction = new Action(@delegate, ctx, altActionST);
                CodeBlockForAlt alt = primaryAltsCode[i];
                alt.InsertOp(0, altAction);
            }

            // Insert code to set ctx.stop after primary block and before op * loop
            Template setStopTokenAST = codegenTemplates.GetInstanceOf("recRuleSetStopToken");
            Action setStopTokenAction = new Action(@delegate, function.ruleCtx, setStopTokenAST);
            outerAlt.InsertOp(1, setStopTokenAction);

            // Insert code to set _prevctx at start of * loop
            Template setPrevCtx = codegenTemplates.GetInstanceOf("recRuleSetPrevCtx");
            Action setPrevCtxAction = new Action(@delegate, function.ruleCtx, setPrevCtx);
            opAltStarBlock.AddIterationOp(setPrevCtxAction);

            // Insert code in front of each op alt to create specialized context if there was an alt label
            for (int i = 0; i < opAltsCode.Count; i++)
            {
                Template altActionST;
                LeftRecursiveRuleAltInfo altInfo = r.recOpAlts.GetElement(i);
                string templateName;
                if (altInfo.altLabel != null)
                {
                    templateName = "recRuleLabeledAltStartAction";
                    altActionST = codegenTemplates.GetInstanceOf(templateName);
                    altActionST.Add("currentAltLabel", altInfo.altLabel);
                }
                else
                {
                    templateName = "recRuleAltStartAction";
                    altActionST = codegenTemplates.GetInstanceOf(templateName);
                    altActionST.Add("ctxName", Utils.Capitalize(r.name));
                }
                altActionST.Add("ruleName", r.name);
                // add label of any LR ref we deleted
                altActionST.Add("label", altInfo.leftRecursiveRuleRefLabel);
                if (altActionST.impl.FormalArguments.Any(x => x.Name == "isListLabel"))
                {
                    altActionST.Add("isListLabel", altInfo.isListLabel);
                }
                else if (altInfo.isListLabel)
                {
                    @delegate.GetGenerator().tool.errMgr.ToolError(ErrorType.CODE_TEMPLATE_ARG_ISSUE, templateName, "isListLabel");
                }
                AltLabelStructDecl ctx = null;
                if (altInfo.altLabel != null)
                    function.altLabelCtxs.TryGetValue(altInfo.altLabel, out ctx);
                Action altAction = new Action(@delegate, ctx, altActionST);
                CodeBlockForAlt alt = opAltsCode[i];
                alt.InsertOp(0, altAction);
            }
        }

        public virtual void BuildNormalRuleFunction(Rule r, RuleFunction function)
        {
            CodeGenerator gen = @delegate.GetGenerator();
            // TRIGGER factory functions for rule alts, elements
            GrammarASTAdaptor adaptor = new GrammarASTAdaptor(r.ast.Token.InputStream);
            GrammarAST blk = (GrammarAST)r.ast.GetFirstChildWithType(ANTLRParser.BLOCK);
            CommonTreeNodeStream nodes = new CommonTreeNodeStream(adaptor, blk);
            walker = new SourceGenTriggers(nodes, this);
            try
            {
                // walk AST of rule alts/elements
                function.code = DefaultOutputModelFactory.List(walker.block(null, null));
                function.hasLookaheadBlock = walker.hasLookaheadBlock;
            }
            catch (Antlr.Runtime.RecognitionException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }

            function.ctxType = @delegate.GetTarget().GetRuleFunctionContextStructName(function);

            function.postamble = RulePostamble(function, r);
        }

        public virtual void BuildLexerRuleActions(Lexer lexer, Rule r)
        {
            if (r.actions.Count == 0)
            {
                return;
            }

            CodeGenerator gen = @delegate.GetGenerator();
            Grammar g = @delegate.GetGrammar();
            string ctxType = @delegate.GetTarget().GetRuleFunctionContextStructName(r);
            RuleActionFunction raf;
            if (!lexer.actionFuncs.TryGetValue(r, out raf) || raf == null)
            {
                raf = new RuleActionFunction(@delegate, r, ctxType);
            }

            foreach (ActionAST a in r.actions)
            {
                if (a is PredAST)
                {
                    PredAST p = (PredAST)a;
                    RuleSempredFunction rsf ;
                    if (!lexer.sempredFuncs.TryGetValue(r, out rsf) || rsf == null)
                    {
                        rsf = new RuleSempredFunction(@delegate, r, ctxType);
                        lexer.sempredFuncs[r] = rsf;
                    }
                    rsf.actions[g.sempreds[p]] = new Action(@delegate, p);
                }
                else if (a.Type == ANTLRParser.ACTION)
                {
                    raf.actions[g.lexerActions[a]] = new Action(@delegate, a);
                }
            }

            if (raf.actions.Count > 0 && !lexer.actionFuncs.ContainsKey(r))
            {
                // only add to lexer if the function actually contains actions
                lexer.actionFuncs[r] = raf;
            }
        }

        public virtual RuleFunction Rule(Rule r)
        {
            RuleFunction rf = @delegate.Rule(r);
            foreach (CodeGeneratorExtension ext in extensions)
                rf = ext.Rule(rf);
            return rf;
        }

        public virtual IList<SrcOp> RulePostamble(RuleFunction function, Rule r)
        {
            IList<SrcOp> ops = @delegate.RulePostamble(function, r);
            foreach (CodeGeneratorExtension ext in extensions)
                ops = ext.RulePostamble(ops);

            return ops;
        }

        public virtual Grammar GetGrammar()
        {
            return @delegate.GetGrammar();
        }

        public virtual CodeGenerator GetGenerator()
        {
            return @delegate.GetGenerator();
        }

        public virtual CodeBlockForAlt Alternative(Alternative alt, bool outerMost)
        {
            CodeBlockForAlt blk = @delegate.Alternative(alt, outerMost);
            if (outerMost)
            {
                currentOuterMostAlternativeBlock = (CodeBlockForOuterMostAlt)blk;
            }
            foreach (CodeGeneratorExtension ext in extensions)
                blk = ext.Alternative(blk, outerMost);
            return blk;
        }

        public virtual CodeBlockForAlt FinishAlternative(CodeBlockForAlt blk, IList<SrcOp> ops,
                                                 bool outerMost)
        {
            blk = @delegate.FinishAlternative(blk, ops);
            foreach (CodeGeneratorExtension ext in extensions)
                blk = ext.FinishAlternative(blk, outerMost);
            return blk;
        }

        public virtual IList<SrcOp> RuleRef(GrammarAST ID, GrammarAST label, GrammarAST args)
        {
            IList<SrcOp> ops = @delegate.RuleRef(ID, label, args);
            foreach (CodeGeneratorExtension ext in extensions)
            {
                ops = ext.RuleRef(ops);
            }
            return ops;
        }

        public virtual IList<SrcOp> TokenRef(GrammarAST ID, GrammarAST label, GrammarAST args)
        {
            IList<SrcOp> ops = @delegate.TokenRef(ID, label, args);
            foreach (CodeGeneratorExtension ext in extensions)
            {
                ops = ext.TokenRef(ops);
            }
            return ops;
        }

        public virtual IList<SrcOp> StringRef(GrammarAST ID, GrammarAST label)
        {
            IList<SrcOp> ops = @delegate.StringRef(ID, label);
            foreach (CodeGeneratorExtension ext in extensions)
            {
                ops = ext.StringRef(ops);
            }
            return ops;
        }

        /** (A|B|C) possibly with ebnfRoot and label */
        public IList<SrcOp> Set(GrammarAST setAST, GrammarAST labelAST, bool invert)
        {
            IList<SrcOp> ops = @delegate.Set(setAST, labelAST, invert);
            foreach (CodeGeneratorExtension ext in extensions)
            {
                ops = ext.Set(ops);
            }
            return ops;
        }

        public virtual CodeBlockForAlt Epsilon(Alternative alt, bool outerMost)
        {
            CodeBlockForAlt blk = @delegate.Epsilon(alt, outerMost);
            foreach (CodeGeneratorExtension ext in extensions)
                blk = ext.Epsilon(blk);
            return blk;
        }

        public virtual IList<SrcOp> Wildcard(GrammarAST ast, GrammarAST labelAST)
        {
            IList<SrcOp> ops = @delegate.Wildcard(ast, labelAST);
            foreach (CodeGeneratorExtension ext in extensions)
            {
                ops = ext.Set(ops);
            }
            return ops;
        }

        public virtual IList<SrcOp> Action(ActionAST ast)
        {
            IList<SrcOp> ops = @delegate.Action(ast);
            foreach (CodeGeneratorExtension ext in extensions)
                ops = ext.Action(ops);
            return ops;
        }

        public virtual IList<SrcOp> Sempred(ActionAST ast)
        {
            IList<SrcOp> ops = @delegate.Sempred(ast);
            foreach (CodeGeneratorExtension ext in extensions)
                ops = ext.Sempred(ops);
            return ops;
        }

        public virtual Choice GetChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts, GrammarAST label)
        {
            Choice c = @delegate.GetChoiceBlock(blkAST, alts, label);
            foreach (CodeGeneratorExtension ext in extensions)
                c = ext.GetChoiceBlock(c);
            return c;
        }

        public virtual Choice GetEBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts)
        {
            Choice c = @delegate.GetEBNFBlock(ebnfRoot, alts);
            foreach (CodeGeneratorExtension ext in extensions)
                c = ext.GetEBNFBlock(c);
            return c;
        }

        public virtual bool NeedsImplicitLabel(GrammarAST ID, LabeledOp op)
        {
            bool needs = @delegate.NeedsImplicitLabel(ID, op);
            foreach (CodeGeneratorExtension ext in extensions)
                needs |= ext.NeedsImplicitLabel(ID, op);
            return needs;
        }

        public virtual OutputModelObject GetRoot()
        {
            return root;
        }

        public virtual void SetRoot(OutputModelObject root)
        {
            this.root = root;
        }

        public virtual RuleFunction GetCurrentRuleFunction()
        {
            if (currentRule.Count > 0)
                return currentRule.Peek();
            return null;
        }

        public virtual void PushCurrentRule(RuleFunction r)
        {
            currentRule.Push(r);
        }

        public virtual RuleFunction PopCurrentRule()
        {
            if (currentRule.Count > 0)
                return currentRule.Pop();
            return null;
        }

        public virtual Alternative GetCurrentOuterMostAlt()
        {
            return currentOuterMostAlt;
        }

        public virtual void SetCurrentOuterMostAlt(Alternative currentOuterMostAlt)
        {
            this.currentOuterMostAlt = currentOuterMostAlt;
        }

        public virtual void SetCurrentBlock(CodeBlock blk)
        {
            currentBlock = blk;
        }

        public virtual CodeBlock GetCurrentBlock()
        {
            return currentBlock;
        }

        public virtual void SetCurrentOuterMostAlternativeBlock(CodeBlockForOuterMostAlt currentOuterMostAlternativeBlock)
        {
            this.currentOuterMostAlternativeBlock = currentOuterMostAlternativeBlock;
        }

        public virtual CodeBlockForOuterMostAlt GetCurrentOuterMostAlternativeBlock()
        {
            return currentOuterMostAlternativeBlock;
        }

        public virtual int GetCodeBlockLevel()
        {
            return codeBlockLevel;
        }
    }
}
