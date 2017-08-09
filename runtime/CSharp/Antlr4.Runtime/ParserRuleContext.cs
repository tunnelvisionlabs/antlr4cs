// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;

namespace Antlr4.Runtime
{
    /// <summary>A rule invocation record for parsing.</summary>
    /// <remarks>
    /// A rule invocation record for parsing.
    /// Contains all of the information about the current rule not stored in the
    /// RuleContext. It handles parse tree children list, Any ATN state
    /// tracing, and the default values available for rule invocations:
    /// start, stop, rule index, current alt number.
    /// Subclasses made for each rule and grammar track the parameters,
    /// return values, locals, and labels specific to that rule. These
    /// are the objects that are returned from rules.
    /// Note text is not an actual field of a rule return value; it is computed
    /// from start and stop using the input stream's toString() method.  I
    /// could add a ctor to this so that we can pass in and store the input
    /// stream, but I'm not sure we want to do that.  It would seem to be undefined
    /// to get the .text property anyway if the rule matches tokens from multiple
    /// input streams.
    /// I do not use getters for fields of objects that are used simply to
    /// group values such as this aggregate.  The getters/setters are there to
    /// satisfy the superclass interface.
    /// </remarks>
    public class ParserRuleContext : RuleContext
    {
        private static readonly Antlr4.Runtime.ParserRuleContext Empty = new Antlr4.Runtime.ParserRuleContext();

        /// <summary>
        /// If we are debugging or building a parse tree for a visitor,
        /// we need to track all of the tokens and rule invocations associated
        /// with this rule's context.
        /// </summary>
        /// <remarks>
        /// If we are debugging or building a parse tree for a visitor,
        /// we need to track all of the tokens and rule invocations associated
        /// with this rule's context. This is empty for parsing w/o tree constr.
        /// operation because we don't the need to track the details about
        /// how we parse this rule.
        /// </remarks>
        public IList<IParseTree> children;

        /// <summary>
        /// For debugging/tracing purposes, we want to track all of the nodes in
        /// the ATN traversed by the parser for a particular rule.
        /// </summary>
        /// <remarks>
        /// For debugging/tracing purposes, we want to track all of the nodes in
        /// the ATN traversed by the parser for a particular rule.
        /// This list indicates the sequence of ATN nodes used to match
        /// the elements of the children list. This list does not include
        /// ATN nodes and other rules used to match rule invocations. It
        /// traces the rule invocation node itself but nothing inside that
        /// other rule's ATN submachine.
        /// There is NOT a one-to-one correspondence between the children and
        /// states list. There are typically many nodes in the ATN traversed
        /// for each element in the children list. For example, for a rule
        /// invocation there is the invoking state and the following state.
        /// The parser setState() method updates field s and adds it to this list
        /// if we are debugging/tracing.
        /// This does not trace states visited during prediction.
        /// </remarks>
        public IToken start;

        /// <summary>
        /// For debugging/tracing purposes, we want to track all of the nodes in
        /// the ATN traversed by the parser for a particular rule.
        /// </summary>
        /// <remarks>
        /// For debugging/tracing purposes, we want to track all of the nodes in
        /// the ATN traversed by the parser for a particular rule.
        /// This list indicates the sequence of ATN nodes used to match
        /// the elements of the children list. This list does not include
        /// ATN nodes and other rules used to match rule invocations. It
        /// traces the rule invocation node itself but nothing inside that
        /// other rule's ATN submachine.
        /// There is NOT a one-to-one correspondence between the children and
        /// states list. There are typically many nodes in the ATN traversed
        /// for each element in the children list. For example, for a rule
        /// invocation there is the invoking state and the following state.
        /// The parser setState() method updates field s and adds it to this list
        /// if we are debugging/tracing.
        /// This does not trace states visited during prediction.
        /// </remarks>
        public IToken stop;

        /// <summary>The exception that forced this rule to return.</summary>
        /// <remarks>
        /// The exception that forced this rule to return. If the rule successfully
        /// completed, this is
        /// <see langword="null"/>
        /// .
        /// </remarks>
        public RecognitionException exception;

