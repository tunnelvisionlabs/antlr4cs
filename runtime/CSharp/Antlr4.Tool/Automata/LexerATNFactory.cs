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

namespace Antlr4.Automata
{
    using System.Collections.Generic;
    using System.Linq;
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
                    set.Add(a, b);
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

        /** For a lexer, a string is a sequence of char to match.  That is,
         *  "fog" is treated as 'f' 'o' 'g' not as a single transition in
         *  the DFA.  Machine== o-'f'-&gt;o-'o'-&gt;o-'g'-&gt;o and has n+1 states
         *  for n characters.
         */
        public override Handle StringLiteral(TerminalAST stringLiteralAST)
        {
            string chars = stringLiteralAST.Text;
            chars = CharSupport.GetStringFromGrammarStringLiteral(chars);
            int n = chars.Length;
            ATNState left = NewState(stringLiteralAST);
            ATNState prev = left;
            ATNState right = null;
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

            // unescape all valid escape char like \n, leaving escaped dashes as '\-'
            // so we can avoid seeing them as '-' range ops.
            chars = CharSupport.GetStringFromGrammarStringLiteral(cset);
            // now make x-y become set of char
            int n = chars.Length;
            for (int i = 0; i < n; i++)
            {
                int c = chars[i];
                if (c == '\\' && (i + 1) < n && chars[i + 1] == '-')
                { // \-
                    set.Add('-');
                    i++;
                }
                else if ((i + 2) < n && chars[i + 1] == '-')
                { // range x-y
                    int x = c;
                    int y = chars[i + 2];
                    if (x <= y)
                        set.Add(x, y);
                    i += 2;
                }
                else
                {
                    set.Add(c);
                }
            }
            return set;
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
        protected virtual ILexerAction CreateLexerAction([NotNull] GrammarAST ID, [Nullable] GrammarAST arg)
        {
            string command = ID.Text;
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
                CheckMode(modeName, arg.Token);
                int? mode = GetConstantValue(modeName, arg.Token);
                if (mode == null)
                {
                    return null;
                }

                return new LexerModeAction(mode.Value);
            }
            else if ("pushMode".Equals(command) && arg != null)
            {
                string modeName = arg.Text;
                CheckMode(modeName, arg.Token);
                int? mode = GetConstantValue(modeName, arg.Token);
                if (mode == null)
                {
                    return null;
                }

                return new LexerPushModeAction(mode.Value);
            }
            else if ("type".Equals(command) && arg != null)
            {
                string typeName = arg.Text;
                CheckToken(typeName, arg.Token);
                int? type = GetConstantValue(typeName, arg.Token);
                if (type == null)
                {
                    return null;
                }

                return new LexerTypeAction(type.Value);
            }
            else if ("channel".Equals(command) && arg != null)
            {
                string channelName = arg.Text;
                CheckChannel(channelName, arg.Token);
                int? channel = GetConstantValue(channelName, arg.Token);
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

        protected virtual void CheckMode(string modeName, IToken token)
        {
            if (!modeName.Equals("DEFAULT_MODE") && COMMON_CONSTANTS.ContainsKey(modeName))
            {
                g.tool.errMgr.GrammarError(ErrorType.MODE_CONFLICTS_WITH_COMMON_CONSTANTS, g.fileName, token, token.Text);
            }
        }

        protected virtual void CheckToken(string tokenName, IToken token)
        {
            if (!tokenName.Equals("EOF") && COMMON_CONSTANTS.ContainsKey(tokenName))
            {
                g.tool.errMgr.GrammarError(ErrorType.TOKEN_CONFLICTS_WITH_COMMON_CONSTANTS, g.fileName, token, token.Text);
            }
        }

        protected virtual void CheckChannel(string channelName, IToken token)
        {
            if (!channelName.Equals("HIDDEN") && !channelName.Equals("DEFAULT_TOKEN_CHANNEL") && COMMON_CONSTANTS.ContainsKey(channelName))
            {
                g.tool.errMgr.GrammarError(ErrorType.CHANNEL_CONFLICTS_WITH_COMMON_CONSTANTS, g.fileName, token, token.Text);
            }
        }

        [return: Nullable]
        protected virtual int? GetConstantValue([Nullable] string name, [Nullable] IToken token)
        {
            if (name == null)
            {
                return null;
            }

            int commonConstant;
            if (COMMON_CONSTANTS.TryGetValue(name, out commonConstant))
            {
                return commonConstant;
            }

            int tokenType = g.GetTokenType(name);
            if (tokenType != Antlr4.Runtime.TokenConstants.InvalidType)
            {
                return tokenType;
            }

            int channelValue = g.GetChannelValue(name);
            if (channelValue >= Antlr4.Runtime.TokenConstants.MinUserChannelValue)
            {
                return channelValue;
            }

            IList<string> modeNames = new List<string>(((LexerGrammar)g).modes.Keys);
            int mode = modeNames.IndexOf(name);
            if (mode >= 0)
            {
                return mode;
            }

            int result;
            if (!int.TryParse(name, out result))
            {
                g.tool.errMgr.GrammarError(ErrorType.UNKNOWN_LEXER_CONSTANT, g.fileName, token, currentRule.name, token != null ? token.Text : null);
                return null;
            }

            return result;
        }
    }
}