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

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using System.Text;
    using Antlr4.Codegen.Model;
    using Antlr4.Codegen.Model.Chunk;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using ANTLRStringStream = Antlr.Runtime.ANTLRStringStream;
    using Exception = System.Exception;
    using IToken = Antlr.Runtime.IToken;

    /** */
    public class ActionTranslator : ActionSplitterListener
    {
        public static readonly IDictionary<string, System.Func<StructDecl, string, RulePropertyRef>> thisRulePropToModelMap =
            new Dictionary<string, System.Func<StructDecl, string, RulePropertyRef>>
            {
                { "start", (ctx, label) => new ThisRulePropertyRef_start(ctx, label) },
                { "stop",  (ctx, label) => new ThisRulePropertyRef_stop(ctx, label) },
                { "text",  (ctx, label) => new ThisRulePropertyRef_text(ctx, label) },
                { "ctx",   (ctx, label) => new ThisRulePropertyRef_ctx(ctx, label) },
                { "parser",  (ctx, label) => new ThisRulePropertyRef_parser(ctx, label) },
            };

        public static readonly IDictionary<string, System.Func<StructDecl, string, RulePropertyRef>> rulePropToModelMap =
            new Dictionary<string, System.Func<StructDecl, string, RulePropertyRef>>
            {
                { "start", (ctx, label) => new RulePropertyRef_start(ctx, label) },
                { "stop",  (ctx, label) => new RulePropertyRef_stop(ctx, label) },
                { "text",  (ctx, label) => new RulePropertyRef_text(ctx, label) },
                { "ctx",   (ctx, label) => new RulePropertyRef_ctx(ctx, label) },
                { "parser",  (ctx, label) => new RulePropertyRef_parser(ctx, label) },
            };

        public static readonly IDictionary<string, System.Func<StructDecl, string, TokenPropertyRef>> tokenPropToModelMap =
            new Dictionary<string, System.Func<StructDecl, string, TokenPropertyRef>>
            {
                { "text",  (ctx, label) => new TokenPropertyRef_text(ctx, label) },
                { "type",  (ctx, label) => new TokenPropertyRef_type(ctx, label) },
                { "line",  (ctx, label) => new TokenPropertyRef_line(ctx, label) },
                { "index", (ctx, label) => new TokenPropertyRef_index(ctx, label) },
                { "pos",   (ctx, label) => new TokenPropertyRef_pos(ctx, label) },
                { "channel", (ctx, label) => new TokenPropertyRef_channel(ctx, label) },
                { "int",   (ctx, label) => new TokenPropertyRef_int(ctx, label) },
            };

        internal CodeGenerator gen;
        internal ActionAST node;
        internal RuleFunction rf;
        internal IList<ActionChunk> chunks = new List<ActionChunk>();
        internal OutputModelFactory factory;
        internal StructDecl nodeContext;

        public ActionTranslator(OutputModelFactory factory, ActionAST node)
        {
            this.factory = factory;
            this.node = node;
            this.gen = factory.GetGenerator();
        }

        public static string ToString(IList<ActionChunk> chunks)
        {
            StringBuilder buf = new StringBuilder();
            foreach (ActionChunk c in chunks)
                buf.Append(c.ToString());

            return buf.ToString();
        }

        public static IList<ActionChunk> TranslateAction(OutputModelFactory factory,
                                                        RuleFunction rf,
                                                        IToken tokenWithinAction,
                                                        ActionAST node)
        {
            string action = tokenWithinAction.Text;
            if (action != null && action.Length > 0 && action[0] == '{')
            {
                int firstCurly = action.IndexOf('{');
                int lastCurly = action.LastIndexOf('}');
                if (firstCurly >= 0 && lastCurly >= 0)
                {
                    action = action.Substring(firstCurly + 1, lastCurly - firstCurly - 1); // trim {...}
                }
            }

            return TranslateActionChunk(factory, rf, action, node);
        }

        public static IList<ActionChunk> TranslateActionChunk(OutputModelFactory factory,
                                                             RuleFunction rf,
                                                             string action,
                                                             ActionAST node)
        {
            IToken tokenWithinAction = node.Token;
            ActionTranslator translator = new ActionTranslator(factory, node);
            translator.rf = rf;
            factory.GetGrammar().tool.Log("action-translator", "translate " + action);
            string altLabel = node.GetAltLabel();
            if (rf != null)
            {
                translator.nodeContext = rf.ruleCtx;
                if (altLabel != null)
                {
                    AltLabelStructDecl decl;
                    rf.altLabelCtxs.TryGetValue(altLabel, out decl);
                    translator.nodeContext = decl;
                }
            }
            ANTLRStringStream @in = new ANTLRStringStream(action);
            @in.Line = tokenWithinAction.Line;
            @in.CharPositionInLine = tokenWithinAction.CharPositionInLine;
            ActionSplitter trigger = new ActionSplitter(@in, translator);
            // forces eval, triggers listener methods
            trigger.GetActionTokens();
            return translator.chunks;
        }

        public virtual void Attr(string expr, IToken x)
        {
            gen.g.tool.Log("action-translator", "attr " + x);
            Attribute a = node.resolver.ResolveToAttribute(x.Text, node);
            if (a != null)
            {
                switch (a.dict.type)
                {
                case AttributeDict.DictType.ARG:
                    chunks.Add(new ArgRef(nodeContext, x.Text));
                    break;
                case AttributeDict.DictType.RET:
                    chunks.Add(new RetValueRef(rf.ruleCtx, x.Text));
                    break;
                case AttributeDict.DictType.LOCAL:
                    chunks.Add(new LocalRef(nodeContext, x.Text));
                    break;
                case AttributeDict.DictType.PREDEFINED_RULE:
                    chunks.Add(GetRulePropertyRef(x));
                    break;
                }
            }
            if (node.resolver.ResolvesToToken(x.Text, node))
            {
                chunks.Add(new TokenRef(nodeContext, GetTokenLabel(x.Text))); // $label
                return;
            }
            if (node.resolver.ResolvesToLabel(x.Text, node))
            {
                chunks.Add(new LabelRef(nodeContext, GetTokenLabel(x.Text))); // $x for x=ID etc...
                return;
            }
            if (node.resolver.ResolvesToListLabel(x.Text, node))
            {
                chunks.Add(new ListLabelRef(nodeContext, x.Text)); // $ids for ids+=ID etc...
                return;
            }
            Rule r = factory.GetGrammar().GetRule(x.Text);
            if (r != null)
            {
                chunks.Add(new LabelRef(nodeContext, GetRuleLabel(x.Text))); // $r for r rule ref
            }
        }

        public virtual void QualifiedAttr(string expr, IToken x, IToken y)
        {
            gen.g.tool.Log("action-translator", "qattr " + x + "." + y);
            if (node.resolver.ResolveToAttribute(x.Text, node) != null)
            {
                // must be a member access to a predefined attribute like $ctx.foo
                Attr(expr, x);
                chunks.Add(new ActionText(nodeContext, "." + y.Text));
                return;
            }
            Attribute a = node.resolver.ResolveToAttribute(x.Text, y.Text, node);
            switch (a.dict.type)
            {
            case AttributeDict.DictType.ARG:
                chunks.Add(new ArgRef(nodeContext, y.Text));
                break; // has to be current rule
            case AttributeDict.DictType.RET:
                if (factory.GetCurrentRuleFunction() != null &&
                    factory.GetCurrentRuleFunction().name.Equals(x.Text))
                {
                    chunks.Add(new RetValueRef(rf.ruleCtx, y.Text));
                    break;
                }
                else
                {
                    chunks.Add(new QRetValueRef(nodeContext, GetRuleLabel(x.Text), y.Text));
                    break;
                }
            case AttributeDict.DictType.PREDEFINED_RULE:
                if (factory.GetCurrentRuleFunction() != null &&
                    factory.GetCurrentRuleFunction().name.Equals(x.Text))
                {
                    chunks.Add(GetRulePropertyRef(y));
                }
                else
                {
                    chunks.Add(GetRulePropertyRef(x, y));
                }
                break;
            case AttributeDict.DictType.TOKEN:
                chunks.Add(GetTokenPropertyRef(x, y));
                break;
            }
        }

        public virtual void SetAttr(string expr, IToken x, IToken rhs)
        {
            gen.g.tool.Log("action-translator", "setAttr " + x + " " + rhs);
            IList<ActionChunk> rhsChunks = TranslateActionChunk(factory, rf, rhs.Text, node);
            SetAttr s = new SetAttr(nodeContext, x.Text, rhsChunks);
            chunks.Add(s);
        }

        public virtual void NonLocalAttr(string expr, IToken x, IToken y)
        {
            gen.g.tool.Log("action-translator", "nonLocalAttr " + x + "::" + y);
            Rule r = factory.GetGrammar().GetRule(x.Text);
            chunks.Add(new NonLocalAttrRef(nodeContext, x.Text, y.Text, r.index));
        }

        public virtual void SetNonLocalAttr(string expr, IToken x, IToken y, IToken rhs)
        {
            gen.g.tool.Log("action-translator", "setNonLocalAttr " + x + "::" + y + "=" + rhs);
            Rule r = factory.GetGrammar().GetRule(x.Text);
            IList<ActionChunk> rhsChunks = TranslateActionChunk(factory, rf, rhs.Text, node);
            SetNonLocalAttr s = new SetNonLocalAttr(nodeContext, x.Text, y.Text, r.index, rhsChunks);
            chunks.Add(s);
        }

        public virtual void Text(string text)
        {
            chunks.Add(new ActionText(nodeContext, text));
        }

        internal virtual TokenPropertyRef GetTokenPropertyRef(IToken x, IToken y)
        {
            try
            {
                System.Func<StructDecl, string, TokenPropertyRef> c = tokenPropToModelMap[y.Text];
                TokenPropertyRef @ref =
                    c(nodeContext, GetTokenLabel(x.Text));
                return @ref;
            }
            catch (Exception e)
            {
                factory.GetGrammar().tool.errMgr.ToolError(ErrorType.INTERNAL_ERROR, e);
            }
            return null;
        }

        // $text
        internal virtual RulePropertyRef GetRulePropertyRef(IToken prop)
        {
            try
            {
                System.Func<StructDecl, string, RulePropertyRef> c = thisRulePropToModelMap[prop.Text];
                RulePropertyRef @ref =
                    c(nodeContext, GetRuleLabel(prop.Text));
                return @ref;
            }
            catch (Exception e)
            {
                factory.GetGrammar().tool.errMgr.ToolError(ErrorType.INTERNAL_ERROR, e);
            }
            return null;
        }

        internal virtual RulePropertyRef GetRulePropertyRef(IToken x, IToken prop)
        {
            Grammar g = factory.GetGrammar();
            try
            {
                System.Func<StructDecl, string, RulePropertyRef> c = rulePropToModelMap[prop.Text];
                RulePropertyRef @ref =
                    c(nodeContext, GetRuleLabel(x.Text));
                return @ref;
            }
            catch (Exception e)
            {
                g.tool.errMgr.ToolError(ErrorType.INTERNAL_ERROR, e, prop.Text);
            }
            return null;
        }

        public virtual string GetTokenLabel(string x)
        {
            if (node.resolver.ResolvesToLabel(x, node))
                return x;
            return factory.GetTarget().GetImplicitTokenLabel(x);
        }

        public virtual string GetRuleLabel(string x)
        {
            if (node.resolver.ResolvesToLabel(x, node))
                return x;
            return factory.GetTarget().GetImplicitRuleLabel(x);
        }
    }
}