        public ParserRuleContext()
        {
        }

        public static Antlr4.Runtime.ParserRuleContext EmptyContext
        {
            get
            {
                //	public List<Integer> states;
                return Empty;
            }
        }

        /// <summary>
        /// COPY a ctx (I'm deliberately not using copy constructor) to avoid
        /// confusion with creating node with parent.
        /// </summary>
        /// <remarks>
        /// COPY a ctx (I'm deliberately not using copy constructor) to avoid
        /// confusion with creating node with parent. Does not copy children.
        /// <p>This is used in the generated parser code to flip a generic XContext
        /// node for rule X to a YContext for alt label Y. In that sense, it is not
        /// really a generic copy function.</p>
        /// <p>If we do an error sync() at start of a rule, we might add error nodes
        /// to the generic XContext so this function must copy those nodes to the
        /// YContext as well else they are lost!</p>
        /// </remarks>
        public virtual void CopyFrom(Antlr4.Runtime.ParserRuleContext ctx)
        {
            this.parent = ctx.parent;
            this.invokingState = ctx.invokingState;
            this.start = ctx.start;
            this.stop = ctx.stop;
            // copy any error nodes to alt label node
            if (ctx.children != null)
            {
                this.children = new List<IParseTree>();
                // reset parent pointer for any error nodes
                foreach (IParseTree child in ctx.children)
                {
                    if (child is ErrorNodeImpl)
                    {
                        this.children.Add(child);
                        ((ErrorNodeImpl)child).parent = this;
                    }
                }
            }
        }

        public ParserRuleContext([Nullable] Antlr4.Runtime.ParserRuleContext parent, int invokingStateNumber)
            : base(parent, invokingStateNumber)
        {
        }

        // Double dispatch methods for listeners
        public virtual void EnterRule(IParseTreeListener listener)
        {
        }

        public virtual void ExitRule(IParseTreeListener listener)
        {
        }

        /// <summary>Does not set parent link; other add methods do that</summary>
        public virtual void AddChild(ITerminalNode t)
        {
            if (children == null)
            {
                children = new List<IParseTree>();
            }
            children.Add(t);
        }

        public virtual void AddChild(RuleContext ruleInvocation)
        {
            if (children == null)
            {
                children = new List<IParseTree>();
            }
            children.Add(ruleInvocation);
        }

        /// <summary>
        /// Used by enterOuterAlt to toss out a RuleContext previously added as
        /// we entered a rule.
        /// </summary>
        /// <remarks>
        /// Used by enterOuterAlt to toss out a RuleContext previously added as
        /// we entered a rule. If we have # label, we will need to remove
        /// generic ruleContext object.
        /// </remarks>
        public virtual void RemoveLastChild()
        {
            if (children != null)
            {
                children.RemoveAt(children.Count - 1);
            }
        }

        //	public void trace(int s) {
        //		if ( states==null ) states = new ArrayList<Integer>();
        //		states.add(s);
        //	}
        public virtual ITerminalNode AddChild(IToken matchedToken)
        {
            TerminalNodeImpl t = new TerminalNodeImpl(matchedToken);
            AddChild(t);
            t.parent = this;
            return t;
        }

        public virtual IErrorNode AddErrorNode(IToken badToken)
        {
            ErrorNodeImpl t = new ErrorNodeImpl(badToken);
            AddChild(t);
            t.parent = this;
            return t;
        }

        public override RuleContext Parent
        {
            get
            {
                return (Antlr4.Runtime.ParserRuleContext)base.Parent;
            }
        }

        public override IParseTree GetChild(int i)
        {
            return children != null && i >= 0 && i < children.Count ? children[i] : null;
        }

        public virtual T GetChild<T>(Type ctxType, int i)
            where T : IParseTree
        {
            if (children == null || i < 0 || i >= children.Count)
            {
                return null;
            }
            int j = -1;
            // what element have we found with ctxType?
            foreach (IParseTree o in children)
            {
                if (ctxType.IsInstanceOfType(o))
                {
                    j++;
                    if (j == i)
                    {
                        return ctxType.Cast(o);
                    }
                }
            }
            return null;
        }

