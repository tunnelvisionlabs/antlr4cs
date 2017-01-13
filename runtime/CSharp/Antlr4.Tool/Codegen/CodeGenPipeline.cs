// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using Antlr4.Parse;
    using Antlr4.StringTemplate;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    public class CodeGenPipeline
    {
        internal Grammar g;

        public CodeGenPipeline(Grammar g)
        {
            this.g = g;
        }

        public virtual void Process()
        {
            CodeGenerator gen = new CodeGenerator(g);
            AbstractTarget target = gen.GetTarget();
            if (target == null)
            {
                return;
            }

            IntervalSet idTypes = new IntervalSet();
            idTypes.Add(ANTLRParser.ID);
            idTypes.Add(ANTLRParser.RULE_REF);
            idTypes.Add(ANTLRParser.TOKEN_REF);
            IList<GrammarAST> idNodes = g.ast.GetNodesWithType(idTypes);
            foreach (GrammarAST idNode in idNodes)
            {
                if (target.GrammarSymbolCausesIssueInGeneratedCode(idNode))
                {
                    g.tool.errMgr.GrammarError(ErrorType.USE_OF_BAD_WORD,
                                               g.fileName, idNode.Token,
                                               idNode.Text);
                }
            }

            // all templates are generated in memory to report the most complete
            // error information possible, but actually writing output files stops
            // after the first error is reported
            int errorCount = g.tool.errMgr.GetNumErrors();

            if (g.IsLexer())
            {
                if (target.NeedsHeader())
                {
                    Template lexerHeader = gen.GenerateLexer(true); // Header file if needed.
                    if (g.tool.errMgr.GetNumErrors() == errorCount)
                    {
                        WriteRecognizer(lexerHeader, gen, true);
                    }
                }
                Template lexer = gen.GenerateLexer(false);
                if (g.tool.errMgr.GetNumErrors() == errorCount)
                {
                    WriteRecognizer(lexer, gen, false);
                }
            }
            else
            {
                if (target.NeedsHeader())
                {
                    Template parserHeader = gen.GenerateParser(true);
                    if (g.tool.errMgr.GetNumErrors() == errorCount)
                    {
                        WriteRecognizer(parserHeader, gen, true);
                    }
                }
                Template parser = gen.GenerateParser(false);
                if (g.tool.errMgr.GetNumErrors() == errorCount)
                {
                    WriteRecognizer(parser, gen, false);
                }

                if (g.tool.gen_listener)
                {
                    if (target.NeedsHeader())
                    {
                        Template listenerHeader = gen.GenerateListener(true);
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteListener(listenerHeader, true);
                        }
                    }
                    Template listener = gen.GenerateListener(false);
                    if (g.tool.errMgr.GetNumErrors() == errorCount)
                    {
                        gen.WriteListener(listener, false);
                    }

                    if (target.NeedsHeader())
                    {
                        Template baseListener = gen.GenerateBaseListener(true);
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteBaseListener(baseListener, true);
                        }
                    }
                    if (target.WantsBaseListener())
                    {
                        Template baseListener = gen.GenerateBaseListener(false);
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteBaseListener(baseListener, false);
                        }
                    }
                }
                if (g.tool.gen_visitor)
                {
                    if (target.NeedsHeader())
                    {
                        Template visitorHeader = gen.GenerateVisitor(true);
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteVisitor(visitorHeader, true);
                        }
                    }
                    Template visitor = gen.GenerateVisitor(false);
                    if (g.tool.errMgr.GetNumErrors() == errorCount)
                    {
                        gen.WriteVisitor(visitor, false);
                    }

                    if (target.NeedsHeader())
                    {
                        Template baseVisitor = gen.GenerateBaseVisitor(true);
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteBaseVisitor(baseVisitor, true);
                        }
                    }
                    if (target.WantsBaseVisitor())
                    {
                        Template baseVisitor = gen.GenerateBaseVisitor(false);
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteBaseVisitor(baseVisitor, false);
                        }
                    }
                }
            }

            gen.WriteVocabFile();
        }

        protected virtual void WriteRecognizer(Template template, CodeGenerator gen, bool header)
        {
#if false
            if (g.tool.launch_ST_inspector)
            {
                STViz viz = template.inspect();
                if (g.tool.ST_inspector_wait_for_close)
                {
                    try
                    {
                        viz.waitForClose();
                    }
                    catch (InterruptedException ex)
                    {
                        g.tool.errMgr.toolError(ErrorType.INTERNAL_ERROR, ex);
                    }
                }
            }
#endif

            gen.WriteRecognizer(template, header);
        }
    }
}
