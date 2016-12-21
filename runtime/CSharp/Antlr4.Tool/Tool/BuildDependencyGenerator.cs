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
    using System.Text;
    using Antlr4.Codegen;
    using Antlr4.Parse;
    using Antlr4.StringTemplate;
    using Path = System.IO.Path;

    /** Given a grammar file, show the dependencies on .tokens etc...
     *  Using ST, emit a simple "make compatible" list of dependencies.
     *  For example, combined grammar T.g (no token import) generates:
     *
     *  	TParser.java : T.g
     *  	T.tokens : T.g
     *  	TLexer.java : T.g
     *
     *  If we are using the listener pattern (-listener on the command line)
     *  then we add:
     *
     *      TListener.java : T.g
     *      TBaseListener.java : T.g
     *
     *  If we are using the visitor pattern (-visitor on the command line)
     *  then we add:
     *
     *      TVisitor.java : T.g
     *      TBaseVisitor.java : T.g
     *
     *  If "-lib libdir" is used on command-line with -depend and option
     *  tokenVocab=A in grammar, then include the path like this:
     *
     * 		T.g: libdir/A.tokens
     *
     *  Pay attention to -o as well:
     *
     * 		outputdir/TParser.java : T.g
     *
     *  So this output shows what the grammar depends on *and* what it generates.
     *
     *  Operate on one grammar file at a time.  If given a list of .g on the
     *  command-line with -depend, just emit the dependencies.  The grammars
     *  may depend on each other, but the order doesn't matter.  Build tools,
     *  reading in this output, will know how to organize it.
     *
     *  This code was obvious until I removed redundant "./" on front of files
     *  and had to escape spaces in filenames :(
     *
     *  I literally copied from v3 so might be slightly inconsistent with the
     *  v4 code base.
     */
    public class BuildDependencyGenerator
    {
        protected AntlrTool tool;
        protected Grammar g;
        protected CodeGenerator generator;
        protected TemplateGroup templates;

        public BuildDependencyGenerator(AntlrTool tool, Grammar g)
        {
            this.tool = tool;
            this.g = g;
            string language = g.GetOptionString("language");
            generator = new CodeGenerator(tool, g, language);
        }

        /** From T.g return a list of File objects that
         *  name files ANTLR will emit from T.g.
         */
        public virtual IList<string> GetGeneratedFileList()
        {
            AbstractTarget target = generator.GetTarget();
            if (target == null)
            {
                // if the target could not be loaded, no code will be generated.
                return new List<string>();
            }

            IList<string> files = new List<string>();

            // add generated recognizer; e.g., TParser.java
            files.Add(GetOutputFile(generator.GetRecognizerFileName()));
            // add output vocab file; e.g., T.tokens. This is always generated to
            // the base output directory, which will be just . if there is no -o option
            //
            files.Add(GetOutputFile(generator.GetVocabFileName()));
            // are we generating a .h file?
            Template headerExtST = null;
            Template extST = target.GetTemplates().GetInstanceOf("codeFileExtension");
            if (target.GetTemplates().IsDefined("headerFile"))
            {
                headerExtST = target.GetTemplates().GetInstanceOf("headerFileExtension");
                string suffix = Grammar.GetGrammarTypeToFileNameSuffix(g.Type);
                string fileName = g.name + suffix + headerExtST.Render();
                files.Add(GetOutputFile(fileName));
            }
            if (g.IsCombined())
            {
                // add autogenerated lexer; e.g., TLexer.java TLexer.h TLexer.tokens

                string suffix = Grammar.GetGrammarTypeToFileNameSuffix(ANTLRParser.LEXER);
                string lexer = g.name + suffix + extST.Render();
                files.Add(GetOutputFile(lexer));
                string lexerTokens = g.name + suffix + CodeGenerator.VOCAB_FILE_EXTENSION;
                files.Add(GetOutputFile(lexerTokens));

                // TLexer.h
                if (headerExtST != null)
                {
                    string header = g.name + suffix + headerExtST.Render();
                    files.Add(GetOutputFile(header));
                }
            }

            if (g.tool.gen_listener)
            {
                // add generated listener; e.g., TListener.java
                files.Add(GetOutputFile(generator.GetListenerFileName()));
                // add generated base listener; e.g., TBaseListener.java
                files.Add(GetOutputFile(generator.GetBaseListenerFileName()));
            }

            if (g.tool.gen_visitor)
            {
                // add generated visitor; e.g., TVisitor.java
                files.Add(GetOutputFile(generator.GetVisitorFileName()));
                // add generated base visitor; e.g., TBaseVisitor.java
                files.Add(GetOutputFile(generator.GetBaseVisitorFileName()));
            }


            // handle generated files for imported grammars
            IList<Grammar> imports = g.GetAllImportedGrammars();
            if (imports != null)
            {
                foreach (Grammar g in imports)
                {
                    //string outputDir = tool.GetOutputDirectory(g.fileName);
                    //string fname = GroomQualifiedFileName(outputDir, g.GetRecognizerName() + extST.Render());
                    //files.Add(Path.Combine(outputDir, fname));
                    files.Add(GetOutputFile(g.fileName));
                }
            }

            if (files.Count == 0)
            {
                return null;
            }
            return files;
        }

        public virtual string GetOutputFile(string fileName)
        {
            string outputDir = tool.GetOutputDirectory(g.fileName);
            if (outputDir.Equals("."))
            {
                // pay attention to -o then
                outputDir = tool.GetOutputDirectory(fileName);
            }

            if (outputDir.Equals("."))
            {
                return fileName;
            }

            if (Path.GetFileName(outputDir).Equals("."))
            {
                string fname = outputDir;
                int dot = fname.LastIndexOf('.');
                outputDir = outputDir.Substring(0, dot);
            }

            if (Path.GetFileName(outputDir).IndexOf(' ') >= 0)
            {
                // has spaces?
                string escSpaces = outputDir.Replace(" ", "\\ ");
                outputDir = escSpaces;
            }

            return Path.Combine(outputDir, fileName);
        }

        /**
         * Return a list of File objects that name files ANTLR will read
         * to process T.g; This can be .tokens files if the grammar uses the tokenVocab option
         * as well as any imported grammar files.
         */
        public virtual IList<string> GetDependenciesFileList()
        {
            // Find all the things other than imported grammars
            IList<string> files = GetNonImportDependenciesFileList();

            // Handle imported grammars
            IList<Grammar> imports = g.GetAllImportedGrammars();
            if (imports != null)
            {
                foreach (Grammar g in imports)
                {
                    string libdir = tool.libDirectory;
                    string fileName = GroomQualifiedFileName(libdir, g.fileName);
                    files.Add(fileName);
                }
            }

            if (files.Count == 0)
            {
                return null;
            }
            return files;
        }

        /**
         * Return a list of File objects that name files ANTLR will read
         * to process T.g; This can only be .tokens files and only
         * if they use the tokenVocab option.
         *
         * @return List of dependencies other than imported grammars
         */
        public virtual IList<string> GetNonImportDependenciesFileList()
        {
            IList<string> files = new List<string>();

            // handle token vocabulary loads
            string tokenVocab = g.GetOptionString("tokenVocab");
            if (tokenVocab != null)
            {
                string fileName =
                    tokenVocab + CodeGenerator.VOCAB_FILE_EXTENSION;
                string vocabFile;
                if (tool.libDirectory.Equals("."))
                {
                    vocabFile = fileName;
                }
                else
                {
                    vocabFile = Path.Combine(tool.libDirectory, fileName);
                }

                files.Add(vocabFile);
            }

            return files;
        }

        public virtual Template GetDependencies()
        {
            LoadDependencyTemplates();
            Template dependenciesST = templates.GetInstanceOf("dependencies");
            dependenciesST.Add("in", GetDependenciesFileList());
            dependenciesST.Add("out", GetGeneratedFileList());
            dependenciesST.Add("grammarFileName", g.fileName);
            return dependenciesST;
        }

        public virtual void LoadDependencyTemplates()
        {
            if (templates != null)
                return;

            string fileName = Path.Combine("Tool", "Templates", "depend.stg");
            templates = new TemplateGroupFile(Path.GetFullPath(fileName), Encoding.UTF8);
        }

        public virtual CodeGenerator GetGenerator()
        {
            return generator;
        }

        public virtual string GroomQualifiedFileName(string outputDir, string fileName)
        {
            if (outputDir.Equals("."))
            {
                return fileName;
            }
            else if (outputDir.IndexOf(' ') >= 0)
            {
                // has spaces?
                string escSpaces = outputDir.Replace(" ", "\\ ");
                return Path.Combine(escSpaces, fileName);
            }
            else
            {
                return Path.Combine(outputDir, fileName);
            }
        }
    }
}