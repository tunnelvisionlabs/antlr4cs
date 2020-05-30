// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Antlr4.Analysis;
    using Antlr4.Automata;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Runtime.Atn;
    using Antlr4.Runtime.Dfa;
    using Antlr4.Tool.Ast;
    using ArgumentException = System.ArgumentException;
    using ArgumentNullException = System.ArgumentNullException;
    using ICharStream = Antlr4.Runtime.ICharStream;
    using IIntSet = Antlr4.Runtime.Misc.IIntSet;
    using Interval = Antlr4.Runtime.Misc.Interval;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;
    using InvalidOperationException = System.InvalidOperationException;
    using IOException = System.IO.IOException;
    using ITokenStream = Antlr4.Runtime.ITokenStream;
    using IVocabulary = Antlr4.Runtime.IVocabulary;
    using Lexer = Antlr4.Runtime.Lexer;
    using LexerInterpreter = Antlr4.Runtime.LexerInterpreter;
    using Math = System.Math;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NotSupportedException = System.NotSupportedException;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using ParserInterpreter = Antlr4.Runtime.ParserInterpreter;
    using TokenConstants = Antlr4.Runtime.TokenConstants;
    using Tuple = System.Tuple;
    using Vocabulary = Antlr4.Runtime.Vocabulary;

    public class Grammar : AttributeResolver
    {
        public static readonly string GRAMMAR_FROM_STRING_NAME = "<string>";
        /**
         * This value is used in the following situations to indicate that a token
         * type does not have an associated name which can be directly referenced in
         * a grammar.
         *
         * <ul>
         * <li>This value is the name and display name for the token with type
         * {@link Token#INVALID_TYPE}.</li>
         * <li>This value is the name for tokens with a type not represented by a
         * named token. The display name for these tokens is simply the string
         * representation of the token type as an integer.</li>
         * </ul>
         */
        public static readonly string INVALID_TOKEN_NAME = "<INVALID>";
        /**
         * This value is used as the name for elements in the array returned by
         * {@link #getRuleNames} for indexes not associated with a rule.
         */
        public static readonly string INVALID_RULE_NAME = "<invalid>";

        public static readonly ISet<string> parserOptions =
                new HashSet<string>
                {
                "superClass",
                "contextSuperClass",
                "TokenLabelType",
                "abstract",
                "tokenVocab",
                "language",
                "exportMacro",
                "useInternalAccessModifier",
                "includeDebuggerNonUserCodeAttribute",
                "excludeClsCompliantAttribute",
                };

        public static readonly ISet<string> lexerOptions = parserOptions;

        public static readonly ISet<string> ruleOptions =
            new HashSet<string>
            {
                "baseContext",
            };

        public static readonly ISet<string> ParserBlockOptions =
            new HashSet<string>
            {
                "sll",
            };

        public static readonly ISet<string> LexerBlockOptions = new HashSet<string>();

        /** Legal options for rule refs like id&lt;key=value&gt; */
        public static readonly ISet<string> ruleRefOptions =
            new HashSet<string>
            {
                LeftRecursiveRuleTransformer.PRECEDENCE_OPTION_NAME,
                LeftRecursiveRuleTransformer.TOKENINDEX_OPTION_NAME,
            };

        /** Legal options for terminal refs like ID&lt;assoc=right&gt; */
        public static readonly ISet<string> tokenOptions =
            new HashSet<string>
            {
                "assoc",
                LeftRecursiveRuleTransformer.TOKENINDEX_OPTION_NAME,
            };

        public static readonly ISet<string> actionOptions = new HashSet<string>();

        public static readonly ISet<string> semPredOptions =
            new HashSet<string>
            {
                LeftRecursiveRuleTransformer.PRECEDENCE_OPTION_NAME,
                "fail",
            };

        public static readonly ISet<string> doNotCopyOptionsToLexer =
            new HashSet<string>
            {
                "superClass",
                "TokenLabelType",
                "abstract",
                "tokenVocab",
            };

        public static readonly IDictionary<string, AttributeDict> grammarAndLabelRefTypeToScope =
            new Dictionary<string, AttributeDict>
            {
                { "parser:RULE_LABEL", Rule.predefinedRulePropertiesDict },
                { "parser:TOKEN_LABEL", AttributeDict.predefinedTokenDict },
                { "combined:RULE_LABEL", Rule.predefinedRulePropertiesDict },
                { "combined:TOKEN_LABEL", AttributeDict.predefinedTokenDict },
            };

        public string name;
        public GrammarRootAST ast;

        /** Track token stream used to create this grammar */
        [NotNull]
        public readonly Antlr.Runtime.ITokenStream tokenStream;

        /** If we transform grammar, track original unaltered token stream.
         *  This is set to the same value as tokenStream when tokenStream is
         *  initially set.
         *
         *  If this field differs from tokenStream, then we have transformed
         *  the grammar.
         */
        [NotNull]
        public readonly Antlr.Runtime.ITokenStream originalTokenStream;

        public string text; // testing only
        public string fileName;

        /** Was this parser grammar created from a COMBINED grammar?  If so,
         *  this is what we extracted.
         */
        public LexerGrammar implicitLexer;

        /** If this is an extracted/implicit lexer, we point at original grammar */
        public Grammar originalGrammar;

        /** If we're imported, who imported us? If null, implies grammar is root */
        public Grammar parent;
        public IList<Grammar> importedGrammars;

        /** All rules defined in this specific grammar, not imported. Also does
         *  not include lexical rules if combined.
         */
        public OrderedHashMap<string, Rule> rules = new OrderedHashMap<string, Rule>();
        public IList<Rule> indexToRule = new List<Rule>();

        /**
         * This maps a context name → a collection of {@link RuleAST} nodes in
         * the original grammar. The union of accessors and labels identified by
         * these ASTs define the accessor methods and fields of the generated
         * context classes.
         *
         * <p>
         * The keys of this map match the result of {@link Rule#getBaseContext}.</p>
         * <p>
         * The values in this map are clones of the nodes in the original grammar
         * (provided by {@link GrammarAST#dupTree}) to ensure that grammar
         * transformations do not affect the values generated for the contexts. The
         * duplication is performed after nodes from imported grammars are merged
         * into the AST.</p>
         */
        public readonly IDictionary<string, IList<RuleAST>> contextASTs = new Dictionary<string, IList<RuleAST>>();

        int ruleNumber = 0; // used to get rule indexes (0..n-1)
        int stringLiteralRuleNumber = 0; // used to invent rule names for 'keyword', ';', ... (0..n-1)

        /** The ATN that represents the grammar with edges labelled with tokens
         *  or epsilon.  It is more suitable to analysis than an AST representation.
         */
        public ATN atn;

        public IDictionary<int, Interval> stateToGrammarRegionMap;

        public IDictionary<int, DFA> decisionDFAs = new Dictionary<int, DFA>();

        public IList<IntervalSet[]> decisionLOOK;

        [NotNull]
        public readonly AntlrTool tool;

        /** Token names and literal tokens like "void" are uniquely indexed.
         *  with -1 implying EOF.  Characters are different; they go from
         *  -1 (EOF) to \uFFFE.  For example, 0 could be a binary byte you
         *  want to lexer.  Labels of DFA/ATN transitions can be both tokens
         *  and characters.  I use negative numbers for bookkeeping labels
         *  like EPSILON. Char/String literals and token types overlap in the same
         *  space, however.
         */
        int maxTokenType = TokenConstants.MinUserTokenType - 1;

        /**
         * Map token like {@code ID} (but not literals like {@code 'while'}) to its
         * token type.
         */
        public readonly IDictionary<string, int> tokenNameToTypeMap = new LinkedHashMap<string, int>();

        /**
         * Map token literals like {@code 'while'} to its token type. It may be that
         * {@code WHILE="while"=35}, in which case both {@link #tokenNameToTypeMap}
         * and this field will have entries both mapped to 35.
         */
        public readonly IDictionary<string, int> stringLiteralToTypeMap = new LinkedHashMap<string, int>();

        /**
         * Reverse index for {@link #stringLiteralToTypeMap}. Indexed with raw token
         * type. 0 is invalid.
         */
        public readonly IList<string> typeToStringLiteralList = new List<string>();

        /**
         * Map a token type to its token name. Indexed with raw token type. 0 is
         * invalid.
         */
        public readonly IList<string> typeToTokenList = new List<string>();

        /**
         * The maximum channel value which is assigned by this grammar. Values below
         * {@link Token#MIN_USER_CHANNEL_VALUE} are assumed to be predefined.
         */
        int maxChannelType = TokenConstants.MinUserChannelValue - 1;

        /**
         * Map channel like {@code COMMENTS_CHANNEL} to its constant channel value.
         * Only user-defined channels are defined in this map.
         */
        public readonly IDictionary<string, int> channelNameToValueMap = new LinkedHashMap<string, int>();

        /**
         * Map a constant channel value to its name. Indexed with raw channel value.
         * The predefined channels {@link Token#DEFAULT_CHANNEL} and
         * {@link Token#HIDDEN_CHANNEL} are not stored in this list, so the values
         * at the corresponding indexes is {@code null}.
         */
        public readonly IList<string> channelValueToNameList = new List<string>();

        /** Map a name to an action.
         *  The code generator will use this to fill holes in the output files.
         *  I track the AST node for the action in case I need the line number
         *  for errors.
         */
        public IDictionary<string, ActionAST> namedActions = new Dictionary<string, ActionAST>();

        /** Tracks all user lexer actions in all alternatives of all rules.
         *  Doesn't track sempreds.  maps tree node to action index (alt number 1..n).
         */
        public LinkedHashMap<ActionAST, int> lexerActions = new LinkedHashMap<ActionAST, int>();

        /** All sempreds found in grammar; maps tree node to sempred index;
         *  sempred index is 0..n-1
         */
        public LinkedHashMap<PredAST, int> sempreds = new LinkedHashMap<PredAST, int>();
        /** Map the other direction upon demand */
        public LinkedHashMap<int, PredAST> indexToPredMap;

        public static readonly string AUTO_GENERATED_TOKEN_NAME_PREFIX = "T__";

        public Grammar(AntlrTool tool, [NotNull] GrammarRootAST ast)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            if (ast.tokenStream == null)
            {
                throw new ArgumentException("ast must have a token stream", nameof(ast));
            }

            this.tool = tool;
            this.ast = ast;
            this.name = (ast.GetChild(0)).Text;
            this.tokenStream = ast.tokenStream;
            this.originalTokenStream = this.tokenStream;

            InitTokenSymbolTables();
        }

        /** For testing */
        public Grammar(string grammarText)
            : this(GRAMMAR_FROM_STRING_NAME, grammarText, null)
        {
        }

        public Grammar(string grammarText, LexerGrammar tokenVocabSource)
            : this(GRAMMAR_FROM_STRING_NAME, grammarText, tokenVocabSource, null)
        {
        }

        /** For testing */
        public Grammar(string grammarText, ANTLRToolListener listener)
            : this(GRAMMAR_FROM_STRING_NAME, grammarText, listener)
        {
        }

        /** For testing; builds trees, does sem anal */
        public Grammar(string fileName, string grammarText)
            : this(fileName, grammarText, null)
        {
        }

        /** For testing; builds trees, does sem anal */
        public Grammar(string fileName, string grammarText, [Nullable] ANTLRToolListener listener)
            : this(fileName, grammarText, null, listener)
        {
        }

        /** For testing; builds trees, does sem anal */
        public Grammar(string fileName, string grammarText, Grammar tokenVocabSource, [Nullable] ANTLRToolListener listener)
        {
            this.text = grammarText;
            this.fileName = fileName;
            this.tool = new AntlrTool();
            this.tool.AddListener(listener);
            Antlr.Runtime.ANTLRStringStream @in = new Antlr.Runtime.ANTLRStringStream(grammarText);
            @in.name = fileName;

            this.ast = tool.Parse(fileName, @in);
            if (ast == null)
            {
                throw new NotSupportedException();
            }

            if (ast.tokenStream == null)
            {
                throw new InvalidOperationException("expected ast to have a token stream");
            }

            this.tokenStream = ast.tokenStream;
            this.originalTokenStream = this.tokenStream;

            // ensure each node has pointer to surrounding grammar
            Antlr.Runtime.Tree.TreeVisitor v = new Antlr.Runtime.Tree.TreeVisitor(new GrammarASTAdaptor());
            v.Visit(ast, new SetPointersAction(this));
            InitTokenSymbolTables();

            if (tokenVocabSource != null)
            {
                ImportVocab(tokenVocabSource);
            }

            tool.Process(this, false);
        }

        private sealed class SetPointersAction : Antlr.Runtime.Tree.ITreeVisitorAction
        {
            private readonly Grammar grammar;

            public SetPointersAction(Grammar grammar)
            {
                this.grammar = grammar;
            }

            public object Pre(object t)
            {
                ((GrammarAST)t).g = grammar;
                return t;
            }
            public object Post(object t)
            {
                return t;
            }
        }

        protected virtual void InitTokenSymbolTables()
        {
            tokenNameToTypeMap["EOF"] = TokenConstants.Eof;

            // reserve a spot for the INVALID token
            typeToTokenList.Add(null);
        }

        public virtual void LoadImportedGrammars()
        {
            if (ast == null)
                return;
            GrammarAST i = (GrammarAST)ast.GetFirstChildWithType(ANTLRParser.IMPORT);
            if (i == null)
                return;
            ISet<string> visited = new HashSet<string>();
            visited.Add(this.name);
            importedGrammars = new List<Grammar>();
            foreach (object c in i.Children)
            {
                GrammarAST t = (GrammarAST)c;
                string importedGrammarName = null;
                if (t.Type == ANTLRParser.ASSIGN)
                {
                    t = (GrammarAST)t.GetChild(1);
                    importedGrammarName = t.Text;
                }
                else if (t.Type == ANTLRParser.ID)
                {
                    importedGrammarName = t.Text;
                }
                if (visited.Contains(importedGrammarName))
                {
                    // ignore circular refs
                    continue;
                }
                Grammar g;
                try
                {
                    g = tool.LoadImportedGrammar(this, t);
                }
                catch (IOException)
                {
                    tool.errMgr.GrammarError(ErrorType.ERROR_READING_IMPORTED_GRAMMAR,
                                             importedGrammarName,
                                             t.Token,
                                             importedGrammarName,
                                             name);
                    continue;
                }
                // did it come back as error node or missing?
                if (g == null)
                    continue;
                g.parent = this;
                importedGrammars.Add(g);
                g.LoadImportedGrammars(); // recursively pursue any imports in this import
            }
        }

        public virtual void DefineAction(GrammarAST atAST)
        {
            if (atAST.ChildCount == 2)
            {
                string name = atAST.GetChild(0).Text;
                namedActions[name] = (ActionAST)atAST.GetChild(1);
            }
            else
            {
                string scope = atAST.GetChild(0).Text;
                string gtype = GetTypeString();
                if (scope.Equals(gtype) || (scope.Equals("parser") && gtype.Equals("combined")))
                {
                    string name = atAST.GetChild(1).Text;
                    namedActions[name] = (ActionAST)atAST.GetChild(2);
                }
            }
        }

        /**
         * Define the specified rule in the grammar. This method assigns the rule's
         * {@link Rule#index} according to the {@link #ruleNumber} field, and adds
         * the {@link Rule} instance to {@link #rules} and {@link #indexToRule}.
         *
         * @param r The rule to define in the grammar.
         * @return {@code true} if the rule was added to the {@link Grammar}
         * instance; otherwise, {@code false} if a rule with this name already
         * existed in the grammar instance.
         */
        public virtual bool DefineRule([NotNull] Rule r)
        {
            Rule rule;
            if (rules.TryGetValue(r.name, out rule))
            {
                return false;
            }

            rules[r.name] = r;
            r.index = ruleNumber++;
            indexToRule.Add(r);
            return true;
        }

        /**
         * Undefine the specified rule from this {@link Grammar} instance. The
         * instance {@code r} is removed from {@link #rules} and
         * {@link #indexToRule}. This method updates the {@link Rule#index} field
         * for all rules defined after {@code r}, and decrements {@link #ruleNumber}
         * in preparation for adding new rules.
         * <p>
         * This method does nothing if the current {@link Grammar} does not contain
         * the instance {@code r} at index {@code r.index} in {@link #indexToRule}.
         * </p>
         *
         * @param r
         * @return {@code true} if the rule was removed from the {@link Grammar}
         * instance; otherwise, {@code false} if the specified rule was not defined
         * in the grammar.
         */
        public virtual bool UndefineRule([NotNull] Rule r)
        {
            if (r.index < 0 || r.index >= indexToRule.Count || indexToRule[r.index] != r)
            {
                return false;
            }

            Debug.Assert(rules.ContainsKey(r.name) && rules[r.name] == r);

            rules.Remove(r.name);
            indexToRule.RemoveAt(r.index);
            for (int i = r.index; i < indexToRule.Count; i++)
            {
                Debug.Assert(indexToRule[i].index == i + 1);
                indexToRule[i].index--;
            }

            ruleNumber--;
            return true;
        }

        //	public int getNumRules() {
        //		int n = rules.size();
        //		List<Grammar> imports = getAllImportedGrammars();
        //		if ( imports!=null ) {
        //			for (Grammar g : imports) n += g.getNumRules();
        //		}
        //		return n;
        //	}

        public virtual Rule GetRule(string name)
        {
            Rule r;
            if (rules.TryGetValue(name, out r))
                return r;
            return null;
            /*
            List<Grammar> imports = getAllImportedGrammars();
            if ( imports==null ) return null;
            for (Grammar g : imports) {
                r = g.getRule(name); // recursively walk up hierarchy
                if ( r!=null ) return r;
            }
            return null;
            */
        }

        public virtual ATN GetATN()
        {
            if (atn == null)
            {
                ParserATNFactory factory = new ParserATNFactory(this);
                atn = factory.CreateATN();
            }
            return atn;
        }

        public virtual Rule GetRule(int index)
        {
            return indexToRule[index];
        }

        public virtual Rule GetRule(string grammarName, string ruleName)
        {
            if (grammarName != null)
            { // scope override
                Grammar g = GetImportedGrammar(grammarName);
                if (g == null)
                {
                    return null;
                }

                Rule r;
                g.rules.TryGetValue(ruleName, out r);
                return r;
            }

            return GetRule(ruleName);
        }

        protected virtual string GetBaseContextName(string ruleName)
        {
            Rule referencedRule;
            if (rules.TryGetValue(ruleName, out referencedRule))
            {
                ruleName = referencedRule.GetBaseContext();
            }

            return ruleName;
        }

        public virtual IList<AltAST> GetUnlabeledAlternatives(RuleAST ast)
        {
            AltLabelVisitor visitor = new AltLabelVisitor(new Antlr.Runtime.Tree.CommonTreeNodeStream(new GrammarASTAdaptor(), ast));
            visitor.rule();
            return visitor.GetUnlabeledAlternatives();
        }

        public virtual IDictionary<string, IList<System.Tuple<int, AltAST>>> GetLabeledAlternatives(RuleAST ast)
        {
            AltLabelVisitor visitor = new AltLabelVisitor(new Antlr.Runtime.Tree.CommonTreeNodeStream(new GrammarASTAdaptor(), ast));
            visitor.rule();
            return visitor.GetLabeledAlternatives();
        }

        /** Get list of all imports from all grammars in the delegate subtree of g.
         *  The grammars are in import tree preorder.  Don't include ourselves
         *  in list as we're not a delegate of ourselves.
         */
        public virtual IList<Grammar> GetAllImportedGrammars()
        {
            if (importedGrammars == null)
            {
                return null;
            }

            LinkedHashMap<string, Grammar> delegates = new LinkedHashMap<string, Grammar>();
            foreach (Grammar d in importedGrammars)
            {
                delegates[d.fileName] = d;
                IList<Grammar> ds = d.GetAllImportedGrammars();
                if (ds != null)
                {
                    foreach (Grammar imported in ds)
                    {
                        delegates[imported.fileName] = imported;
                    }
                }
            }

            return new List<Grammar>(delegates.Values);
        }

        public virtual IList<Grammar> GetImportedGrammars()
        {
            return importedGrammars;
        }

        public virtual LexerGrammar GetImplicitLexer()
        {
            return implicitLexer;
        }

        /** convenience method for Tool.loadGrammar() */
        public static Grammar Load(string fileName)
        {
            AntlrTool antlr = new AntlrTool();
            return antlr.LoadGrammar(fileName);
        }

        /** Return list of imported grammars from root down to our parent.
         *  Order is [root, ..., this.parent].  (us not included).
         */
        public virtual IList<Grammar> GetGrammarAncestors()
        {
            Grammar root = GetOutermostGrammar();
            if (this == root)
                return null;
            IList<Grammar> grammars = new List<Grammar>();
            // walk backwards to root, collecting grammars
            Grammar p = this.parent;
            while (p != null)
            {
                grammars.Insert(0, p); // add to head so in order later
                p = p.parent;
            }
            return grammars;
        }

        /** Return the grammar that imported us and our parents. Return this
         *  if we're root.
         */
        public virtual Grammar GetOutermostGrammar()
        {
            if (parent == null)
                return this;
            return parent.GetOutermostGrammar();
        }

        public virtual bool IsAbstract()
        {
            return bool.Parse(GetOptionString("abstract"));
        }

        /** Get the name of the generated recognizer; may or may not be same
         *  as grammar name.
         *  Recognizer is TParser and TLexer from T if combined, else
         *  just use T regardless of grammar type.
         */
        public virtual string GetRecognizerName()
        {
            string suffix = "";
            IList<Grammar> grammarsFromRootToMe = GetOutermostGrammar().GetGrammarAncestors();
            string qualifiedName = name;
            if (grammarsFromRootToMe != null)
            {
                StringBuilder buf = new StringBuilder();
                foreach (Grammar g in grammarsFromRootToMe)
                {
                    buf.Append(g.name);
                    buf.Append('_');
                }
                if (IsAbstract())
                {
                    buf.Append("Abstract");
                }
                buf.Append(name);
                qualifiedName = buf.ToString();
            }
            else if (IsAbstract())
            {
                qualifiedName = "Abstract" + name;
            }

            if (IsCombined() || (IsLexer() && implicitLexer != null))
            {
                suffix = Grammar.GetGrammarTypeToFileNameSuffix(Type);
            }
            return qualifiedName + suffix;
        }

        public virtual string GetStringLiteralLexerRuleName(string lit)
        {
            return AUTO_GENERATED_TOKEN_NAME_PREFIX + stringLiteralRuleNumber++;
        }

        /** Return grammar directly imported by this grammar */
        public virtual Grammar GetImportedGrammar(string name)
        {
            foreach (Grammar g in importedGrammars)
            {
                if (g.name.Equals(name))
                    return g;
            }
            return null;
        }

        public virtual int GetTokenType(string token)
        {
            int? I = null;
            if (token[0] == '\'')
            {
                int value;
                if (stringLiteralToTypeMap.TryGetValue(token, out value))
                    I = value;
            }
            else
            {
                // must be a label like ID
                int value;
                if (tokenNameToTypeMap.TryGetValue(token, out value))
                    I = value;
            }

            int i = I ?? TokenConstants.InvalidType;
            //tool.Log("grammar", "grammar type " + type + " " + tokenName + "->" + i);
            return i;
        }

        /** Given a token type, get a meaningful name for it such as the ID
         *  or string literal.  If this is a lexer and the ttype is in the
         *  char vocabulary, compute an ANTLR-valid (possibly escaped) char literal.
         */
        public virtual string GetTokenDisplayName(int ttype)
        {
            // inside any target's char range and is lexer grammar?
            if (IsLexer() &&
                 ttype >= Lexer.MinCharValue && ttype <= Lexer.MaxCharValue)
            {
                return CharSupport.GetANTLRCharLiteralForChar(ttype);
            }

            if (ttype == TokenConstants.Eof)
            {
                return "EOF";
            }

            if (ttype == TokenConstants.InvalidType)
            {
                return INVALID_TOKEN_NAME;
            }

            if (ttype >= 0 && ttype < typeToStringLiteralList.Count && typeToStringLiteralList[ttype] != null)
            {
                return typeToStringLiteralList[ttype];
            }

            if (ttype >= 0 && ttype < typeToTokenList.Count && typeToTokenList[ttype] != null)
            {
                return typeToTokenList[ttype];
            }

            return ttype.ToString();
        }

        /**
         * Gets the name by which a token can be referenced in the generated code.
         * For tokens defined in a {@code tokens{}} block or via a lexer rule, this
         * is the declared name of the token. For token types generated by the use
         * of a string literal within a parser rule of a combined grammar, this is
         * the automatically generated token type which includes the
         * {@link #AUTO_GENERATED_TOKEN_NAME_PREFIX} prefix. For types which are not
         * associated with a defined token, this method returns
         * {@link #INVALID_TOKEN_NAME}.
         *
         * @param ttype The token type.
         * @return The name of the token with the specified type.
         */
        [return: NotNull]
        public virtual string GetTokenName(int ttype)
        {
            // inside any target's char range and is lexer grammar?
            if (IsLexer() &&
                 ttype >= Lexer.MinCharValue && ttype <= Lexer.MaxCharValue)
            {
                return CharSupport.GetANTLRCharLiteralForChar(ttype);
            }

            if (ttype == TokenConstants.Eof)
            {
                return "EOF";
            }

            if (ttype >= 0 && ttype < typeToTokenList.Count && typeToTokenList[ttype] != null)
            {
                return typeToTokenList[ttype];
            }

            return INVALID_TOKEN_NAME;
        }

        /**
         * Gets the constant channel value for a user-defined channel.
         *
         * <p>
         * This method only returns channel values for user-defined channels. All
         * other channels, including the predefined channels
         * {@link Token#DEFAULT_CHANNEL} and {@link Token#HIDDEN_CHANNEL} along with
         * any channel defined in code (e.g. in a {@code @members{}} block), are
         * ignored.</p>
         *
         * @param channel The channel name.
         * @return The channel value, if {@code channel} is the name of a known
         * user-defined token channel; otherwise, -1.
         */
        public virtual int GetChannelValue(string channel)
        {
            int result;
            if (!channelNameToValueMap.TryGetValue(channel, out result))
                return -1;

            return result;
        }

        /**
         * Gets an array of rule names for rules defined or imported by the
         * grammar. The array index is the rule index, and the value is the name of
         * the rule with the corresponding {@link Rule#index}.
         *
         * <p>If no rule is defined with an index for an element of the resulting
         * array, the value of that element is {@link #INVALID_RULE_NAME}.</p>
         *
         * @return The names of all rules defined in the grammar.
         */
        public virtual string[] GetRuleNames()
        {
            string[] result = new string[rules.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = INVALID_RULE_NAME;

            foreach (Rule rule in rules.Values)
            {
                result[rule.index] = rule.name;
            }

            return result;
        }

        /**
         * Gets an array of token names for tokens defined or imported by the
         * grammar. The array index is the token type, and the value is the result
         * of {@link #getTokenName} for the corresponding token type.
         *
         * @see #getTokenName
         * @return The token names of all tokens defined in the grammar.
         */
        public virtual string[] GetTokenNames()
        {
            int numTokens = GetMaxTokenType();
            string[] tokenNames = new string[numTokens + 1];
            for (int i = 0; i < tokenNames.Length; i++)
            {
                tokenNames[i] = GetTokenName(i);
            }

            return tokenNames;
        }

        /**
         * Gets an array of display names for tokens defined or imported by the
         * grammar. The array index is the token type, and the value is the result
         * of {@link #getTokenDisplayName} for the corresponding token type.
         *
         * @see #getTokenDisplayName
         * @return The display names of all tokens defined in the grammar.
         */
        public virtual string[] GetTokenDisplayNames()
        {
            int numTokens = GetMaxTokenType();
            string[] tokenNames = new string[numTokens + 1];
            for (int i = 0; i < tokenNames.Length; i++)
            {
                tokenNames[i] = GetTokenDisplayName(i);
            }

            return tokenNames;
        }

        /**
         * Gets the literal names assigned to tokens in the grammar.
         */
        [return: NotNull]
        public virtual string[] GetTokenLiteralNames()
        {
            int numTokens = GetMaxTokenType();
            string[] literalNames = new string[numTokens + 1];
            for (int i = 0; i < Math.Min(literalNames.Length, typeToStringLiteralList.Count); i++)
            {
                literalNames[i] = typeToStringLiteralList[i];
            }

            foreach (KeyValuePair<string, int> entry in stringLiteralToTypeMap)
            {
                if (entry.Value >= 0 && entry.Value < literalNames.Length && literalNames[entry.Value] == null)
                {
                    literalNames[entry.Value] = entry.Key;
                }
            }

            return literalNames;
        }

        /**
         * Gets the symbolic names assigned to tokens in the grammar.
         */
        [return: NotNull]
        public virtual string[] GetTokenSymbolicNames()
        {
            int numTokens = GetMaxTokenType();
            string[] symbolicNames = new string[numTokens + 1];
            for (int i = 0; i < Math.Min(symbolicNames.Length, typeToTokenList.Count); i++)
            {
                if (typeToTokenList[i] == null || typeToTokenList[i].StartsWith(AUTO_GENERATED_TOKEN_NAME_PREFIX))
                {
                    continue;
                }

                symbolicNames[i] = typeToTokenList[i];
            }

            return symbolicNames;
        }

        /**
         * Gets a {@link Vocabulary} instance describing the vocabulary used by the
         * grammar.
         */
        [return: NotNull]
        public virtual IVocabulary GetVocabulary()
        {
            return new Vocabulary(GetTokenLiteralNames(), GetTokenSymbolicNames());
        }

        /** Given an arbitrarily complex SemanticContext, walk the "tree" and get display string.
         *  Pull predicates from grammar text.
         */
        public virtual string GetSemanticContextDisplayString(SemanticContext semctx)
        {
            if (semctx is SemanticContext.Predicate)
            {
                return GetPredicateDisplayString((SemanticContext.Predicate)semctx);
            }
            if (semctx is SemanticContext.AND)
            {
                SemanticContext.AND and = (SemanticContext.AND)semctx;
                return JoinPredicateOperands(and, " and ");
            }
            if (semctx is SemanticContext.OR)
            {
                SemanticContext.OR or = (SemanticContext.OR)semctx;
                return JoinPredicateOperands(or, " or ");
            }
            return semctx.ToString();
        }

        public virtual string JoinPredicateOperands(SemanticContext.Operator op, string separator)
        {
            StringBuilder buf = new StringBuilder();
            foreach (SemanticContext operand in op.Operands)
            {
                if (buf.Length > 0)
                {
                    buf.Append(separator);
                }

                buf.Append(GetSemanticContextDisplayString(operand));
            }

            return buf.ToString();
        }

        public virtual LinkedHashMap<int, PredAST> GetIndexToPredicateMap()
        {
            LinkedHashMap<int, PredAST> indexToPredMap = new LinkedHashMap<int, PredAST>();
            foreach (Rule r in rules.Values)
            {
                foreach (ActionAST a in r.actions)
                {
                    if (a is PredAST)
                    {
                        PredAST p = (PredAST)a;
                        indexToPredMap[sempreds[p]] = p;
                    }
                }
            }
            return indexToPredMap;
        }

        public virtual string GetPredicateDisplayString(SemanticContext.Predicate pred)
        {
            if (indexToPredMap == null)
            {
                indexToPredMap = GetIndexToPredicateMap();
            }
            ActionAST actionAST = indexToPredMap[pred.predIndex];
            return actionAST.Text;
        }

        /** What is the max char value possible for this grammar's target?  Use
         *  unicode max if no target defined.
         */
        public virtual int GetMaxCharValue()
        {
            return Antlr4.Runtime.Lexer.MaxCharValue;
            //		if ( generator!=null ) {
            //			return generator.target.getMaxCharValue(generator);
            //		}
            //		else {
            //			return Label.MAX_CHAR_VALUE;
            //		}
        }

        /** Return a set of all possible token or char types for this grammar */
        public virtual IIntSet GetTokenTypes()
        {
            if (IsLexer())
            {
                return GetAllCharValues();
            }
            return IntervalSet.Of(TokenConstants.MinUserTokenType, GetMaxTokenType());
        }

        /** Return min to max char as defined by the target.
         *  If no target, use max unicode char value.
         */
        public virtual IIntSet GetAllCharValues()
        {
            return IntervalSet.Of(Lexer.MinCharValue, GetMaxCharValue());
        }

        /** How many token types have been allocated so far? */
        public virtual int GetMaxTokenType()
        {
            return typeToTokenList.Count - 1; // don't count 0 (invalid)
        }

        /** Return a new unique integer in the token type space */
        public virtual int GetNewTokenType()
        {
            maxTokenType++;
            return maxTokenType;
        }

        /** Return a new unique integer in the channel value space. */
        public virtual int GetNewChannelNumber()
        {
            maxChannelType++;
            return maxChannelType;
        }

        public virtual void ImportTokensFromTokensFile()
        {
            string vocab = GetOptionString("tokenVocab");
            if (vocab != null)
            {
                TokenVocabParser vparser = new TokenVocabParser(this);
                IDictionary<string, int> tokens = vparser.Load();
                tool.Log("grammar", "tokens=" + tokens);
                foreach (string t in tokens.Keys)
                {
                    if (t[0] == '\'')
                        DefineStringLiteral(t, tokens[t]);
                    else
                        DefineTokenName(t, tokens[t]);
                }
            }
        }

        public virtual void ImportVocab(Grammar importG)
        {
            foreach (string tokenName in importG.tokenNameToTypeMap.Keys)
            {
                DefineTokenName(tokenName, importG.tokenNameToTypeMap[tokenName]);
            }
            foreach (string tokenName in importG.stringLiteralToTypeMap.Keys)
            {
                DefineStringLiteral(tokenName, importG.stringLiteralToTypeMap[tokenName]);
            }
            foreach (KeyValuePair<string, int> channel in importG.channelNameToValueMap)
            {
                DefineChannelName(channel.Key, channel.Value);
            }
            //		this.tokenNameToTypeMap.putAll( importG.tokenNameToTypeMap );
            //		this.stringLiteralToTypeMap.putAll( importG.stringLiteralToTypeMap );
            int max = Math.Max(this.typeToTokenList.Count, importG.typeToTokenList.Count);
            Utils.SetSize(typeToTokenList, max);
            for (int ttype = 0; ttype < importG.typeToTokenList.Count; ttype++)
            {
                maxTokenType = Math.Max(maxTokenType, ttype);
                this.typeToTokenList[ttype] = importG.typeToTokenList[ttype];
            }

            max = Math.Max(this.channelValueToNameList.Count, importG.channelValueToNameList.Count);
            Utils.SetSize(channelValueToNameList, max);
            for (int channelValue = 0; channelValue < importG.channelValueToNameList.Count; channelValue++)
            {
                maxChannelType = Math.Max(maxChannelType, channelValue);
                this.channelValueToNameList[channelValue] = importG.channelValueToNameList[channelValue];
            }
        }

        public virtual int DefineTokenName(string name)
        {
            int prev;
            if (!tokenNameToTypeMap.TryGetValue(name, out prev))
                return DefineTokenName(name, GetNewTokenType());

            return prev;
        }

        public virtual int DefineTokenName(string name, int ttype)
        {
            int prev;
            if (tokenNameToTypeMap.TryGetValue(name, out prev))
                return prev;

            tokenNameToTypeMap[name] = ttype;
            SetTokenForType(ttype, name);
            maxTokenType = Math.Max(maxTokenType, ttype);
            return ttype;
        }

        public virtual int DefineStringLiteral(string lit)
        {
            if (stringLiteralToTypeMap.ContainsKey(lit))
            {
                return stringLiteralToTypeMap[lit];
            }
            return DefineStringLiteral(lit, GetNewTokenType());

        }

        public virtual int DefineStringLiteral(string lit, int ttype)
        {
            if (!stringLiteralToTypeMap.ContainsKey(lit))
            {
                stringLiteralToTypeMap[lit] = ttype;
                // track in reverse index too
                if (ttype >= typeToStringLiteralList.Count)
                {
                    Utils.SetSize(typeToStringLiteralList, ttype + 1);
                }
                typeToStringLiteralList[ttype] = lit;

                SetTokenForType(ttype, lit);
                return ttype;
            }
            return TokenConstants.InvalidType;
        }

        public virtual int DefineTokenAlias(string name, string lit)
        {
            int ttype = DefineTokenName(name);
            stringLiteralToTypeMap[lit] = ttype;
            SetTokenForType(ttype, name);
            return ttype;
        }

        public virtual void SetTokenForType(int ttype, string text)
        {
            if (ttype == TokenConstants.Eof)
            {
                // ignore EOF, it will be reported as an error separately
                return;
            }

            if (ttype >= typeToTokenList.Count)
            {
                Utils.SetSize(typeToTokenList, ttype + 1);
            }
            string prevToken = typeToTokenList[ttype];
            if (prevToken == null || prevToken[0] == '\'')
            {
                // only record if nothing there before or if thing before was a literal
                typeToTokenList[ttype] = text;
            }
        }

        /**
         * Define a token channel with a specified name.
         *
         * <p>
         * If a channel with the specified name already exists, the previously
         * assigned channel value is returned.</p>
         *
         * @param name The channel name.
         * @return The constant channel value assigned to the channel.
         */
        public virtual int DefineChannelName(string name)
        {
            int prev;
            if (!channelNameToValueMap.TryGetValue(name, out prev))
            {
                return DefineChannelName(name, GetNewChannelNumber());
            }

            return prev;
        }

        /**
         * Define a token channel with a specified name.
         *
         * <p>
         * If a channel with the specified name already exists, the previously
         * assigned channel value is not altered.</p>
         *
         * @param name The channel name.
         * @return The constant channel value assigned to the channel.
         */
        public virtual int DefineChannelName(string name, int value)
        {
            int prev;
            if (channelNameToValueMap.TryGetValue(name, out prev))
            {
                return prev;
            }

            channelNameToValueMap[name] = value;
            SetChannelNameForValue(value, name);
            maxChannelType = Math.Max(maxChannelType, value);
            return value;
        }

        /**
         * Sets the channel name associated with a particular channel value.
         *
         * <p>
         * If a name has already been assigned to the channel with constant value
         * {@code channelValue}, this method does nothing.</p>
         *
         * @param channelValue The constant value for the channel.
         * @param name The channel name.
         */
        public virtual void SetChannelNameForValue(int channelValue, string name)
        {
            if (channelValue >= channelValueToNameList.Count)
            {
                Utils.SetSize(channelValueToNameList, channelValue + 1);
            }

            string prevChannel = channelValueToNameList[channelValue];
            if (prevChannel == null)
            {
                channelValueToNameList[channelValue] = name;
            }
        }

        // no isolated attr at grammar action level
        public virtual Attribute ResolveToAttribute(string x, ActionAST node)
        {
            return null;
        }

        // no $x.y makes sense here
        public virtual Attribute ResolveToAttribute(string x, string y, ActionAST node)
        {
            return null;
        }

        public virtual bool ResolvesToLabel(string x, ActionAST node)
        {
            return false;
        }

        public virtual bool ResolvesToListLabel(string x, ActionAST node)
        {
            return false;
        }

        public virtual bool ResolvesToToken(string x, ActionAST node)
        {
            return false;
        }

        public virtual bool ResolvesToAttributeDict(string x, ActionAST node)
        {
            return false;
        }

        /** Given a grammar type, what should be the default action scope?
         *  If I say @members in a COMBINED grammar, for example, the
         *  default scope should be "parser".
         */
        public virtual string GetDefaultActionScope()
        {
            switch (Type)
            {
            case ANTLRParser.LEXER:
                return "lexer";
            case ANTLRParser.PARSER:
            case ANTLRParser.COMBINED:
                return "parser";
            }
            return null;
        }

        public virtual int Type
        {
            get
            {
                if (ast != null)
                    return ast.grammarType;
                return 0;
            }
        }

        public virtual Antlr.Runtime.ITokenStream GetTokenStream()
        {
            if (ast != null)
                return ast.tokenStream;
            return null;
        }

        public virtual bool IsLexer()
        {
            return Type == ANTLRParser.LEXER;
        }
        public virtual bool IsParser()
        {
            return Type == ANTLRParser.PARSER;
        }
        public virtual bool IsCombined()
        {
            return Type == ANTLRParser.COMBINED;
        }

        /** Is id a valid token name? Does id start with an uppercase letter? */
        public static bool IsTokenName(string id)
        {
            return char.IsUpper(id[0]);
        }

        public virtual string GetTypeString()
        {
            if (ast == null)
                return null;
            return ANTLRParser.tokenNames[Type].ToLower();
        }

        public static string GetGrammarTypeToFileNameSuffix(int type)
        {
            switch (type)
            {
            case ANTLRParser.LEXER:
                return "Lexer";
            case ANTLRParser.PARSER:
                return "Parser";
            // if combined grammar, gen Parser and Lexer will be done later
            // TODO: we are separate now right?
            case ANTLRParser.COMBINED:
                return "Parser";
            default:
                return "<invalid>";
            }
        }

        public virtual string GetOptionString(string key)
        {
            return ast.GetOptionString(key);
        }

        /** Given ^(TOKEN_REF ^(OPTIONS ^(ELEMENT_OPTIONS (= assoc right))))
         *  set option assoc=right in TOKEN_REF.
         */
        public static void SetNodeOptions(GrammarAST node, GrammarAST options)
        {
            if (options == null)
                return;
            GrammarASTWithOptions t = (GrammarASTWithOptions)node;
            if (t.ChildCount == 0 || options.ChildCount == 0)
                return;
            foreach (object o in options.Children)
            {
                GrammarAST c = (GrammarAST)o;
                if (c.Type == ANTLRParser.ASSIGN)
                {
                    t.SetOption(c.GetChild(0).Text, (GrammarAST)c.GetChild(1));
                }
                else
                {
                    t.SetOption(c.Text, null); // no arg such as ID<VarNodeType>
                }
            }
        }

        /** Return list of (TOKEN_NAME node, 'literal' node) pairs */
        public static IList<System.Tuple<GrammarAST, GrammarAST>> GetStringLiteralAliasesFromLexerRules(GrammarRootAST ast)
        {
            string[] patterns = {
            "(RULE %name:TOKEN_REF (BLOCK (ALT %lit:STRING_LITERAL)))",
            "(RULE %name:TOKEN_REF (BLOCK (ALT %lit:STRING_LITERAL ACTION)))",
            "(RULE %name:TOKEN_REF (BLOCK (ALT %lit:STRING_LITERAL SEMPRED)))",
            "(RULE %name:TOKEN_REF (BLOCK (LEXER_ALT_ACTION (ALT %lit:STRING_LITERAL) .)))",
            "(RULE %name:TOKEN_REF (BLOCK (LEXER_ALT_ACTION (ALT %lit:STRING_LITERAL) . .)))",
            "(RULE %name:TOKEN_REF (BLOCK (LEXER_ALT_ACTION (ALT %lit:STRING_LITERAL) (LEXER_ACTION_CALL . .))))",
            "(RULE %name:TOKEN_REF (BLOCK (LEXER_ALT_ACTION (ALT %lit:STRING_LITERAL) . (LEXER_ACTION_CALL . .))))",
            "(RULE %name:TOKEN_REF (BLOCK (LEXER_ALT_ACTION (ALT %lit:STRING_LITERAL) (LEXER_ACTION_CALL . .) .)))",
			// TODO: allow doc comment in there
		};
            GrammarASTAdaptor adaptor = new GrammarASTAdaptor(ast.Token.InputStream);
            Antlr.Runtime.Tree.TreeWizard wiz = new Antlr.Runtime.Tree.TreeWizard(adaptor, ANTLRParser.tokenNames);
            IList<System.Tuple<GrammarAST, GrammarAST>> lexerRuleToStringLiteral =
                new List<System.Tuple<GrammarAST, GrammarAST>>();

            IList<GrammarAST> ruleNodes = ast.GetNodesWithType(ANTLRParser.RULE);
            if (ruleNodes == null || ruleNodes.Count == 0)
                return null;

            foreach (GrammarAST r in ruleNodes)
            {
                //tool.log("grammar", r.toStringTree());
                //			System.out.println("chk: "+r.toStringTree());
                Antlr.Runtime.Tree.ITree name = r.GetChild(0);
                if (name.Type == ANTLRParser.TOKEN_REF)
                {
                    // check rule against patterns
                    bool isLitRule;
                    foreach (string pattern in patterns)
                    {
                        isLitRule =
                            DefAlias(r, pattern, wiz, lexerRuleToStringLiteral);
                        if (isLitRule)
                            break;
                    }
                    //				if ( !isLitRule ) System.out.println("no pattern matched");
                }
            }
            return lexerRuleToStringLiteral;
        }

        protected static bool DefAlias(GrammarAST r, string pattern,
                                          Antlr.Runtime.Tree.TreeWizard wiz,
                                          IList<System.Tuple<GrammarAST, GrammarAST>> lexerRuleToStringLiteral)
        {
            Dictionary<string, object> nodes = new Dictionary<string, object>();
            if (wiz.Parse(r, pattern, nodes))
            {
                GrammarAST litNode = (GrammarAST)nodes["lit"];
                GrammarAST nameNode = (GrammarAST)nodes["name"];
                System.Tuple<GrammarAST, GrammarAST> pair = Tuple.Create(nameNode, litNode);
                lexerRuleToStringLiteral.Add(pair);
                return true;
            }
            return false;
        }

        public virtual ISet<string> GetStringLiterals()
        {
            StringCollector collector = new StringCollector(this);
            collector.VisitGrammar(ast);
            return collector.strings;
        }

        private class StringCollector : GrammarTreeVisitor
        {
            public readonly ISet<string> strings = new LinkedHashSet<string>();
            private readonly Grammar grammar;

            public StringCollector(Grammar grammar)
            {
                this.grammar = grammar;
            }

            public override void StringRef(TerminalAST @ref)
            {
                strings.Add(@ref.Text);
            }

            public override ErrorManager GetErrorManager()
            {
                return grammar.tool.errMgr;
            }
        }

        public virtual void SetLookaheadDFA(int decision, DFA lookaheadDFA)
        {
            decisionDFAs[decision] = lookaheadDFA;
        }

        public static IDictionary<int, Interval> GetStateToGrammarRegionMap(GrammarRootAST ast, IntervalSet grammarTokenTypes)
        {
            IDictionary<int, Interval> stateToGrammarRegionMap = new Dictionary<int, Interval>();
            if (ast == null)
                return stateToGrammarRegionMap;

            IList<GrammarAST> nodes = ast.GetNodesWithType(grammarTokenTypes);
            foreach (GrammarAST n in nodes)
            {
                if (n.atnState != null)
                {
                    Interval tokenRegion = Interval.Of(n.TokenStartIndex, n.TokenStopIndex);
                    Antlr.Runtime.Tree.ITree ruleNode = null;
                    // RULEs, BLOCKs of transformed recursive rules point to original token interval
                    switch (n.Type)
                    {
                    case ANTLRParser.RULE:
                        ruleNode = n;
                        break;
                    case ANTLRParser.BLOCK:
                    case ANTLRParser.CLOSURE:
                        ruleNode = n.GetAncestor(ANTLRParser.RULE);
                        break;
                    }
                    if (ruleNode is RuleAST)
                    {
                        string ruleName = ((RuleAST)ruleNode).GetRuleName();
                        Rule r = ast.g.GetRule(ruleName);
                        if (r is LeftRecursiveRule)
                        {
                            RuleAST originalAST = ((LeftRecursiveRule)r).GetOriginalAST();
                            tokenRegion = Interval.Of(originalAST.TokenStartIndex, originalAST.TokenStopIndex);
                        }
                    }
                    stateToGrammarRegionMap[n.atnState.stateNumber] = tokenRegion;
                }
            }
            return stateToGrammarRegionMap;
        }

        /** Given an ATN state number, return the token index range within the grammar from which that ATN state was derived. */
        public virtual Interval GetStateToGrammarRegion(int atnStateNumber)
        {
            if (stateToGrammarRegionMap == null)
            {
                stateToGrammarRegionMap = GetStateToGrammarRegionMap(ast, null); // map all nodes with non-null atn state ptr
            }
            if (stateToGrammarRegionMap == null)
                return Interval.Invalid;

            Interval result;
            if (!stateToGrammarRegionMap.TryGetValue(atnStateNumber, out result))
                result = Interval.Invalid;

            return result;
        }

        public virtual LexerInterpreter CreateLexerInterpreter(ICharStream input)
        {
            if (this.IsParser())
            {
                throw new InvalidOperationException("A lexer interpreter can only be created for a lexer or combined grammar.");
            }

            if (this.IsCombined())
            {
                return implicitLexer.CreateLexerInterpreter(input);
            }

            char[] serializedAtn = ATNSerializer.GetSerializedAsChars(atn, GetRuleNames());
            ATN deserialized = new ATNDeserializer().Deserialize(serializedAtn);
            return new LexerInterpreter(fileName, GetVocabulary(), GetRuleNames(), ((LexerGrammar)this).modes.Keys, deserialized, input);
        }

        /** @since 4.5.1 */
        public virtual GrammarParserInterpreter CreateGrammarParserInterpreter(ITokenStream tokenStream)
        {
            if (this.IsLexer())
            {
                throw new InvalidOperationException("A parser interpreter can only be created for a parser or combined grammar.");
            }

            char[] serializedAtn = ATNSerializer.GetSerializedAsChars(atn, GetRuleNames());
            ATN deserialized = new ATNDeserializer().Deserialize(serializedAtn);
            return new GrammarParserInterpreter(this, deserialized, tokenStream);
        }

        public virtual ParserInterpreter CreateParserInterpreter(ITokenStream tokenStream)
        {
            if (this.IsLexer())
            {
                throw new InvalidOperationException("A parser interpreter can only be created for a parser or combined grammar.");
            }

            char[] serializedAtn = ATNSerializer.GetSerializedAsChars(atn, GetRuleNames());
            ATN deserialized = new ATNDeserializer().Deserialize(serializedAtn);
            return new ParserInterpreter(fileName, GetVocabulary(), GetRuleNames(), deserialized, tokenStream);
        }

        protected class AltLabelVisitor : GrammarTreeVisitor
        {
            private readonly IDictionary<string, IList<System.Tuple<int, AltAST>>> labeledAlternatives =
                new Dictionary<string, IList<System.Tuple<int, AltAST>>>();
            private readonly IList<AltAST> unlabeledAlternatives =
                new List<AltAST>();

            public AltLabelVisitor(Antlr.Runtime.Tree.ITreeNodeStream input)
                : base(input)
            {
            }

            public virtual IDictionary<string, IList<System.Tuple<int, AltAST>>> GetLabeledAlternatives()
            {
                return labeledAlternatives;
            }

            public virtual IList<AltAST> GetUnlabeledAlternatives()
            {
                return unlabeledAlternatives;
            }

            public override void DiscoverOuterAlt(AltAST alt)
            {
                if (alt.altLabel != null)
                {
                    IList<System.Tuple<int, AltAST>> list;
                    if (!labeledAlternatives.TryGetValue(alt.altLabel.Text, out list) || list == null)
                    {
                        list = new List<System.Tuple<int, AltAST>>();
                        labeledAlternatives[alt.altLabel.Text] = list;
                    }

                    list.Add(Tuple.Create(currentOuterAltNumber, alt));
                }
                else
                {
                    unlabeledAlternatives.Add(alt);
                }
            }
        }
    }
}
