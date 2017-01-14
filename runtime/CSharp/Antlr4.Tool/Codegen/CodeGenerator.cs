// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Antlr4.Codegen.Model;
    using Antlr4.Misc;
    using Antlr4.StringTemplate;
    using Antlr4.Tool;
    using Activator = System.Activator;
    using ArgumentException = System.ArgumentException;
    using InvalidCastException = System.InvalidCastException;
    using IOException = System.IO.IOException;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NotSupportedException = System.NotSupportedException;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using Path = System.IO.Path;
    using TextWriter = System.IO.TextWriter;
    using TokenConstants = Antlr4.Runtime.TokenConstants;
    using Type = System.Type;
    using TypeLoadException = System.TypeLoadException;

    /** General controller for code gen.  Can instantiate sub generator(s).
     */
    public class CodeGenerator
    {
        public static readonly string TEMPLATE_ROOT = Path.Combine("Tool", "Templates", "Codegen");
        public static readonly string VOCAB_FILE_EXTENSION = ".tokens";
        public static readonly string DEFAULT_LANGUAGE = "Java";
        public static readonly string vocabFilePattern =
            "<tokens.keys:{t | <t>=<tokens.(t)>\n}>" +
            "<literals.keys:{t | <t>=<literals.(t)>\n}>";

        [NotNull]
        public readonly Grammar g;
        [NotNull]
        public readonly AntlrTool tool;
        [NotNull]
        public readonly string language;

        private AbstractTarget target;

        public int lineWidth = 72;

        public CodeGenerator([NotNull] Grammar g)
            : this(g.tool, g, g.GetOptionString("language"))
        {
        }

        public CodeGenerator([NotNull] AntlrTool tool, [NotNull] Grammar g, string language)
        {
            this.g = g;
            this.tool = tool;
            this.language = language != null ? language : DEFAULT_LANGUAGE;
        }

        [return: Nullable]
        public virtual AbstractTarget GetTarget()
        {
            if (target == null)
            {
                LoadLanguageTarget(language);
            }

            return target;
        }

        [return: Nullable]
        public virtual TemplateGroup GetTemplates()
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                return null;
            }

            return target.GetTemplates();
        }

        protected virtual void LoadLanguageTarget(string language)
        {
            string targetName = "Antlr4.Codegen.Target." + language + "Target";
            try
            {
                Type c = Type.GetType(targetName, true);
                target = (AbstractTarget)Activator.CreateInstance(c, this);
            }
            catch (TargetInvocationException e)
            {
                tool.errMgr.ToolError(ErrorType.CANNOT_CREATE_TARGET_GENERATOR,
                             e,
                             targetName);
            }
            catch (TypeLoadException e)
            {
                tool.errMgr.ToolError(ErrorType.CANNOT_CREATE_TARGET_GENERATOR,
                             e,
                             targetName);
            }
            catch (ArgumentException e)
            {
                tool.errMgr.ToolError(ErrorType.CANNOT_CREATE_TARGET_GENERATOR,
                             e,
                             targetName);
            }
            catch (InvalidCastException e)
            {
                tool.errMgr.ToolError(ErrorType.CANNOT_CREATE_TARGET_GENERATOR,
                             e,
                             targetName);
            }
        }

        // CREATE TEMPLATES BY WALKING MODEL

        private OutputModelController CreateController()
        {
            OutputModelFactory factory = new ParserFactory(this);
            OutputModelController controller = new OutputModelController(factory);
            factory.SetController(controller);
            return controller;
        }

        private Template Walk(OutputModelObject outputModel, bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            OutputModelWalker walker = new OutputModelWalker(tool, target.GetTemplates());
            return walker.Walk(outputModel, header);
        }

        public virtual Template GenerateLexer()
        {
            return GenerateLexer(false);
        }
        public virtual Template GenerateLexer(bool header)
        {
            return Walk(CreateController().BuildLexerOutputModel(header), header);
        }

        public virtual Template GenerateParser()
        {
            return GenerateParser(false);
        }
        public virtual Template GenerateParser(bool header)
        {
            return Walk(CreateController().BuildParserOutputModel(header), header);
        }

        public virtual Template GenerateListener()
        {
            return GenerateListener(false);
        }
        public virtual Template GenerateListener(bool header)
        {
            return Walk(CreateController().BuildListenerOutputModel(header), header);
        }

        public virtual Template GenerateBaseListener()
        {
            return GenerateBaseListener(false);
        }
        public virtual Template GenerateBaseListener(bool header)
        {
            return Walk(CreateController().BuildBaseListenerOutputModel(header), header);
        }

        public virtual Template GenerateVisitor()
        {
            return GenerateVisitor(false);
        }
        public virtual Template GenerateVisitor(bool header)
        {
            return Walk(CreateController().BuildVisitorOutputModel(header), header);
        }

        public virtual Template GenerateBaseVisitor()
        {
            return GenerateBaseVisitor(false);
        }
        public virtual Template GenerateBaseVisitor(bool header)
        {
            return Walk(CreateController().BuildBaseVisitorOutputModel(header), header);
        }

        /** Generate a token vocab file with all the token names/types.  For example:
         *  ID=7
         *  FOR=8
         *  'for'=8
         *
         *  This is independent of the target language; used by ANTLR internally
         */
        internal virtual Template GetTokenVocabOutput()
        {
            Template vocabFileST = new Template(vocabFilePattern);
            IDictionary<string, int> tokens = new LinkedHashMap<string, int>();
            // make constants for the token names
            foreach (string t in g.tokenNameToTypeMap.Keys)
            {
                int tokenType;
                if (g.tokenNameToTypeMap.TryGetValue(t, out tokenType) && tokenType >= TokenConstants.MinUserTokenType)
                {
                    tokens[t] = tokenType;
                }
            }
            vocabFileST.Add("tokens", tokens);

            // now dump the strings
            IDictionary<string, int> literals = new LinkedHashMap<string, int>();
            foreach (string literal in g.stringLiteralToTypeMap.Keys)
            {
                int tokenType;
                if (g.stringLiteralToTypeMap.TryGetValue(literal, out tokenType) && tokenType >= TokenConstants.MinUserTokenType)
                {
                    literals[literal] = tokenType;
                }
            }

            vocabFileST.Add("literals", literals);

            return vocabFileST;
        }

        public virtual void WriteRecognizer(Template outputFileST, bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            target.GenFile(g, outputFileST, GetRecognizerFileName(header));
        }

        public virtual void WriteListener(Template outputFileST, bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            target.GenFile(g, outputFileST, GetListenerFileName(header));
        }

        public virtual void WriteBaseListener(Template outputFileST, bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            target.GenFile(g, outputFileST, GetBaseListenerFileName(header));
        }

        public virtual void WriteVisitor(Template outputFileST, bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            target.GenFile(g, outputFileST, GetVisitorFileName(header));
        }

        public virtual void WriteBaseVisitor(Template outputFileST, bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            target.GenFile(g, outputFileST, GetBaseVisitorFileName(header));
        }

        public virtual void WriteVocabFile()
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            // write out the vocab interchange file; used by ANTLR,
            // does not change per target
            Template tokenVocabSerialization = GetTokenVocabOutput();
            string fileName = GetVocabFileName();
            if (fileName != null)
            {
                target.GenFile(g, tokenVocabSerialization, fileName);
            }
        }

        public virtual void Write(Template code, string fileName)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                using (TextWriter w = tool.GetOutputFileWriter(g, fileName))
                {
                    ITemplateWriter wr = new AutoIndentWriter(w);
                    wr.LineWidth = lineWidth;
                    code.Write(wr);
                }

                stopwatch.Stop();
            }
            catch (IOException ioe)
            {
                tool.errMgr.ToolError(ErrorType.CANNOT_WRITE_FILE,
                                      ioe,
                                      fileName);
            }
        }

        public virtual string GetRecognizerFileName()
        {
            return GetRecognizerFileName(false);
        }
        public virtual string GetListenerFileName()
        {
            return GetListenerFileName(false);
        }
        public virtual string GetVisitorFileName()
        {
            return GetVisitorFileName(false);
        }
        public virtual string GetBaseListenerFileName()
        {
            return GetBaseListenerFileName(false);
        }
        public virtual string GetBaseVisitorFileName()
        {
            return GetBaseVisitorFileName(false);
        }

        public virtual string GetRecognizerFileName(bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            return target.GetRecognizerFileName(header);
        }

        public virtual string GetListenerFileName(bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            return target.GetListenerFileName(header);
        }

        public virtual string GetVisitorFileName(bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            return target.GetVisitorFileName(header);
        }

        public virtual string GetBaseListenerFileName(bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            return target.GetBaseListenerFileName(header);
        }

        public virtual string GetBaseVisitorFileName(bool header)
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            return target.GetBaseVisitorFileName(header);
        }

        /** What is the name of the vocab file generated for this grammar?
         *  Returns null if no .tokens file should be generated.
         */
        public virtual string GetVocabFileName()
        {
            return g.name + VOCAB_FILE_EXTENSION;
        }

        public virtual string GetHeaderFileName()
        {
            AbstractTarget target = GetTarget();
            if (target == null)
            {
                throw new NotSupportedException("Cannot generate code without a target.");
            }

            Template extST = target.GetTemplates().GetInstanceOf("headerFileExtension");
            if (extST == null)
                return null;
            string recognizerName = g.GetRecognizerName();
            return recognizerName + extST.Render();
        }
    }
}