        public virtual ITerminalNode GetToken(int ttype, int i)
        {
            if (children == null || i < 0 || i >= children.Count)
            {
                return null;
            }
            int j = -1;
            // what token with ttype have we found?
            foreach (IParseTree o in children)
            {
                if (o is ITerminalNode)
                {
                    ITerminalNode tnode = (ITerminalNode)o;
                    IToken symbol = tnode.Symbol;
                    if (symbol.Type == ttype)
                    {
                        j++;
                        if (j == i)
                        {
                            return tnode;
                        }
                    }
                }
            }
            return null;
        }

        public virtual IList<ITerminalNode> GetTokens(int ttype)
        {
            if (children == null)
            {
                return Antlr4.Runtime.Sharpen.Collections.EmptyList();
            }
            IList<ITerminalNode> tokens = null;
            foreach (IParseTree o in children)
            {
                if (o is ITerminalNode)
                {
                    ITerminalNode tnode = (ITerminalNode)o;
                    IToken symbol = tnode.Symbol;
                    if (symbol.Type == ttype)
                    {
                        if (tokens == null)
                        {
                            tokens = new List<ITerminalNode>();
                        }
                        tokens.Add(tnode);
                    }
                }
            }
            if (tokens == null)
            {
                return Antlr4.Runtime.Sharpen.Collections.EmptyList();
            }
            return tokens;
        }

        public virtual T GetRuleContext<T>(Type ctxType, int i)
            where T : Antlr4.Runtime.ParserRuleContext
        {
            return GetChild(ctxType, i);
        }

        public virtual IList<T> GetRuleContexts<T>(Type ctxType)
            where T : Antlr4.Runtime.ParserRuleContext
        {
            if (children == null)
            {
                return Antlr4.Runtime.Sharpen.Collections.EmptyList();
            }
            IList<T> contexts = null;
            foreach (IParseTree o in children)
            {
                if (ctxType.IsInstanceOfType(o))
                {
                    if (contexts == null)
                    {
                        contexts = new List<T>();
                    }
                    contexts.Add(ctxType.Cast(o));
                }
            }
            if (contexts == null)
            {
                return Antlr4.Runtime.Sharpen.Collections.EmptyList();
            }
            return contexts;
        }

        public override int ChildCount
        {
            get
            {
                return children != null ? children.Count : 0;
            }
        }

        public override Interval SourceInterval
        {
            get
            {
                if (start == null)
                {
                    return Interval.Invalid;
                }
                if (stop == null || stop.TokenIndex < start.TokenIndex)
                {
                    return Interval.Of(start.TokenIndex, start.TokenIndex - 1);
                }
                // empty
                return Interval.Of(start.TokenIndex, stop.TokenIndex);
            }
        }

        /// <summary>Get the initial token in this context.</summary>
        /// <remarks>
        /// Get the initial token in this context.
        /// Note that the range from start to stop is inclusive, so for rules that do not consume anything
        /// (for example, zero length or error productions) this token may exceed stop.
        /// </remarks>
        public virtual IToken Start
        {
            get
            {
                return start;
            }
        }

        /// <summary>Get the final token in this context.</summary>
        /// <remarks>
        /// Get the final token in this context.
        /// Note that the range from start to stop is inclusive, so for rules that do not consume anything
        /// (for example, zero length or error productions) this token may precede start.
        /// </remarks>
        public virtual IToken Stop
        {
            get
            {
                return stop;
            }
        }

        /// <summary>Used for rule context info debugging during parse-time, not so much for ATN debugging</summary>
        public virtual string ToInfoString(Parser recognizer)
        {
            IList<string> rules = recognizer.GetRuleInvocationStack(this);
            Antlr4.Runtime.Sharpen.Collections.Reverse(rules);
            return "ParserRuleContext" + rules + "{" + "start=" + start + ", stop=" + stop + '}';
        }
    }
}
