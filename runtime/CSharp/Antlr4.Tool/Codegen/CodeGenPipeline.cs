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
                Template lexer = gen.GenerateLexer();
                if (g.tool.errMgr.GetNumErrors() == errorCount)
                {
                    WriteRecognizer(lexer, gen);
                }
            }
            else
            {
                Template parser = gen.GenerateParser();
                if (g.tool.errMgr.GetNumErrors() == errorCount)
                {
                    WriteRecognizer(parser, gen);
                }
                if (g.tool.gen_listener)
                {
                    Template listener = gen.GenerateListener();
                    if (g.tool.errMgr.GetNumErrors() == errorCount)
                    {
                        gen.WriteListener(listener);
                    }
                    if (target.WantsBaseListener())
                    {
                        Template baseListener = gen.GenerateBaseListener();
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteBaseListener(baseListener);
                        }
                    }
                }
                if (g.tool.gen_visitor)
                {
                    Template visitor = gen.GenerateVisitor();
                    if (g.tool.errMgr.GetNumErrors() == errorCount)
                    {
                        gen.WriteVisitor(visitor);
                    }
                    if (target.WantsBaseVisitor())
                    {
                        Template baseVisitor = gen.GenerateBaseVisitor();
                        if (g.tool.errMgr.GetNumErrors() == errorCount)
                        {
                            gen.WriteBaseVisitor(baseVisitor);
                        }
                    }
                }
                gen.WriteHeaderFile();
            }
            gen.WriteVocabFile();
        }

        protected virtual void WriteRecognizer(Template template, CodeGenerator gen)
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

            gen.WriteRecognizer(template);
        }
    }
}
