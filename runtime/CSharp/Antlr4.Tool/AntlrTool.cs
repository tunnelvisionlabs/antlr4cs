// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using Antlr4.Analysis;
    using Antlr4.Automata;
    using Antlr4.Codegen;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Semantics;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using ANTLRStringStream = Antlr.Runtime.ANTLRStringStream;
    using ArgumentNullException = System.ArgumentNullException;
    using CommonTokenStream = Antlr.Runtime.CommonTokenStream;
    using Console = System.Console;
    using StreamWriter = System.IO.StreamWriter;
    using Directory = System.IO.Directory;
    using Exception = System.Exception;
    using File = System.IO.File;
    using FileMode = System.IO.FileMode;
    using ICharStream = Antlr.Runtime.ICharStream;
    using IOException = System.IO.IOException;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using Path = System.IO.Path;
    using RecognitionException = Antlr.Runtime.RecognitionException;
    using StringWriter = System.IO.StringWriter;
    using TextWriter = System.IO.TextWriter;
    using Type = System.Type;
    using NotSupportedException = System.NotSupportedException;

    public class AntlrTool
    {
        public static readonly string VERSION;
        static AntlrTool()
        {
            var assembly = typeof(AntlrTool).GetTypeInfo().Assembly;
            var informationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            VERSION = informationalVersionAttribute?.InformationalVersion ?? assembly.GetName().Version.ToString(4);
        }

        public static readonly string GRAMMAR_EXTENSION = ".g4";
        public static readonly string LEGACY_GRAMMAR_EXTENSION = ".g";

        public static readonly IList<string> ALL_GRAMMAR_EXTENSIONS =
            new List<string> { GRAMMAR_EXTENSION, LEGACY_GRAMMAR_EXTENSION }.AsReadOnly();

        public enum OptionArgType
        {
            NONE, STRING
        } // NONE implies boolean
        public class Option
        {
            internal string fieldName;
            internal string name;
            internal OptionArgType argType;
            internal string description;

            public Option(string fieldName, string name, string description)
                : this(fieldName, name, OptionArgType.NONE, description)
            {
            }

            public Option(string fieldName, string name, OptionArgType argType, string description)
            {
                this.fieldName = fieldName;
                this.name = name;
                this.argType = argType;
                this.description = description;
            }
        }

        // fields set by option manager

        public string inputDirectory; // used by mvn plugin but not set by tool itself.
        public string outputDirectory;
        public string libDirectory;
        public bool generate_ATN_dot = false;
        public string grammarEncoding = "UTF-8";
        public string msgFormat = "antlr";
        public bool launch_ST_inspector = false;
        public bool ST_inspector_wait_for_close = false;
        public bool force_atn = false;
        public bool log = false;
        public bool gen_listener = true;
        public bool gen_visitor = false;
        public bool gen_dependencies = false;
        public string genPackage = null;
        public IDictionary<string, string> grammarOptions = null;
        public bool warnings_are_errors = false;
        public bool longMessages = false;

        public static Option[] optionDefs = {
        new Option(nameof(outputDirectory),   "-o", OptionArgType.STRING, "specify output directory where all output is generated"),
        new Option(nameof(libDirectory),      "-lib", OptionArgType.STRING, "specify location of grammars, tokens files"),
        new Option(nameof(generate_ATN_dot),  "-atn", "generate rule augmented transition network diagrams"),
        new Option(nameof(grammarEncoding),   "-encoding", OptionArgType.STRING, "specify grammar file encoding; e.g., euc-jp"),
        new Option(nameof(msgFormat),         "-message-format", OptionArgType.STRING, "specify output style for messages in antlr, gnu, vs2005"),
        new Option(nameof(longMessages),      "-long-messages", "show exception details when available for errors and warnings"),
        new Option(nameof(gen_listener),      "-listener", "generate parse tree listener (default)"),
        new Option(nameof(gen_listener),      "-no-listener", "don't generate parse tree listener"),
        new Option(nameof(gen_visitor),       "-visitor", "generate parse tree visitor"),
        new Option(nameof(gen_visitor),       "-no-visitor", "don't generate parse tree visitor (default)"),
        new Option(nameof(genPackage),        "-package", OptionArgType.STRING, "specify a package/namespace for the generated code"),
        new Option(nameof(gen_dependencies),  "-depend", "generate file dependencies"),
        new Option("",                  "-D<option>=value", "set/override a grammar-level option"),
        new Option(nameof(warnings_are_errors), "-Werror", "treat warnings as errors"),
        new Option(nameof(launch_ST_inspector), "-XdbgST", "launch StringTemplate visualizer on generated code"),
        new Option(nameof(ST_inspector_wait_for_close), "-XdbgSTWait", "wait for STViz to close before continuing"),
        new Option(nameof(force_atn),         "-Xforce-atn", "use the ATN simulator for all predictions"),
        new Option(nameof(log),               "-Xlog", "dump lots of logging info to antlr-timestamp.log"),
    };

        // helper vars for option management
        protected bool haveOutputDir = false;
        protected bool return_dont_exit = false;

        // The internal options are for my use on the command line during dev
        public static bool internalOption_PrintGrammarTree = false;
        public static bool internalOption_ShowATNConfigsInDFA = false;


        public readonly string[] args;

        protected IList<string> grammarFiles = new List<string>();

        public ErrorManager errMgr;
        public LogManager logMgr = new LogManager();

        private TextWriter _consoleOut = Console.Out;
        private TextWriter _consoleError = Console.Error;

        IList<ANTLRToolListener> listeners = new List<ANTLRToolListener>();

        /** Track separately so if someone adds a listener, it's the only one
         *  instead of it and the default stderr listener.
         */
        DefaultToolListener defaultListener;

        public static int Main(string[] args)
        {
            AntlrTool antlr = new AntlrTool(args);
            if (args.Length == 0)
            {
                antlr.Help();
                return 0;
            }

            try
            {
                antlr.ProcessGrammarsOnCommandLine();
            }
            finally
            {
                if (antlr.log)
                {
                    try
                    {
                        string logname = antlr.logMgr.Save();
                        antlr.ConsoleOut.WriteLine("wrote " + logname);
                    }
                    catch (IOException ioe)
                    {
                        antlr.errMgr.ToolError(ErrorType.INTERNAL_ERROR, ioe);
                    }
                }
            }

            if (antlr.errMgr.GetNumErrors() > 0)
            {
                return 1;
            }

            return 0;
        }

        public AntlrTool()
            : this(null)
        {
        }

        public AntlrTool(string[] args)
        {
            this.args = args;
            defaultListener = new DefaultToolListener(this);
            errMgr = new ErrorManager(this);
            errMgr.SetFormat(msgFormat);
            HandleArgs();
        }

        public TextWriter ConsoleOut
        {
            get
            {
                return _consoleOut;
            }

            set
            {
                _consoleOut = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public TextWriter ConsoleError
        {
            get
            {
                return _consoleError;
            }

            set
            {
                _consoleError = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        protected virtual void HandleArgs()
        {
            int i = 0;
            while (args != null && i < args.Length)
            {
                string arg = args[i];
                i++;
                if (arg.StartsWith("-D"))
                {
                    // -Dlanguage=Java syntax
                    HandleOptionSetArg(arg);
                    continue;
                }

                if (arg[0] != '-')
                {
                    // file name
                    if (!grammarFiles.Contains(arg))
                        grammarFiles.Add(arg);
                    continue;
                }

                bool found = false;
                foreach (Option o in optionDefs)
                {
                    if (arg.Equals(o.name))
                    {
                        found = true;
                        string argValue = null;
                        if (o.argType == OptionArgType.STRING)
                        {
                            argValue = args[i];
                            i++;
                        }

                        // use reflection to set field
                        try
                        {
                            FieldInfo f = GetField(GetType(), o.fieldName);
                            if (argValue == null)
                            {
                                if (arg.StartsWith("-no-"))
                                    f.SetValue(this, false);
                                else
                                    f.SetValue(this, true);
                            }
                            else
                                f.SetValue(this, argValue);
                        }
                        catch (Exception)
                        {
                            errMgr.ToolError(ErrorType.INTERNAL_ERROR, "can't access field " + o.fieldName);
                        }
                    }
                }
                if (!found)
                {
                    errMgr.ToolError(ErrorType.INVALID_CMDLINE_ARG, arg);
                }
            }
            if (outputDirectory != null)
            {
                if (outputDirectory.EndsWith("/") ||
                    outputDirectory.EndsWith("\\"))
                {
                    outputDirectory =
                        outputDirectory.Substring(0, outputDirectory.Length - 1);
                }

                string outDir = outputDirectory;
                haveOutputDir = true;
                if (File.Exists(outDir))
                {
                    errMgr.ToolError(ErrorType.OUTPUT_DIR_IS_FILE, outputDirectory);
                    libDirectory = ".";
                }
            }
            else
            {
                outputDirectory = ".";
            }
            if (libDirectory != null)
            {
                if (libDirectory.EndsWith("/") ||
                    libDirectory.EndsWith("\\"))
                {
                    libDirectory = libDirectory.Substring(0, libDirectory.Length - 1);
                }

                string outDir = libDirectory;
                if (!Directory.Exists(outDir))
                {
                    errMgr.ToolError(ErrorType.DIR_NOT_FOUND, libDirectory);
                    libDirectory = ".";
                }
            }
            else
            {
                libDirectory = ".";
            }

            if (launch_ST_inspector)
            {
                //TemplateGroup.trackCreationEvents = true;
                return_dont_exit = true;
            }
        }

        private static FieldInfo GetField(Type type, string name)
        {
            var field = type.GetTypeInfo().GetDeclaredField(name);
            if (field != null)
                return field;

            var baseType = type.GetTypeInfo().BaseType;
            if (baseType != null)
                return GetField(baseType, name);

            return null;
        }

        protected virtual void HandleOptionSetArg(string arg)
        {
            int eq = arg.IndexOf('=');
            if (eq > 0 && arg.Length > 3)
            {
                string option = arg.Substring("-D".Length, eq - "-D".Length);
                string value = arg.Substring(eq + 1);
                if (value.Length == 0)
                {
                    errMgr.ToolError(ErrorType.BAD_OPTION_SET_SYNTAX, arg);
                    return;
                }
                if (Grammar.parserOptions.Contains(option) ||
                     Grammar.lexerOptions.Contains(option))
                {
                    if (grammarOptions == null)
                        grammarOptions = new Dictionary<string, string>();
                    grammarOptions[option] = value;
                }
                else
                {
                    errMgr.GrammarError(ErrorType.ILLEGAL_OPTION,
                                        null,
                                        null,
                                        option);
                }
            }
            else
            {
                errMgr.ToolError(ErrorType.BAD_OPTION_SET_SYNTAX, arg);
            }
        }

        public virtual void ProcessGrammarsOnCommandLine()
        {
            IList<GrammarRootAST> sortedGrammars = SortGrammarByTokenVocab(grammarFiles);

            foreach (GrammarRootAST t in sortedGrammars)
            {
                Grammar g = CreateGrammar(t);
                g.fileName = t.fileName;
                if (gen_dependencies)
                {
                    BuildDependencyGenerator dep =
                        new BuildDependencyGenerator(this, g);
                    //IList<string> outputFiles = dep.GetGeneratedFileList();
                    //IList<string> dependents = dep.GetDependenciesFileList();
                    //g.tool.ConsoleOut.WriteLine("output: " + outputFiles);
                    //g.tool.ConsoleOut.WriteLine("dependents: " + dependents);
                    g.tool.ConsoleOut.WriteLine(dep.GetDependencies().Render());
                }
                else if (errMgr.GetNumErrors() == 0)
                {
                    Process(g, true);
                }
            }
        }

        /** To process a grammar, we load all of its imported grammars into
            subordinate grammar objects. Then we merge the imported rules
            into the root grammar. If a root grammar is a combined grammar,
            we have to extract the implicit lexer. Once all this is done, we
            process the lexer first, if present, and then the parser grammar
         */
        public virtual void Process(Grammar g, bool gencode)
        {
            g.LoadImportedGrammars();

            GrammarTransformPipeline transform = new GrammarTransformPipeline(g, this);
            transform.Process();

            LexerGrammar lexerg;
            GrammarRootAST lexerAST;
            if (g.ast != null && g.ast.grammarType == ANTLRParser.COMBINED &&
                 !g.ast.hasErrors)
            {
                lexerAST = transform.ExtractImplicitLexer(g); // alters g.ast
                if (lexerAST != null)
                {
                    if (grammarOptions != null)
                    {
                        lexerAST.cmdLineOptions = grammarOptions;
                    }

                    lexerg = new LexerGrammar(this, lexerAST);
                    lexerg.fileName = g.fileName;
                    lexerg.originalGrammar = g;
                    g.implicitLexer = lexerg;
                    lexerg.implicitLexerOwner = g;

                    int prevErrors = errMgr.GetNumErrors();
                    ProcessNonCombinedGrammar(lexerg, gencode);
                    if (errMgr.GetNumErrors() > prevErrors)
                    {
                        return;
                    }

                    //				System.out.println("lexer tokens="+lexerg.tokenNameToTypeMap);
                    //				System.out.println("lexer strings="+lexerg.stringLiteralToTypeMap);
                }
            }
            if (g.implicitLexer != null)
                g.ImportVocab(g.implicitLexer);
            //		System.out.println("tokens="+g.tokenNameToTypeMap);
            //		System.out.println("strings="+g.stringLiteralToTypeMap);
            ProcessNonCombinedGrammar(g, gencode);
        }

        public virtual void ProcessNonCombinedGrammar(Grammar g, bool gencode)
        {
            if (g.ast == null || g.ast.hasErrors)
                return;
            if (internalOption_PrintGrammarTree)
                ConsoleOut.WriteLine(g.ast.ToStringTree());

            bool ruleFail = CheckForRuleIssues(g);
            if (ruleFail)
                return;

            int prevErrors = errMgr.GetNumErrors();
            // MAKE SURE GRAMMAR IS SEMANTICALLY CORRECT (FILL IN GRAMMAR OBJECT)
            SemanticPipeline sem = new SemanticPipeline(g);
            sem.Process();

            if (errMgr.GetNumErrors() > prevErrors)
                return;

            // BUILD ATN FROM AST
            ATNFactory factory;
            if (g.IsLexer())
                factory = new LexerATNFactory((LexerGrammar)g);
            else
                factory = new ParserATNFactory(g);
            g.atn = factory.CreateATN();

            if (generate_ATN_dot)
                GenerateATNs(g);

            // PERFORM GRAMMAR ANALYSIS ON ATN: BUILD DECISION DFAs
            AnalysisPipeline anal = new AnalysisPipeline(g);
            anal.Process();

            //if ( generate_DFA_dot ) generateDFAs(g);

            if (g.tool.GetNumErrors() > prevErrors)
                return;

            // GENERATE CODE
            if (gencode)
            {
                CodeGenPipeline gen = new CodeGenPipeline(g);
                gen.Process();
            }
        }

        /**
         * Important enough to avoid multiple definitions that we do very early,
         * right after AST construction. Also check for undefined rules in
         * parser/lexer to avoid exceptions later. Return true if we find multiple
         * definitions of the same rule or a reference to an undefined rule or
         * parser rule ref in lexer rule.
         */
        public virtual bool CheckForRuleIssues(Grammar g)
        {
            // check for redefined rules
            GrammarAST RULES = (GrammarAST)g.ast.GetFirstChildWithType(ANTLRParser.RULES);
            IList<GrammarAST> rules = new List<GrammarAST>(RULES.GetAllChildrenWithType(ANTLRParser.RULE));
            foreach (GrammarAST mode in g.ast.GetAllChildrenWithType(ANTLRParser.MODE))
            {
                foreach (GrammarAST child in mode.GetAllChildrenWithType(ANTLRParser.RULE))
                    rules.Add(child);
            }

            bool redefinition = false;
            IDictionary<string, RuleAST> ruleToAST = new Dictionary<string, RuleAST>();
            foreach (GrammarAST r in rules)
            {
                RuleAST ruleAST = (RuleAST)r;
                GrammarAST ID = (GrammarAST)ruleAST.GetChild(0);
                string ruleName = ID.Text;
                RuleAST prev;
                if (ruleToAST.TryGetValue(ruleName, out prev) && prev != null)
                {
                    GrammarAST prevChild = (GrammarAST)prev.GetChild(0);
                    g.tool.errMgr.GrammarError(ErrorType.RULE_REDEFINITION,
                                               g.fileName,
                                               ID.Token,
                                               ruleName,
                                               prevChild.Token.Line);
                    redefinition = true;
                    continue;
                }
                ruleToAST[ruleName] = ruleAST;
            }

            // check for undefined rules
            UndefChecker chk = new UndefChecker(this, g, ruleToAST);
            chk.VisitGrammar(g.ast);

            return redefinition || chk.badref;
        }

        private class UndefChecker : GrammarTreeVisitor
        {
            public bool badref = false;
            private readonly AntlrTool tool;
            private readonly Grammar g;
            private readonly IDictionary<string, RuleAST> ruleToAST;

            public UndefChecker(AntlrTool tool, Grammar g, IDictionary<string, RuleAST> ruleToAST)
            {
                this.tool = tool;
                this.g = g;
                this.ruleToAST = ruleToAST;
            }

            public override void TokenRef(TerminalAST @ref)
            {
                if ("EOF".Equals(@ref.Text))
                {
                    // this is a special predefined reference
                    return;
                }

                if (g.IsLexer())
                    RuleRef(@ref, null);
            }

            public override void RuleRef(GrammarAST @ref, ActionAST arg)
            {
                RuleAST ruleAST;
                ruleToAST.TryGetValue(@ref.Text, out ruleAST);
                string fileName = @ref.Token.InputStream.SourceName;
                if (char.IsUpper(currentRuleName[0]) &&
                    char.IsLower(@ref.Text[0]))
                {
                    badref = true;
                    tool.errMgr.GrammarError(ErrorType.PARSER_RULE_REF_IN_LEXER_RULE,
                                        fileName, @ref.Token, @ref.Text, currentRuleName);
                }
                else if (ruleAST == null)
                {
                    badref = true;
                    tool.errMgr.GrammarError(ErrorType.UNDEFINED_RULE_REF,
                                        fileName, @ref.Token, @ref.Text);
                }
            }

            public override ErrorManager GetErrorManager()
            {
                return tool.errMgr;
            }
        }

        public virtual IList<GrammarRootAST> SortGrammarByTokenVocab(IList<string> fileNames)
        {
            //System.Console.WriteLine(fileNames);
            Graph<string> g = new Graph<string>();
            IList<GrammarRootAST> roots = new List<GrammarRootAST>();
            foreach (string fileName in fileNames)
            {
                GrammarAST t = ParseGrammar(fileName);
                if (t == null || t is GrammarASTErrorNode)
                    continue; // came back as error node
                if (((GrammarRootAST)t).hasErrors)
                    continue;
                GrammarRootAST root = (GrammarRootAST)t;
                roots.Add(root);
                root.fileName = fileName;
                string grammarName = root.GetChild(0).Text;

                GrammarAST tokenVocabNode = FindOptionValueAST(root, "tokenVocab");
                // Make grammars depend on any tokenVocab options
                if (tokenVocabNode != null)
                {
                    string vocabName = tokenVocabNode.Text;
                    // Strip quote characters if any
                    int len = vocabName.Length;
                    int firstChar = vocabName[0];
                    int lastChar = vocabName[len - 1];
                    if (len >= 2 && firstChar == '\'' && lastChar == '\'')
                    {
                        vocabName = vocabName.Substring(1, len - 2);
                    }
                    // If the name contains a path delimited by forward slashes,
                    // use only the part after the last slash as the name
                    int lastSlash = vocabName.LastIndexOf('/');
                    if (lastSlash >= 0)
                    {
                        vocabName = vocabName.Substring(lastSlash + 1);
                    }
                    g.AddEdge(grammarName, vocabName);
                }
                // add cycle to graph so we always process a grammar if no error
                // even if no dependency
                g.AddEdge(grammarName, grammarName);
            }

            IList<string> sortedGrammarNames = g.Sort();
            //System.Console.WriteLine("sortedGrammarNames=" + sortedGrammarNames);

            IList<GrammarRootAST> sortedRoots = new List<GrammarRootAST>();
            foreach (string grammarName in sortedGrammarNames)
            {
                foreach (GrammarRootAST root in roots)
                {
                    if (root.GetGrammarName().Equals(grammarName))
                    {
                        sortedRoots.Add(root);
                        break;
                    }
                }
            }

            return sortedRoots;
        }

        /** Manually get option node from tree; return null if no defined. */
        public static GrammarAST FindOptionValueAST(GrammarRootAST root, string option)
        {
            GrammarAST options = (GrammarAST)root.GetFirstChildWithType(ANTLRParser.OPTIONS);
            if (options != null && options.ChildCount > 0)
            {
                foreach (object o in options.Children)
                {
                    GrammarAST c = (GrammarAST)o;
                    if (c.Type == ANTLRParser.ASSIGN &&
                         c.GetChild(0).Text.Equals(option))
                    {
                        return (GrammarAST)c.GetChild(1);
                    }
                }
            }
            return null;
        }


        /** Given the raw AST of a grammar, create a grammar object
            associated with the AST. Once we have the grammar object, ensure
            that all nodes in tree referred to this grammar. Later, we will
            use it for error handling and generally knowing from where a rule
            comes from.
         */
        public virtual Grammar CreateGrammar(GrammarRootAST ast)
        {
            Grammar g;
            if (ast.grammarType == ANTLRParser.LEXER)
                g = new LexerGrammar(this, ast);
            else
                g = new Grammar(this, ast);

            // ensure each node has pointer to surrounding grammar
            GrammarTransformPipeline.SetGrammarPtr(g, ast);
            return g;
        }

        public virtual GrammarRootAST ParseGrammar(string fileName)
        {
            try
            {
                string file = fileName;
                if (!Path.IsPathRooted(file))
                {
                    file = Path.Combine(inputDirectory, fileName);
                }

                string fileContent = File.ReadAllText(file, Encoding.GetEncoding(grammarEncoding));
                char[] fileChars = fileContent.ToCharArray();
                ANTLRStringStream @in = new ANTLRStringStream(fileChars, fileChars.Length, fileName);
                GrammarRootAST t = Parse(fileName, @in);
                return t;
            }
            catch (IOException ioe)
            {
                errMgr.ToolError(ErrorType.CANNOT_OPEN_FILE, ioe, fileName);
            }
            return null;
        }

        /** Convenience method to load and process an ANTLR grammar. Useful
         *  when creating interpreters.  If you need to access to the lexer
         *  grammar created while processing a combined grammar, use
         *  getImplicitLexer() on returned grammar.
         */
        public virtual Grammar LoadGrammar(string fileName)
        {
            GrammarRootAST grammarRootAST = ParseGrammar(fileName);
            Grammar g = CreateGrammar(grammarRootAST);
            g.fileName = fileName;
            Process(g, false);
            return g;
        }

        private readonly IDictionary<string, Grammar> importedGrammars = new Dictionary<string, Grammar>();

        /**
         * Try current dir then dir of g then lib dir
         * @param g
         * @param nameNode The node associated with the imported grammar name.
         */
        public virtual Grammar LoadImportedGrammar(Grammar g, GrammarAST nameNode)
        {
            string name = nameNode.Text;
            Grammar imported;
            if (!importedGrammars.TryGetValue(name, out imported) || imported == null)
            {
                g.tool.Log("grammar", "load " + name + " from " + g.fileName);
                string importedFile = null;
                foreach (string extension in ALL_GRAMMAR_EXTENSIONS)
                {
                    importedFile = GetImportedGrammarFile(g, name + extension);
                    if (importedFile != null)
                    {
                        break;
                    }
                }

                if (importedFile == null)
                {
                    errMgr.GrammarError(ErrorType.CANNOT_FIND_IMPORTED_GRAMMAR, g.fileName, nameNode.Token, name);
                    return null;
                }

                string absolutePath = Path.GetFullPath(importedFile);
                string fileContent = File.ReadAllText(absolutePath, Encoding.GetEncoding(grammarEncoding));
                char[] fileChars = fileContent.ToCharArray();
                ANTLRStringStream @in = new ANTLRStringStream(fileChars, fileChars.Length, importedFile);
                GrammarRootAST root = Parse(g.fileName, @in);
                if (root == null)
                {
                    return null;
                }

                imported = CreateGrammar(root);
                imported.fileName = absolutePath;
                importedGrammars[root.GetGrammarName()] = imported;
            }

            return imported;
        }

        public virtual GrammarRootAST ParseGrammarFromString(string grammar)
        {
            return Parse("<string>", new ANTLRStringStream(grammar));
        }

        public virtual GrammarRootAST Parse(string fileName, ICharStream @in)
        {
            try
            {
                GrammarASTAdaptor adaptor = new GrammarASTAdaptor(@in);
                ToolANTLRLexer lexer = new ToolANTLRLexer(@in, this);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                lexer.tokens = tokens;
                ToolANTLRParser p = new ToolANTLRParser(tokens, this);
                p.TreeAdaptor = adaptor;
                try
                {
                    var r = p.grammarSpec();
                    GrammarAST root = (GrammarAST)r.Tree;
                    if (root is GrammarRootAST)
                    {
                        ((GrammarRootAST)root).hasErrors = lexer.NumberOfSyntaxErrors > 0 || p.NumberOfSyntaxErrors > 0;
                        Debug.Assert(((GrammarRootAST)root).tokenStream == tokens);
                        if (grammarOptions != null)
                        {
                            ((GrammarRootAST)root).cmdLineOptions = grammarOptions;
                        }
                        return ((GrammarRootAST)root);
                    }
                }
                catch (v3TreeGrammarException e)
                {
                    errMgr.GrammarError(ErrorType.V3_TREE_GRAMMAR, fileName, e.location);
                }
                return null;
            }
            catch (RecognitionException)
            {
                // TODO: do we gen errors now?
                errMgr.InternalError("can't generate this message at moment; antlr recovers");
            }
            return null;
        }

        public virtual void GenerateATNs(Grammar g)
        {
            DOTGenerator dotGenerator = new DOTGenerator(g);
            IList<Grammar> grammars = new List<Grammar>();
            grammars.Add(g);
            IList<Grammar> imported = g.GetAllImportedGrammars();
            if (imported != null)
            {
                foreach (Grammar importedGrammar in imported)
                    grammars.Add(importedGrammar);
            }

            foreach (Grammar ig in grammars)
            {
                foreach (Rule r in ig.rules.Values)
                {
                    try
                    {
                        string dot = dotGenerator.GetDOT(g.atn.ruleToStartState[r.index], g.IsLexer());
                        if (dot != null)
                        {
                            WriteDOTFile(g, r, dot);
                        }
                    }
                    catch (IOException ioe)
                    {
                        errMgr.ToolError(ErrorType.CANNOT_WRITE_FILE, ioe);
                    }
                }
            }
        }

        /** This method is used by all code generators to create new output
         *  files. If the outputDir set by -o is not present it will be created.
         *  The final filename is sensitive to the output directory and
         *  the directory where the grammar file was found.  If -o is /tmp
         *  and the original grammar file was foo/t.g4 then output files
         *  go in /tmp/foo.
         *
         *  The output dir -o spec takes precedence if it's absolute.
         *  E.g., if the grammar file dir is absolute the output dir is given
         *  precedence. "-o /tmp /usr/lib/t.g4" results in "/tmp/T.java" as
         *  output (assuming t.g4 holds T.java).
         *
         *  If no -o is specified, then just write to the directory where the
         *  grammar file was found.
         *
         *  If outputDirectory==null then write a String.
         */
        public virtual TextWriter GetOutputFileWriter(Grammar g, string fileName)
        {
            if (outputDirectory == null)
            {
                return new StringWriter();
            }

            // output directory is a function of where the grammar file lives
            // for subdir/T.g4, you get subdir here.  Well, depends on -o etc...
            string outputDir = GetOutputDirectory(g.fileName);
            string outputFile = Path.Combine(outputDir, fileName);
            ConsoleOut.WriteLine($"Generating file '{Path.GetFullPath(outputFile)}' for grammar '{g.fileName}'");

            Directory.CreateDirectory(outputDir);

            return new StreamWriter(File.Open(outputFile, FileMode.Create), Encoding.GetEncoding(grammarEncoding));
        }

        public virtual string GetImportedGrammarFile(Grammar g, string fileName)
        {
            string importedFile = Path.Combine(inputDirectory ?? string.Empty, fileName);
            if (!File.Exists(importedFile))
            {
                string gfile = g.fileName;
                string parentDir = Path.GetDirectoryName(gfile);
                importedFile = Path.Combine(parentDir, fileName);
                if (!File.Exists(importedFile))
                {
                    // try in lib dir
                    importedFile = Path.Combine(libDirectory ?? string.Empty, fileName);
                    if (!File.Exists(importedFile))
                    {
                        return null;
                    }
                }
            }

            return importedFile;
        }

        /**
         * Return the location where ANTLR will generate output files for a given
         * file. This is a base directory and output files will be relative to
         * here in some cases such as when -o option is used and input files are
         * given relative to the input directory.
         *
         * @param fileNameWithPath path to input source
         */
        public virtual string GetOutputDirectory(string fileNameWithPath)
        {
            string outputDir;
            string fileDirectory;

            // Some files are given to us without a PATH but should should
            // still be written to the output directory in the relative path of
            // the output directory. The file directory is either the set of sub directories
            // or just or the relative path recorded for the parent grammar. This means
            // that when we write the tokens files, or the .java files for imported grammars
            // taht we will write them in the correct place.
            if (fileNameWithPath.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) == -1)
            {
                // No path is included in the file name, so make the file
                // directory the same as the parent grammar (which might sitll be just ""
                // but when it is not, we will write the file in the correct place.
                fileDirectory = ".";

            }
            else
            {
                fileDirectory = fileNameWithPath.Substring(0, fileNameWithPath.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }));
            }
            if (haveOutputDir)
            {
                // -o /tmp /var/lib/t.g4 => /tmp/T.java
                // -o subdir/output /usr/lib/t.g4 => subdir/output/T.java
                // -o . /usr/lib/t.g4 => ./T.java
                if (fileDirectory != null &&
                    (Path.IsPathRooted(fileDirectory) ||
                     fileDirectory.StartsWith("~")))
                { // isAbsolute doesn't count this :(
                  // somebody set the dir, it takes precedence; write new file there
                    outputDir = outputDirectory;
                }
                else
                {
                    // -o /tmp subdir/t.g4 => /tmp/subdir/t.g4
                    if (fileDirectory != null)
                    {
                        outputDir = Path.Combine(outputDirectory, fileDirectory);
                    }
                    else
                    {
                        outputDir = outputDirectory;
                    }
                }
            }
            else
            {
                // they didn't specify a -o dir so just write to location
                // where grammar is, absolute or relative, this will only happen
                // with command line invocation as build tools will always
                // supply an output directory.
                outputDir = fileDirectory;
            }

            return outputDir;
        }

        protected virtual void WriteDOTFile(Grammar g, Rule r, string dot)
        {
            WriteDOTFile(g, r.g.name + "." + r.name, dot);
        }

        protected virtual void WriteDOTFile(Grammar g, string name, string dot)
        {
            using (TextWriter fw = GetOutputFileWriter(g, name + ".dot"))
            {
                fw.Write(dot);
            }
        }

        public virtual void Help()
        {
            Info("ANTLR Parser Generator  Version " + AntlrTool.VERSION);
            foreach (Option o in optionDefs)
            {
                string name = o.name + (o.argType != OptionArgType.NONE ? " ___" : "");
                string s = string.Format(" {0} {1}", name, o.description);
                Info(s);
            }
        }

        public virtual void Log([Nullable] string component, string msg)
        {
            logMgr.Log(component, msg);
        }
        public virtual void Log(string msg)
        {
            Log(null, msg);
        }

        public virtual int GetNumErrors()
        {
            return errMgr.GetNumErrors();
        }

        public virtual void AddListener(ANTLRToolListener tl)
        {
            if (tl != null)
                listeners.Add(tl);
        }
        public virtual void RemoveListener(ANTLRToolListener tl)
        {
            listeners.Remove(tl);
        }
        public virtual void RemoveListeners()
        {
            listeners.Clear();
        }
        public virtual IList<ANTLRToolListener> GetListeners()
        {
            return listeners;
        }

        public virtual void Info(string msg)
        {
            if (listeners.Count == 0)
            {
                defaultListener.Info(msg);
                return;
            }

            foreach (ANTLRToolListener l in listeners)
                l.Info(msg);
        }

        public virtual void Error(ANTLRMessage msg)
        {
            if (listeners.Count == 0)
            {
                defaultListener.Error(msg);
                return;
            }

            foreach (ANTLRToolListener l in listeners)
                l.Error(msg);
        }
        public virtual void Warning(ANTLRMessage msg)
        {
            if (listeners.Count == 0)
            {
                defaultListener.Warning(msg);
            }
            else
            {
                foreach (ANTLRToolListener l in listeners)
                    l.Warning(msg);
            }

            if (warnings_are_errors)
            {
                errMgr.Emit(ErrorType.WARNING_TREATED_AS_ERROR, new ANTLRMessage(ErrorType.WARNING_TREATED_AS_ERROR));
            }
        }

        public virtual void Version()
        {
            Info("ANTLR Parser Generator  Version " + VERSION);
        }

        public virtual void Panic()
        {
            throw new Exception("ANTLR panic");
        }
    }
}
