/*
 * [The "BSD license"]
 *  Copyright (c) 2013 Terence Parr
 *  Copyright (c) 2013 Sam Harwell
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
using System;
using System.Collections;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>This is all the parsing support code essentially; most of it is error recovery stuff.
    ///     </summary>
    /// <remarks>This is all the parsing support code essentially; most of it is error recovery stuff.
    ///     </remarks>
    public abstract class Parser : Recognizer<IToken, ParserATNSimulator>
    {
#if !PORTABLE
        public class TraceListener : IParseTreeListener
        {
            public virtual void EnterEveryRule(ParserRuleContext ctx)
            {
                System.Console.Out.WriteLine("enter   " + this._enclosing.RuleNames[ctx.GetRuleIndex
                    ()] + ", LT(1)=" + this._enclosing._input.Lt(1).Text);
            }

            public virtual void ExitEveryRule(ParserRuleContext ctx)
            {
                System.Console.Out.WriteLine("exit    " + this._enclosing.RuleNames[ctx.GetRuleIndex
                    ()] + ", LT(1)=" + this._enclosing._input.Lt(1).Text);
            }

            public virtual void VisitErrorNode(IErrorNode node)
            {
            }

            public virtual void VisitTerminal(ITerminalNode node)
            {
                ParserRuleContext parent = (ParserRuleContext)((IRuleNode)node.Parent).RuleContext;
                IToken token = node.Symbol;
                System.Console.Out.WriteLine("consume " + token + " rule " + this._enclosing.RuleNames
                    [parent.GetRuleIndex()]);
            }

            internal TraceListener(Parser _enclosing)
            {
                this._enclosing = _enclosing;
            }

            private readonly Parser _enclosing;
        }
#endif

        public class TrimToSizeListener : IParseTreeListener
        {
            public static readonly Parser.TrimToSizeListener Instance = new Parser.TrimToSizeListener
                ();

            public virtual void VisitTerminal(ITerminalNode node)
            {
            }

            public virtual void VisitErrorNode(IErrorNode node)
            {
            }

            public virtual void EnterEveryRule(ParserRuleContext ctx)
            {
            }

            public virtual void ExitEveryRule(ParserRuleContext ctx)
            {
                if (ctx.children is List<IParseTree>)
                {
                    ((List<IParseTree>)ctx.children).TrimExcess();
                }
            }
        }

        /// <summary>The error handling strategy for the parser.</summary>
        /// <remarks>
        /// The error handling strategy for the parser. The default value is a new
        /// instance of
        /// <see cref="DefaultErrorStrategy">DefaultErrorStrategy</see>
        /// .
        /// </remarks>
        /// <seealso cref="ErrorHandler"/>
        [NotNull]
        protected internal IAntlrErrorStrategy _errHandler = new DefaultErrorStrategy();

        /// <summary>The input stream.</summary>
        /// <remarks>The input stream.</remarks>
        /// <seealso cref="InputStream()">InputStream()</seealso>
        /// <seealso cref="SetInputStream(ITokenStream)">SetInputStream(ITokenStream)</seealso>
        protected internal ITokenStream _input;

        protected internal readonly List<int> _precedenceStack = new List<int> { 0 };

        /// <summary>
        /// The
        /// <see cref="ParserRuleContext">ParserRuleContext</see>
        /// object for the currently executing rule.
        /// This is always non-null during the parsing process.
        /// </summary>
        protected internal ParserRuleContext _ctx;

        /// <summary>
        /// Specifies whether or not the parser should construct a parse tree during
        /// the parsing process.
        /// </summary>
        /// <remarks>
        /// Specifies whether or not the parser should construct a parse tree during
        /// the parsing process. The default value is
        /// <code>true</code>
        /// .
        /// </remarks>
        /// <seealso cref="BuildParseTree"/>
        protected internal bool _buildParseTrees = true;

#if !PORTABLE
        /// <summary>
        /// When
        /// <see cref="Trace"/>
        /// <code>(true)</code>
        /// is called, a reference to the
        /// <see cref="TraceListener">TraceListener</see>
        /// is stored here so it can be easily removed in a
        /// later call to
        /// <see cref="Trace"/>
        /// <code>(false)</code>
        /// . The listener itself is
        /// implemented as a parser listener so this field is not directly used by
        /// other parser methods.
        /// </summary>
        private Parser.TraceListener _tracer;
#endif

        /// <summary>
        /// The list of
        /// <see cref="Antlr4.Runtime.Tree.IParseTreeListener">Antlr4.Runtime.Tree.IParseTreeListener
        ///     </see>
        /// listeners registered to receive
        /// events during the parse.
        /// </summary>
        /// <seealso cref="AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)">AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)
        ///     </seealso>
        [Nullable]
        protected internal IList<IParseTreeListener> _parseListeners;

        /// <summary>The number of syntax errors reported during parsing.</summary>
        /// <remarks>
        /// The number of syntax errors reported during parsing. This value is
        /// incremented each time
        /// <see cref="NotifyErrorListeners(string)">NotifyErrorListeners(string)</see>
        /// is called.
        /// </remarks>
        protected internal int _syntaxErrors;

        public Parser(ITokenStream input)
        {
            SetInputStream(input);
        }

        /// <summary>reset the parser's state</summary>
        public virtual void Reset()
        {
            if (((ITokenStream)InputStream) != null)
            {
                ((ITokenStream)InputStream).Seek(0);
            }
            _errHandler.Reset(this);
            _ctx = null;
            _syntaxErrors = 0;
#if !PORTABLE
            Trace = false;
#endif
            _precedenceStack.Clear();
            _precedenceStack.Add(0);
            ATNSimulator interpreter = Interpreter;
            if (interpreter != null)
            {
                interpreter.Reset();
            }
        }

        /// <summary>
        /// Match current input symbol against
        /// <code>ttype</code>
        /// . If the symbol type
        /// matches,
        /// <see cref="IAntlrErrorStrategy.ReportMatch(Parser)">IAntlrErrorStrategy.ReportMatch(Parser)
        ///     </see>
        /// and
        /// <see cref="Consume()">Consume()</see>
        /// are
        /// called to complete the match process.
        /// <p/>
        /// If the symbol type does not match,
        /// <see cref="IAntlrErrorStrategy.RecoverInline(Parser)">IAntlrErrorStrategy.RecoverInline(Parser)
        ///     </see>
        /// is called on the current error
        /// strategy to attempt recovery. If
        /// <see cref="BuildParseTree()">BuildParseTree()</see>
        /// is
        /// <code>true</code>
        /// and the token index of the symbol returned by
        /// <see cref="IAntlrErrorStrategy.RecoverInline(Parser)">IAntlrErrorStrategy.RecoverInline(Parser)
        ///     </see>
        /// is -1, the symbol is added to
        /// the parse tree by calling
        /// <see cref="ParserRuleContext.AddErrorNode(IToken)">ParserRuleContext.AddErrorNode(IToken)
        ///     </see>
        /// .
        /// </summary>
        /// <param name="ttype">the token type to match</param>
        /// <returns>the matched symbol</returns>
        /// <exception cref="RecognitionException">
        /// if the current input symbol did not match
        /// <code>ttype</code>
        /// and the error strategy could not recover from the
        /// mismatched symbol
        /// </exception>
        /// <exception cref="Antlr4.Runtime.RecognitionException"></exception>
        [return: NotNull]
        public virtual IToken Match(int ttype)
        {
            IToken t = CurrentToken;
            if (t.Type == ttype)
            {
                _errHandler.ReportMatch(this);
                Consume();
            }
            else
            {
                t = _errHandler.RecoverInline(this);
                if (_buildParseTrees && t.TokenIndex == -1)
                {
                    // we must have conjured up a new token during single token insertion
                    // if it's not the current symbol
                    _ctx.AddErrorNode(t);
                }
            }
            return t;
        }

        /// <summary>Match current input symbol as a wildcard.</summary>
        /// <remarks>
        /// Match current input symbol as a wildcard. If the symbol type matches
        /// (i.e. has a value greater than 0),
        /// <see cref="IAntlrErrorStrategy.ReportMatch(Parser)">IAntlrErrorStrategy.ReportMatch(Parser)
        ///     </see>
        /// and
        /// <see cref="Consume()">Consume()</see>
        /// are called to complete the match process.
        /// <p/>
        /// If the symbol type does not match,
        /// <see cref="IAntlrErrorStrategy.RecoverInline(Parser)">IAntlrErrorStrategy.RecoverInline(Parser)
        ///     </see>
        /// is called on the current error
        /// strategy to attempt recovery. If
        /// <see cref="BuildParseTree()">BuildParseTree()</see>
        /// is
        /// <code>true</code>
        /// and the token index of the symbol returned by
        /// <see cref="IAntlrErrorStrategy.RecoverInline(Parser)">IAntlrErrorStrategy.RecoverInline(Parser)
        ///     </see>
        /// is -1, the symbol is added to
        /// the parse tree by calling
        /// <see cref="ParserRuleContext.AddErrorNode(IToken)">ParserRuleContext.AddErrorNode(IToken)
        ///     </see>
        /// .
        /// </remarks>
        /// <returns>the matched symbol</returns>
        /// <exception cref="RecognitionException">
        /// if the current input symbol did not match
        /// a wildcard and the error strategy could not recover from the mismatched
        /// symbol
        /// </exception>
        /// <exception cref="Antlr4.Runtime.RecognitionException"></exception>
        [return: NotNull]
        public virtual IToken MatchWildcard()
        {
            IToken t = CurrentToken;
            if (t.Type > 0)
            {
                _errHandler.ReportMatch(this);
                Consume();
            }
            else
            {
                t = _errHandler.RecoverInline(this);
                if (_buildParseTrees && t.TokenIndex == -1)
                {
                    // we must have conjured up a new token during single token insertion
                    // if it's not the current symbol
                    _ctx.AddErrorNode(t);
                }
            }
            return t;
        }

        /// <summary>
        /// Track the
        /// <see cref="ParserRuleContext">ParserRuleContext</see>
        /// objects during the parse and hook
        /// them up using the
        /// <see cref="ParserRuleContext.children">ParserRuleContext.children</see>
        /// list so that it
        /// forms a parse tree. The
        /// <see cref="ParserRuleContext">ParserRuleContext</see>
        /// returned from the start
        /// rule represents the root of the parse tree.
        /// <p/>
        /// Note that if we are not building parse trees, rule contexts only point
        /// upwards. When a rule exits, it returns the context but that gets garbage
        /// collected if nobody holds a reference. It points upwards but nobody
        /// points at it.
        /// <p/>
        /// When we build parse trees, we are adding all of these contexts to
        /// <see cref="ParserRuleContext.children">ParserRuleContext.children</see>
        /// list. Contexts are then not candidates
        /// for garbage collection.
        /// </summary>
        /// <summary>
        /// Gets whether or not a complete parse tree will be constructed while
        /// parsing.
        /// </summary>
        /// <remarks>
        /// Gets whether or not a complete parse tree will be constructed while
        /// parsing. This property is
        /// <code>true</code>
        /// for a newly constructed parser.
        /// </remarks>
        /// <returns>
        /// 
        /// <code>true</code>
        /// if a complete parse tree will be constructed while
        /// parsing, otherwise
        /// <code>false</code>
        /// </returns>
        public virtual bool BuildParseTree
        {
            get
            {
                return _buildParseTrees;
            }
            set
            {
                bool buildParseTrees = value;
                this._buildParseTrees = buildParseTrees;
            }
        }

        /// <summary>Trim the internal lists of the parse tree during parsing to conserve memory.
        ///     </summary>
        /// <remarks>
        /// Trim the internal lists of the parse tree during parsing to conserve memory.
        /// This property is set to
        /// <code>false</code>
        /// by default for a newly constructed parser.
        /// </remarks>
        /// <value>
        /// 
        /// <code>true</code>
        /// to trim the capacity of the
        /// <see cref="ParserRuleContext.children">ParserRuleContext.children</see>
        /// list to its size after a rule is parsed.
        /// </value>
        /// <returns>
        /// 
        /// <code>true</code>
        /// if the
        /// <see cref="ParserRuleContext.children">ParserRuleContext.children</see>
        /// list is trimmed
        /// using the default
        /// <see cref="TrimToSizeListener">TrimToSizeListener</see>
        /// during the parse process.
        /// </returns>
        public virtual bool TrimParseTree
        {
            get
            {
                return ParseListeners.Contains(Parser.TrimToSizeListener.Instance);
            }
            set
            {
                bool trimParseTrees = value;
                if (trimParseTrees)
                {
                    if (TrimParseTree)
                    {
                        return;
                    }
                    AddParseListener(Parser.TrimToSizeListener.Instance);
                }
                else
                {
                    RemoveParseListener(Parser.TrimToSizeListener.Instance);
                }
            }
        }

        public virtual IList<IParseTreeListener> ParseListeners
        {
            get
            {
                IList<IParseTreeListener> listeners = _parseListeners;
                if (listeners == null)
                {
                    return Sharpen.Collections.EmptyList<IParseTreeListener>();
                }
                return listeners;
            }
        }

        /// <summary>
        /// Registers
        /// <code>listener</code>
        /// to receive events during the parsing process.
        /// <p/>
        /// To support output-preserving grammar transformations (including but not
        /// limited to left-recursion removal, automated left-factoring, and
        /// optimized code generation), calls to listener methods during the parse
        /// may differ substantially from calls made by
        /// <see cref="Antlr4.Runtime.Tree.ParseTreeWalker.Default">Antlr4.Runtime.Tree.ParseTreeWalker.Default
        ///     </see>
        /// used after the parse is complete. In
        /// particular, rule entry and exit events may occur in a different order
        /// during the parse than after the parser. In addition, calls to certain
        /// rule entry methods may be omitted.
        /// <p/>
        /// With the following specific exceptions, calls to listener events are
        /// <em>deterministic</em>, i.e. for identical input the calls to listener
        /// methods will be the same.
        /// <ul>
        /// <li>Alterations to the grammar used to generate code may change the
        /// behavior of the listener calls.</li>
        /// <li>Alterations to the command line options passed to ANTLR 4 when
        /// generating the parser may change the behavior of the listener calls.</li>
        /// <li>Changing the version of the ANTLR Tool used to generate the parser
        /// may change the behavior of the listener calls.</li>
        /// </ul>
        /// </summary>
        /// <param name="listener">the listener to add</param>
        /// <exception cref="System.ArgumentNullException">
        /// if
        /// <code></code>
        /// listener is
        /// <code>null</code>
        /// </exception>
        public virtual void AddParseListener(IParseTreeListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            if (_parseListeners == null)
            {
                _parseListeners = new List<IParseTreeListener>();
            }
            this._parseListeners.Add(listener);
        }

        /// <summary>
        /// Remove
        /// <code>listener</code>
        /// from the list of parse listeners.
        /// <p/>
        /// If
        /// <code>listener</code>
        /// is
        /// <code>null</code>
        /// or has not been added as a parse
        /// listener, this method does nothing.
        /// </summary>
        /// <seealso cref="AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)">AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)
        ///     </seealso>
        /// <param name="listener">the listener to remove</param>
        public virtual void RemoveParseListener(IParseTreeListener listener)
        {
            if (_parseListeners != null)
            {
                if (_parseListeners.Remove(listener))
                {
                    if (_parseListeners.Count == 0)
                    {
                        _parseListeners = null;
                    }
                }
            }
        }

        /// <summary>Remove all parse listeners.</summary>
        /// <remarks>Remove all parse listeners.</remarks>
        /// <seealso cref="AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)">AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)
        ///     </seealso>
        public virtual void RemoveParseListeners()
        {
            _parseListeners = null;
        }

        /// <summary>Notify any parse listeners of an enter rule event.</summary>
        /// <remarks>Notify any parse listeners of an enter rule event.</remarks>
        /// <seealso cref="AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)">AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)
        ///     </seealso>
        protected internal virtual void TriggerEnterRuleEvent()
        {
            foreach (IParseTreeListener listener in _parseListeners)
            {
                listener.EnterEveryRule(_ctx);
                _ctx.EnterRule(listener);
            }
        }

        /// <summary>Notify any parse listeners of an exit rule event.</summary>
        /// <remarks>Notify any parse listeners of an exit rule event.</remarks>
        /// <seealso cref="AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)">AddParseListener(Antlr4.Runtime.Tree.IParseTreeListener)
        ///     </seealso>
        protected internal virtual void TriggerExitRuleEvent()
        {
            // reverse order walk of listeners
            for (int i = _parseListeners.Count - 1; i >= 0; i--)
            {
                IParseTreeListener listener = _parseListeners[i];
                _ctx.ExitRule(listener);
                listener.ExitEveryRule(_ctx);
            }
        }

        /// <summary>Gets the number of syntax errors reported during parsing.</summary>
        /// <remarks>
        /// Gets the number of syntax errors reported during parsing. This value is
        /// incremented each time
        /// <see cref="NotifyErrorListeners(string)">NotifyErrorListeners(string)</see>
        /// is called.
        /// </remarks>
        /// <seealso cref="NotifyErrorListeners(string)">NotifyErrorListeners(string)</seealso>
        public virtual int NumberOfSyntaxErrors
        {
            get
            {
                return _syntaxErrors;
            }
        }

        public virtual ITokenFactory GetTokenFactory()
        {
            return _input.TokenSource.TokenFactory;
        }

        public virtual IAntlrErrorStrategy ErrorHandler
        {
            get
            {
                return _errHandler;
            }
            set
            {
                IAntlrErrorStrategy handler = value;
                this._errHandler = handler;
            }
        }

        public override ITokenStream InputStream
        {
            get
            {
                return _input;
            }
        }

        /// <summary>Set the token stream and reset the parser.</summary>
        /// <remarks>Set the token stream and reset the parser.</remarks>
        public virtual void SetInputStream(ITokenStream input)
        {
            this._input = null;
            Reset();
            this._input = input;
        }

        /// <summary>
        /// Match needs to return the current input symbol, which gets put
        /// into the label for the associated token ref; e.g., x=ID.
        /// </summary>
        /// <remarks>
        /// Match needs to return the current input symbol, which gets put
        /// into the label for the associated token ref; e.g., x=ID.
        /// </remarks>
        public virtual IToken CurrentToken
        {
            get
            {
                return _input.Lt(1);
            }
        }

        public void NotifyErrorListeners(string msg)
        {
            NotifyErrorListeners(CurrentToken, msg, null);
        }

        public virtual void NotifyErrorListeners(IToken offendingToken, string msg, RecognitionException
             e)
        {
            _syntaxErrors++;
            int line = -1;
            int charPositionInLine = -1;
            if (offendingToken != null)
            {
                line = offendingToken.Line;
                charPositionInLine = offendingToken.Column;
            }
            IAntlrErrorListener<IToken> listener = ((IParserErrorListener)GetErrorListenerDispatch
                ());
            listener.SyntaxError(this, offendingToken, line, charPositionInLine, msg, e);
        }

        /// <summary>
        /// Consume and return the
        /// <linkplain>
        /// #getCurrentToken
        /// current symbol
        /// </linkplain>
        /// .
        /// <p/>
        /// E.g., given the following input with
        /// <code>A</code>
        /// being the current
        /// lookahead symbol, this function moves the cursor to
        /// <code>B</code>
        /// and returns
        /// <code>A</code>
        /// .
        /// <pre>
        /// A B
        /// ^
        /// </pre>
        /// If the parser is not in error recovery mode, the consumed symbol is added
        /// to the parse tree using
        /// <see cref="ParserRuleContext.AddChild(IToken)">ParserRuleContext.AddChild(IToken)
        ///     </see>
        /// , and
        /// <see cref="Antlr4.Runtime.Tree.IParseTreeListener.VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode)
        ///     ">Antlr4.Runtime.Tree.IParseTreeListener.VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode)
        ///     </see>
        /// is called on any parse listeners.
        /// If the parser <em>is</em> in error recovery mode, the consumed symbol is
        /// added to the parse tree using
        /// <see cref="ParserRuleContext.AddErrorNode(IToken)">ParserRuleContext.AddErrorNode(IToken)
        ///     </see>
        /// , and
        /// <see cref="Antlr4.Runtime.Tree.IParseTreeListener.VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode)
        ///     ">Antlr4.Runtime.Tree.IParseTreeListener.VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode)
        ///     </see>
        /// is called on any parse
        /// listeners.
        /// </summary>
        public virtual IToken Consume()
        {
            IToken o = CurrentToken;
            if (o.Type != Eof)
            {
                ((ITokenStream)InputStream).Consume();
            }
            bool hasListener = _parseListeners != null && _parseListeners.Count != 0;
            if (_buildParseTrees || hasListener)
            {
                if (_errHandler.InErrorRecoveryMode(this))
                {
                    IErrorNode node = _ctx.AddErrorNode(o);
                    if (_parseListeners != null)
                    {
                        foreach (IParseTreeListener listener in _parseListeners)
                        {
                            listener.VisitErrorNode(node);
                        }
                    }
                }
                else
                {
                    ITerminalNode node = _ctx.AddChild(o);
                    if (_parseListeners != null)
                    {
                        foreach (IParseTreeListener listener in _parseListeners)
                        {
                            listener.VisitTerminal(node);
                        }
                    }
                }
            }
            return o;
        }

        protected internal virtual void AddContextToParseTree()
        {
            ParserRuleContext parent = (ParserRuleContext)_ctx.parent;
            // add current context to parent if we have a parent
            if (parent != null)
            {
                parent.AddChild(_ctx);
            }
        }

        /// <summary>Always called by generated parsers upon entry to a rule.</summary>
        /// <remarks>
        /// Always called by generated parsers upon entry to a rule. Access field
        /// <see cref="_ctx">_ctx</see>
        /// get the current context.
        /// </remarks>
        public virtual void EnterRule(ParserRuleContext localctx, int state, int ruleIndex
            )
        {
            State = state;
            _ctx = localctx;
            _ctx.start = _input.Lt(1);
            if (_buildParseTrees)
            {
                AddContextToParseTree();
            }
            if (_parseListeners != null)
            {
                TriggerEnterRuleEvent();
            }
        }

        public virtual void EnterLeftFactoredRule(ParserRuleContext localctx, int state, 
            int ruleIndex)
        {
            State = state;
            if (_buildParseTrees)
            {
                ParserRuleContext factoredContext = (ParserRuleContext)_ctx.GetChild(_ctx.ChildCount
                     - 1);
                _ctx.RemoveLastChild();
                factoredContext.parent = localctx;
                localctx.AddChild(factoredContext);
            }
            _ctx = localctx;
            _ctx.start = _input.Lt(1);
            if (_buildParseTrees)
            {
                AddContextToParseTree();
            }
            if (_parseListeners != null)
            {
                TriggerEnterRuleEvent();
            }
        }

        public virtual void ExitRule()
        {
            _ctx.stop = _input.Lt(-1);
            // trigger event on _ctx, before it reverts to parent
            if (_parseListeners != null)
            {
                TriggerExitRuleEvent();
            }
            State = _ctx.invokingState;
            _ctx = (ParserRuleContext)_ctx.parent;
        }

        public virtual void EnterOuterAlt(ParserRuleContext localctx, int altNum)
        {
            // if we have new localctx, make sure we replace existing ctx
            // that is previous child of parse tree
            if (_buildParseTrees && _ctx != localctx)
            {
                ParserRuleContext parent = (ParserRuleContext)_ctx.parent;
                if (parent != null)
                {
                    parent.RemoveLastChild();
                    parent.AddChild(localctx);
                }
            }
            _ctx = localctx;
        }

        public virtual void EnterRecursionRule(ParserRuleContext localctx, int ruleIndex, 
            int precedence)
        {
            _precedenceStack.Add(precedence);
            _ctx = localctx;
            _ctx.start = _input.Lt(1);
            if (_parseListeners != null)
            {
                TriggerEnterRuleEvent();
            }
        }

        // simulates rule entry for left-recursive rules
        /// <summary>
        /// Like
        /// <see cref="EnterRule(ParserRuleContext, int, int)">EnterRule(ParserRuleContext, int, int)
        ///     </see>
        /// but for recursive rules.
        /// </summary>
        public virtual void PushNewRecursionContext(ParserRuleContext localctx, int state
            , int ruleIndex)
        {
            ParserRuleContext previous = _ctx;
            previous.parent = localctx;
            previous.invokingState = state;
            previous.stop = _input.Lt(-1);
            _ctx = localctx;
            _ctx.start = previous.start;
            if (_buildParseTrees)
            {
                _ctx.AddChild(previous);
            }
            if (_parseListeners != null)
            {
                TriggerEnterRuleEvent();
            }
        }

        // simulates rule entry for left-recursive rules
        public virtual void UnrollRecursionContexts(ParserRuleContext _parentctx)
        {
            _precedenceStack.RemoveAt(_precedenceStack.Count - 1);
            _ctx.stop = _input.Lt(-1);
            ParserRuleContext retctx = _ctx;
            // save current ctx (return value)
            // unroll so _ctx is as it was before call to recursive method
            if (_parseListeners != null)
            {
                while (_ctx != _parentctx)
                {
                    TriggerExitRuleEvent();
                    _ctx = (ParserRuleContext)_ctx.parent;
                }
            }
            else
            {
                _ctx = _parentctx;
            }
            // hook into tree
            retctx.parent = _parentctx;
            if (_buildParseTrees && _parentctx != null)
            {
                // add return ctx into invoking rule's tree
                _parentctx.AddChild(retctx);
            }
        }

        public virtual ParserRuleContext GetInvokingContext(int ruleIndex)
        {
            ParserRuleContext p = _ctx;
            while (p != null)
            {
                if (p.GetRuleIndex() == ruleIndex)
                {
                    return p;
                }
                p = (ParserRuleContext)p.parent;
            }
            return null;
        }

        public virtual ParserRuleContext Context
        {
            get
            {
                return _ctx;
            }
        }

        public override bool Precpred(RuleContext localctx, int precedence)
        {
            return precedence >= _precedenceStack[_precedenceStack.Count - 1];
        }

        public override IAntlrErrorListener<IToken> GetErrorListenerDispatch()
        {
            return new ProxyParserErrorListener(GetErrorListeners());
        }

        public virtual bool InContext(string context)
        {
            // TODO: useful in parser?
            return false;
        }

        /// <summary>
        /// Checks whether or not
        /// <code>symbol</code>
        /// can follow the current state in the
        /// ATN. The behavior of this method is equivalent to the following, but is
        /// implemented such that the complete context-sensitive follow set does not
        /// need to be explicitly constructed.
        /// <pre>
        /// return getExpectedTokens().contains(symbol);
        /// </pre>
        /// </summary>
        /// <param name="symbol">the symbol type to check</param>
        /// <returns>
        /// 
        /// <code>true</code>
        /// if
        /// <code>symbol</code>
        /// can follow the current state in
        /// the ATN, otherwise
        /// <code>false</code>
        /// .
        /// </returns>
        public virtual bool IsExpectedToken(int symbol)
        {
            //   		return getInterpreter().atn.nextTokens(_ctx);
            ATN atn = Interpreter.atn;
            ParserRuleContext ctx = _ctx;
            ATNState s = atn.states[State];
            IntervalSet following = atn.NextTokens(s);
            if (following.Contains(symbol))
            {
                return true;
            }
            //        System.out.println("following "+s+"="+following);
            if (!following.Contains(TokenConstants.Epsilon))
            {
                return false;
            }
            while (ctx != null && ctx.invokingState >= 0 && following.Contains(TokenConstants
                .Epsilon))
            {
                ATNState invokingState = atn.states[ctx.invokingState];
                RuleTransition rt = (RuleTransition)invokingState.Transition(0);
                following = atn.NextTokens(rt.followState);
                if (following.Contains(symbol))
                {
                    return true;
                }
                ctx = (ParserRuleContext)ctx.parent;
            }
            if (following.Contains(TokenConstants.Epsilon) && symbol == TokenConstants.Eof)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Computes the set of input symbols which could follow the current parser
        /// state and context, as given by
        /// <see cref="Recognizer{Symbol, ATNInterpreter}.State()">Recognizer&lt;Symbol, ATNInterpreter&gt;.State()
        ///     </see>
        /// and
        /// <see cref="Context()">Context()</see>
        /// ,
        /// respectively.
        /// </summary>
        /// <seealso cref="Antlr4.Runtime.Atn.ATN.GetExpectedTokens(int, RuleContext)">Antlr4.Runtime.Atn.ATN.GetExpectedTokens(int, RuleContext)
        ///     </seealso>
        [return: NotNull]
        public virtual IntervalSet GetExpectedTokens()
        {
            return Atn.GetExpectedTokens(State, Context);
        }

        [return: NotNull]
        public virtual IntervalSet GetExpectedTokensWithinCurrentRule()
        {
            ATN atn = Interpreter.atn;
            ATNState s = atn.states[State];
            return atn.NextTokens(s);
        }

        public virtual ParserRuleContext RuleContext
        {
            get
            {
                //	/** Compute the set of valid tokens reachable from the current
                //	 *  position in the parse.
                //	 */
                //	public IntervalSet nextTokens(@NotNull RuleContext ctx) {
                //		ATN atn = getInterpreter().atn;
                //		ATNState s = atn.states.get(ctx.s);
                //		if ( s == null ) return null;
                //		return atn.nextTokens(s, ctx);
                //	}
                return _ctx;
            }
        }

        /// <summary>
        /// Return List&lt;String&gt; of the rule names in your parser instance
        /// leading up to a call to the current rule.
        /// </summary>
        /// <remarks>
        /// Return List&lt;String&gt; of the rule names in your parser instance
        /// leading up to a call to the current rule.  You could override if
        /// you want more details such as the file/line info of where
        /// in the ATN a rule is invoked.
        /// This is very useful for error messages.
        /// </remarks>
        public virtual IList<string> GetRuleInvocationStack()
        {
            return GetRuleInvocationStack(_ctx);
        }

        public virtual IList<string> GetRuleInvocationStack(RuleContext p)
        {
            string[] ruleNames = RuleNames;
            IList<string> stack = new List<string>();
            while (p != null)
            {
                // compute what follows who invoked us
                int ruleIndex = p.GetRuleIndex();
                if (ruleIndex < 0)
                {
                    stack.Add("n/a");
                }
                else
                {
                    stack.Add(ruleNames[ruleIndex]);
                }
                p = p.parent;
            }
            return stack;
        }

        /// <summary>For debugging and other purposes.</summary>
        /// <remarks>For debugging and other purposes.</remarks>
        public virtual IList<string> GetDFAStrings()
        {
            IList<string> s = new List<string>();
            for (int d = 0; d < _interp.atn.decisionToDFA.Length; d++)
            {
                DFA dfa = _interp.atn.decisionToDFA[d];
                s.Add(dfa.ToString(TokenNames, RuleNames));
            }
            return s;
        }

