// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Automata
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Antlr4.Codegen;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Runtime.Atn;
    using Antlr4.StringTemplate;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using CommonToken = Antlr.Runtime.CommonToken;
    using Interval = Antlr4.Runtime.Misc.Interval;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;
    using IntStreamConstants = Antlr4.Runtime.IntStreamConstants;
    using IToken = Antlr.Runtime.IToken;
    using Lexer = Antlr4.Runtime.Lexer;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using TokenTypes = Antlr4.Runtime.TokenTypes;

    public class LexerATNFactory : ParserATNFactory
    {
        [Nullable]
        public TemplateGroup codegenTemplates;

        /**
         * Provides a map of names of predefined constants which are likely to
         * appear as the argument for lexer commands. These names would be resolved
         * by the Java compiler for lexer commands that are translated to embedded
         * actions, but are required during code generation for creating
         * {@link LexerAction} instances that are usable by a lexer interpreter.
         */
        public static readonly IDictionary<string, int> COMMON_CONSTANTS = new Dictionary<string, int>
        {
            { "HIDDEN", Lexer.Hidden },
            { "DEFAULT_TOKEN_CHANNEL", Lexer.DefaultTokenChannel },
            { "DEFAULT_MODE", Lexer.DefaultMode },
            { "SKIP", TokenTypes.Skip },
            { "MORE", TokenTypes.More },
            { "EOF", Lexer.Eof },
            { "MAX_CHAR_VALUE", Lexer.MaxCharValue },
            { "MIN_CHAR_VALUE", Lexer.MinCharValue },
        };

        private readonly IList<string> ruleCommands = new List<string>();

        /**
         * Maps from an action index to a {@link LexerAction} object.
         */
        protected IDictionary<int, ILexerAction> indexToActionMap = new Dictionary<int, ILexerAction>();
        /**
         * Maps from a {@link LexerAction} object to the action index.
         */
        protected IDictionary<ILexerAction, int> actionToIndexMap = new Dictionary<ILexerAction, int>();

        public LexerATNFactory(LexerGrammar g)
            : base(g)
        {
            // use codegen to get correct language templates for lexer commands
            string language = g.GetOptionString("language");
            CodeGenerator gen = new CodeGenerator(g.tool, null, language);
            AbstractTarget target = gen.GetTarget();
            codegenTemplates = target != null ? target.GetTemplates() : null;
        }

        public static ICollection<string> GetCommonConstants()
        {
            return COMMON_CONSTANTS.Keys;
        }

        public override ATN CreateATN()
        {
            // BUILD ALL START STATES (ONE PER MODE)
            ICollection<string> modes = ((LexerGrammar)g).modes.Keys;
            foreach (string modeName in modes)
            {
                // create s0, start state; implied Tokens rule node
                TokensStartState startState = NewState<TokensStartState>(null);
                atn.DefineMode(modeName, startState);
            }

            // INIT ACTION, RULE->TOKEN_TYPE MAP
            atn.ruleToTokenType = new int[g.rules.Count];
            foreach (Rule r in g.rules.Values)
            {
                atn.ruleToTokenType[r.index] = g.GetTokenType(r.name);
            }

            // CREATE ATN FOR EACH RULE
            _CreateATN(g.rules.Values);

            atn.lexerActions = new ILexerAction[indexToActionMap.Count];
            foreach (KeyValuePair<int, ILexerAction> entry in indexToActionMap)
            {
                atn.lexerActions[entry.Key] = entry.Value;
            }

            // LINK MODE START STATE TO EACH TOKEN RULE
            foreach (string modeName in modes)
            {
                IList<Rule> rules = ((LexerGrammar)g).modes[modeName];
                TokensStartState startState = atn.modeNameToStartState[modeName];
                foreach (Rule r in rules)
                {
                    if (!r.IsFragment())
                    {
                        RuleStartState s = atn.ruleToStartState[r.index];
                        Epsilon(startState, s);
                    }
                }
            }

            ATNOptimizer.Optimize(g, atn);
            return atn;
        }

        public override Handle Rule(GrammarAST ruleAST, string name, Handle blk)
        {
            ruleCommands.Clear();
            return base.Rule(ruleAST, name, blk);
        }

        public override Handle Action(ActionAST action)
        {
            int ruleIndex = currentRule.index;
            int actionIndex = g.lexerActions[action];
            LexerCustomAction lexerAction = new LexerCustomAction(ruleIndex, actionIndex);
            return Action(action, lexerAction);
        }

        protected virtual int GetLexerActionIndex(ILexerAction lexerAction)
        {
            int lexerActionIndex;
            if (!actionToIndexMap.TryGetValue(lexerAction, out lexerActionIndex))
            {
                lexerActionIndex = actionToIndexMap.Count;
                actionToIndexMap[lexerAction] = lexerActionIndex;
                indexToActionMap[lexerActionIndex] = lexerAction;
            }

            return lexerActionIndex;
        }

        public override Handle Action(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                ATNState left = NewState(null);
                ATNState right = NewState(null);
                Epsilon(left, right);
                return new Handle(left, right);
            }

            // define action AST for this rule as if we had found in grammar
            ActionAST ast = new ActionAST(new CommonToken(ANTLRParser.ACTION, action));
            currentRule.DefineActionInAlt(currentOuterAlt, ast);
            return Action(ast);
        }

        protected virtual Handle Action(GrammarAST node, ILexerAction lexerAction)
        {
            ATNState left = NewState(node);
            ATNState right = NewState(node);
            bool isCtxDependent = false;
            int lexerActionIndex = GetLexerActionIndex(lexerAction);
            ActionTransition a =
                new ActionTransition(right, currentRule.index, lexerActionIndex, isCtxDependent);
            left.AddTransition(a);
            node.atnState = left;
            Handle h = new Handle(left, right);
            return h;
        }

        public override Handle LexerAltCommands(Handle alt, Handle cmds)
        {
            Handle h = new Handle(alt.left, cmds.right);
            Epsilon(alt.right, cmds.left);
            return h;
        }

        public override Handle LexerCallCommand(GrammarAST ID, GrammarAST arg)
        {
            ILexerAction lexerAction = CreateLexerAction(ID, arg);
            if (lexerAction != null)
            {
                return Action(ID, lexerAction);
            }

            if (codegenTemplates == null)
            {
                // suppress reporting a single missing template when the target couldn't be loaded
                return Epsilon(ID);
            }

            // fall back to standard action generation for the command
            Template cmdST = codegenTemplates.GetInstanceOf("Lexer" +
                                                      CharSupport.Capitalize(ID.Text) +
                                                      "Command");
            if (cmdST == null)
            {
                g.tool.errMgr.GrammarError(ErrorType.INVALID_LEXER_COMMAND, g.fileName, ID.Token, ID.Text);
                return Epsilon(ID);
            }

            if (cmdST.impl.FormalArguments == null || !cmdST.impl.FormalArguments.Any(x => x.Name == "arg"))
            {
                g.tool.errMgr.GrammarError(ErrorType.UNWANTED_LEXER_COMMAND_ARGUMENT, g.fileName, ID.Token, ID.Text);
                return Epsilon(ID);
            }

            cmdST.Add("arg", arg.Text);
            cmdST.Add("grammar", arg.g);
            return Action(cmdST.Render());
        }

        public override Handle LexerCommand(GrammarAST ID)
        {
            ILexerAction lexerAction = CreateLexerAction(ID, null);
            if (lexerAction != null)
            {
                return Action(ID, lexerAction);
            }

            if (codegenTemplates == null)
            {
                // suppress reporting a single missing template when the target couldn't be loaded
                return Epsilon(ID);
            }

            // fall back to standard action generation for the command
            Template cmdST = codegenTemplates.GetInstanceOf("Lexer" +
                    CharSupport.Capitalize(ID.Text) +
                    "Command");
            if (cmdST == null)
            {
                g.tool.errMgr.GrammarError(ErrorType.INVALID_LEXER_COMMAND, g.fileName, ID.Token, ID.Text);
                return Epsilon(ID);
            }

            if (cmdST.impl.FormalArguments != null && cmdST.impl.FormalArguments.Any(x => x.Name == "arg"))
            {
                g.tool.errMgr.GrammarError(ErrorType.MISSING_LEXER_COMMAND_ARGUMENT, g.fileName, ID.Token, ID.Text);
                return Epsilon(ID);
            }

            return Action(cmdST.Render());
        }

        public override Handle Range(GrammarAST a, GrammarAST b)
        {
            ATNState left = NewState(a);
            ATNState right = NewState(b);
            int t1 = CharSupport.GetCharValueFromGrammarCharLiteral(a.Text);
            int t2 = CharSupport.GetCharValueFromGrammarCharLiteral(b.Text);
            CheckRange(a, b, t1, t2);
            left.AddTransition(new RangeTransition(right, t1, t2));
            a.atnState = left;
            b.atnState = left;
            return new Handle(left, right);
        }

        public override Handle Set(GrammarAST associatedAST, IList<GrammarAST> alts, bool invert)
        {
            ATNState left = NewState(associatedAST);
            ATNState right = NewState(associatedAST);
            IntervalSet set = new IntervalSet();
            foreach (GrammarAST t in alts)
            {
                if (t.Type == ANTLRParser.RANGE)
                {
                    int a = CharSupport.GetCharValueFromGrammarCharLiteral(t.GetChild(0).Text);
                    int b = CharSupport.GetCharValueFromGrammarCharLiteral(t.GetChild(1).Text);
                    if (CheckRange((GrammarAST)t.GetChild(0), (GrammarAST)t.GetChild(1), a, b))
                    {
                        CheckSetCollision(associatedAST, set, a, b);
                        set.Add(a, b);
                    }
                }
                else if (t.Type == ANTLRParser.LEXER_CHAR_SET)
                {
                    set.AddAll(GetSetFromCharSetLiteral(t));
                }
                else if (t.Type == ANTLRParser.STRING_LITERAL)
                {
                    int c = CharSupport.GetCharValueFromGrammarCharLiteral(t.Text);
                    if (c != -1)
                    {
                        CheckSetCollision(associatedAST, set, c);
                        set.Add(c);
                    }
                    else
                    {
                        g.tool.errMgr.GrammarError(ErrorType.INVALID_LITERAL_IN_LEXER_SET,
                                                   g.fileName, t.Token, t.Text);
                    }
                }
                else if (t.Type == ANTLRParser.TOKEN_REF)
                {
                    g.tool.errMgr.GrammarError(ErrorType.UNSUPPORTED_REFERENCE_IN_LEXER_SET,
                                               g.fileName, t.Token, t.Text);
                }
            }
            if (invert)
            {
                left.AddTransition(new NotSetTransition(right, set));
            }
            else
            {
                Transition transition;
                if (set.GetIntervals().Count == 1)
                {
                    Interval interval = set.GetIntervals()[0];
                    transition = new RangeTransition(right, interval.a, interval.b);
                }
                else
                {
                    transition = new SetTransition(right, set);
                }

                left.AddTransition(transition);
            }
            associatedAST.atnState = left;
            return new Handle(left, right);
        }

        protected virtual bool CheckRange(GrammarAST leftNode, GrammarAST rightNode, int leftValue, int rightValue)
        {
            bool result = true;
            if (leftValue == -1)
            {
                result = false;
                g.tool.errMgr.GrammarError(ErrorType.INVALID_LITERAL_IN_LEXER_SET,
                        g.fileName, leftNode.Token, leftNode.Text);
            }
            if (rightValue == -1)
            {
                result = false;
                g.tool.errMgr.GrammarError(ErrorType.INVALID_LITERAL_IN_LEXER_SET,
                        g.fileName, rightNode.Token, rightNode.Text);
            }
            if (!result)
                return result;

            if (rightValue < leftValue)
            {
                g.tool.errMgr.GrammarError(ErrorType.EMPTY_STRINGS_AND_SETS_NOT_ALLOWED,
                        g.fileName, ((GrammarAST)leftNode.Parent).Token, leftNode.Text + ".." + rightNode.Text);
            }
            return result;
        }

        /** For a lexer, a string is a sequence of char to match.  That is,
         *  "fog" is treated as 'f' 'o' 'g' not as a single transition in
         *  the DFA.  Machine== o-'f'-&gt;o-'o'-&gt;o-'g'-&gt;o and has n+1 states
         *  for n characters.
         */
        public override Handle StringLiteral(TerminalAST stringLiteralAST)
        {
            string chars = stringLiteralAST.Text;
            ATNState left = NewState(stringLiteralAST);
            ATNState right;
            chars = CharSupport.GetStringFromGrammarStringLiteral(chars);
            if (chars == null)
            {
                g.tool.errMgr.GrammarError(ErrorType.INVALID_ESCAPE_SEQUENCE,
                        g.fileName, stringLiteralAST.Token);
                return new Handle(left, left);
            }

            int n = chars.Length;
            ATNState prev = left;
            right = null;
            for (int i = 0; i < n; i++)
            {
                right = NewState(stringLiteralAST);
                prev.AddTransition(new AtomTransition(right, chars[i]));
                prev = right;
            }
            stringLiteralAST.atnState = left;
            return new Handle(left, right);
        }

        /** [Aa\t \u1234a-z\]\-] char sets */
        public override Handle CharSetLiteral(GrammarAST charSetAST)
        {
            ATNState left = NewState(charSetAST);
            ATNState right = NewState(charSetAST);
            IntervalSet set = GetSetFromCharSetLiteral(charSetAST);
            left.AddTransition(new SetTransition(right, set));
            charSetAST.atnState = left;
            return new Handle(left, right);
        }

        public virtual IntervalSet GetSetFromCharSetLiteral(GrammarAST charSetAST)
        {
            string chars = charSetAST.Text;
            chars = chars.Substring(1, chars.Length - 2);
            string cset = '"' + chars + '"';
            IntervalSet set = new IntervalSet();

            if (chars.Length == 0)
            {
                g.tool.errMgr.GrammarError(ErrorType.EMPTY_STRINGS_AND_SETS_NOT_ALLOWED,
                        g.fileName, charSetAST.Token, "[]");
                return set;
            }

            // unescape all valid escape char like \n, leaving escaped dashes as '\-'
            // so we can avoid seeing them as '-' range ops.
            chars = CharSupport.GetStringFromGrammarStringLiteral(cset);
            if (chars == null)
            {
                g.tool.errMgr.GrammarError(ErrorType.INVALID_ESCAPE_SEQUENCE,
                                           g.fileName, charSetAST.Token);
                return set;
            }
            int n = chars.Length;
            // now make x-y become set of char
            for (int i = 0; i < n; i++)
            {
                int c = chars[i];
                if (c == '\\' && (i + 1) < n && chars[i + 1] == '-')
                { // \-
                    CheckSetCollision(charSetAST, set, '-');
                    set.Add('-');
                    i++;
                }
                else if ((i + 2) < n && chars[i + 1] == '-')
                { // range x-y
                    int x = c;
                    int y = chars[i + 2];
                    if (x <= y)
                    {
                        CheckSetCollision(charSetAST, set, x, y);
                        set.Add(x, y);
                    }
                    else
                    {
                        g.tool.errMgr.GrammarError(ErrorType.EMPTY_STRINGS_AND_SETS_NOT_ALLOWED,
                                                   g.fileName, charSetAST.Token, "[" + (char)x + "-" + (char)y + "]");
                    }
                    i += 2;
                }
                else
                {
                    CheckSetCollision(charSetAST, set, c);
                    set.Add(c);
                }
            }
            return set;
        }

        protected virtual void CheckSetCollision(GrammarAST ast, IntervalSet set, int el)
        {
            if (set.Contains(el))
            {
                g.tool.errMgr.GrammarError(ErrorType.CHARACTERS_COLLISION_IN_SET, g.fileName, ast.Token,
                        (char)el, ast.Text);
            }
        }

        protected virtual void CheckSetCollision(GrammarAST ast, IntervalSet set, int a, int b)
        {
            for (int i = a; i <= b; i++)
            {
                if (set.Contains(i))
                {
                    string setText;
                    if (ast.Children == null)
                    {
                        setText = ast.Text;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (object child in ast.Children)
                        {
                            if (child is RangeAST)
                            {
                                sb.Append(((RangeAST)child).GetChild(0).Text);
                                sb.Append("..");
                                sb.Append(((RangeAST)child).GetChild(1).Text);
                            }
                            else
                            {
                                sb.Append(((GrammarAST)child).Text);
                            }
                            sb.Append(" | ");
                        }
                        sb.Remove(sb.Length - 3, 3);
                        setText = sb.ToString();
                    }

                    g.tool.errMgr.GrammarError(ErrorType.CHARACTERS_COLLISION_IN_SET, g.fileName, ast.Token,
                                        (char)a + "-" + (char)b, setText);
                    break;
                }
            }
        }

        public override Handle TokenRef(TerminalAST node)
        {
            // Ref to EOF in lexer yields char transition on -1
            if (node.Text.Equals("EOF"))
            {
                ATNState left = NewState(node);
                ATNState right = NewState(node);
                left.AddTransition(new AtomTransition(right, IntStreamConstants.Eof));
                return new Handle(left, right);
            }
            return _RuleRef(node);
        }

        [return: Nullable]
        private ILexerAction CreateLexerAction([NotNull] GrammarAST ID, [Nullable] GrammarAST arg)
        {
            string command = ID.Text;
            CheckCommands(command, ID.Token);

            if ("skip".Equals(command) && arg == null)
            {
                return LexerSkipAction.Instance;
            }
            else if ("more".Equals(command) && arg == null)
            {
                return LexerMoreAction.Instance;
            }
            else if ("popMode".Equals(command) && arg == null)
            {
                return LexerPopModeAction.Instance;
            }
            else if ("mode".Equals(command) && arg != null)
            {
                string modeName = arg.Text;
                int? mode = GetModeConstantValue(modeName, arg.Token);
                if (mode == null)
                {
                    return null;
                }

                return new LexerModeAction(mode.Value);
            }
            else if ("pushMode".Equals(command) && arg != null)
            {
                string modeName = arg.Text;
                int? mode = GetModeConstantValue(modeName, arg.Token);
                if (mode == null)
                {
                    return null;
                }

                return new LexerPushModeAction(mode.Value);
            }
            else if ("type".Equals(command) && arg != null)
            {
                string typeName = arg.Text;
                int? type = GetTokenConstantValue(typeName, arg.Token);
                if (type == null)
                {
                    return null;
                }

                return new LexerTypeAction(type.Value);
            }
            else if ("channel".Equals(command) && arg != null)
            {
                string channelName = arg.Text;
                int? channel = GetChannelConstantValue(channelName, arg.Token);
                if (channel == null)
                {
                    return null;
                }

                return new LexerChannelAction(channel.Value);
            }
            else
            {
                return null;
            }
        }

        private void CheckCommands(string command, IToken commandToken)
        {
            // Command combinations list: https://github.com/antlr/antlr4/issues/1388#issuecomment-263344701
            if (!command.Equals("pushMode") && !command.Equals("popMode"))
            {
                if (ruleCommands.Contains(command))
                {
                    g.tool.errMgr.GrammarError(ErrorType.DUPLICATED_COMMAND, g.fileName, commandToken, command);
                }

                if (!ruleCommands.Equals("mode"))
                {
                    string firstCommand = null;

                    if (command.Equals("skip"))
                    {
                        if (ruleCommands.Contains("more"))
                        {
                            firstCommand = "more";
                        }
                        else if (ruleCommands.Contains("type"))
                        {
                            firstCommand = "type";
                        }
                        else if (ruleCommands.Contains("channel"))
                        {
                            firstCommand = "channel";
                        }
                    }
                    else if (command.Equals("more"))
                    {
                        if (ruleCommands.Contains("skip"))
                        {
                            firstCommand = "skip";
                        }
                        else if (ruleCommands.Contains("type"))
                        {
                            firstCommand = "type";
                        }
                        else if (ruleCommands.Contains("channel"))
                        {
                            firstCommand = "channel";
                        }
                    }
                    else if (command.Equals("type") || command.Equals("channel"))
                    {
                        if (ruleCommands.Contains("more"))
                        {
                            firstCommand = "more";
                        }
                        else if (ruleCommands.Contains("skip"))
                        {
                            firstCommand = "skip";
                        }
                    }

                    if (firstCommand != null)
                    {
                        g.tool.errMgr.GrammarError(ErrorType.INCOMPATIBLE_COMMANDS, g.fileName, commandToken, firstCommand, command);
                    }
                }
            }

            ruleCommands.Add(command);
        }

        [return: Nullable]
        private int? GetModeConstantValue([Nullable] string modeName, [Nullable] IToken token)
        {
            if (modeName == null)
            {
                return null;
            }

            if (modeName.Equals("DEFAULT_MODE"))
            {
                return Lexer.DefaultMode;
            }
            if (COMMON_CONSTANTS.ContainsKey(modeName))
            {
                g.tool.errMgr.GrammarError(ErrorType.MODE_CONFLICTS_WITH_COMMON_CONSTANTS, g.fileName, token, token.Text);
                return null;
            }

            IList<string> modeNames = new List<string>(((LexerGrammar)g).modes.Keys);
            int mode = modeNames.IndexOf(modeName);
            if (mode >= 0)
            {
                return mode;
            }

            int result;
            if (int.TryParse(modeName, out result))
                return result;

            g.tool.errMgr.GrammarError(ErrorType.CONSTANT_VALUE_IS_NOT_A_RECOGNIZED_MODE_NAME, g.fileName, token, token.Text);
            return null;
        }

        [return: Nullable]
        private int? GetTokenConstantValue([Nullable] string tokenName, [Nullable] IToken token)
        {
            if (tokenName == null)
            {
                return null;
            }

            if (tokenName.Equals("EOF"))
            {
                return Lexer.Eof;
            }
            if (COMMON_CONSTANTS.ContainsKey(tokenName))
            {
                g.tool.errMgr.GrammarError(ErrorType.TOKEN_CONFLICTS_WITH_COMMON_CONSTANTS, g.fileName, token, token.Text);
                return null;
            }

            int tokenType = g.GetTokenType(tokenName);
            if (tokenType != Antlr4.Runtime.TokenConstants.InvalidType)
            {
                return tokenType;
            }

            int result;
            if (int.TryParse(tokenName, out result))
                return result;

            g.tool.errMgr.GrammarError(ErrorType.CONSTANT_VALUE_IS_NOT_A_RECOGNIZED_TOKEN_NAME, g.fileName, token, token.Text);
            return null;
        }

        [return: Nullable]
        private int? GetChannelConstantValue([Nullable] string channelName, [Nullable] IToken token)
        {
            if (channelName == null)
            {
                return null;
            }

            if (channelName.Equals("HIDDEN"))
            {
                return Lexer.Hidden;
            }
            if (channelName.Equals("DEFAULT_TOKEN_CHANNEL"))
            {
                return Lexer.DefaultTokenChannel;
            }
            if (COMMON_CONSTANTS.ContainsKey(channelName))
            {
                g.tool.errMgr.GrammarError(ErrorType.CHANNEL_CONFLICTS_WITH_COMMON_CONSTANTS, g.fileName, token, token.Text);
                return null;
            }

            int channelValue = g.GetChannelValue(channelName);
            if (channelValue >= Antlr4.Runtime.TokenConstants.MinUserChannelValue)
            {
                return channelValue;
            }

            int result;
            if (int.TryParse(channelName, out result))
                return result;

            g.tool.errMgr.GrammarError(ErrorType.CONSTANT_VALUE_IS_NOT_A_RECOGNIZED_CHANNEL_NAME, g.fileName, token, token.Text);
            return null;
        }
    }
}