#if !PORTABLE
        /// <summary>For debugging and other purposes.</summary>
        /// <remarks>For debugging and other purposes.</remarks>
        public virtual void DumpDFA()
        {
            bool seenOne = false;
            for (int d = 0; d < _interp.atn.decisionToDFA.Length; d++)
            {
                DFA dfa = _interp.atn.decisionToDFA[d];
                if (!dfa.IsEmpty())
                {
                    if (seenOne)
                    {
                        System.Console.Out.WriteLine();
                    }
                    System.Console.Out.WriteLine("Decision " + dfa.decision + ":");
                    System.Console.Out.Write(dfa.ToString(TokenNames, RuleNames));
                    seenOne = true;
                }
            }
        }
#endif

        public virtual string SourceName
        {
            get
            {
                return _input.SourceName;
            }
        }

#if !PORTABLE
        /// <summary>
        /// During a parse is sometimes useful to listen in on the rule entry and exit
        /// events as well as token matches.
        /// </summary>
        /// <remarks>
        /// During a parse is sometimes useful to listen in on the rule entry and exit
        /// events as well as token matches. This is for quick and dirty debugging.
        /// </remarks>
        public virtual bool Trace
        {
            get
            {
                foreach (object o in ParseListeners)
                {
                    if (o is Parser.TraceListener)
                    {
                        return true;
                    }
                }
                return false;
            }
            set
            {
                bool trace = value;
                if (!trace)
                {
                    RemoveParseListener(_tracer);
                    _tracer = null;
                }
                else
                {
                    if (_tracer != null)
                    {
                        RemoveParseListener(_tracer);
                    }
                    else
                    {
                        _tracer = new Parser.TraceListener(this);
                    }
                    AddParseListener(_tracer);
                }
            }
        }
#endif
    }
}
