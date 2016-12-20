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
    using Antlr4.Tool.Ast;

    /** An outermost alternative for a rule.  We don't track inner alternatives. */
    public class Alternative : AttributeResolver
    {
        public Rule rule;

        public AltAST ast;

        /** What alternative number is this outermost alt? 1..n */
        public int altNum;

        // token IDs, string literals in this alt
        public Runtime.Misc.MultiMap<string, TerminalAST> tokenRefs = new Runtime.Misc.MultiMap<string, TerminalAST>();

        // does not include labels
        public Runtime.Misc.MultiMap<string, GrammarAST> tokenRefsInActions = new Runtime.Misc.MultiMap<string, GrammarAST>();

        // all rule refs in this alt
        public Runtime.Misc.MultiMap<string, GrammarAST> ruleRefs = new Runtime.Misc.MultiMap<string, GrammarAST>();

        // does not include labels
        public Runtime.Misc.MultiMap<string, GrammarAST> ruleRefsInActions = new Runtime.Misc.MultiMap<string, GrammarAST>();

        /** A list of all LabelElementPair attached to tokens like id=ID, ids+=ID */
        public Runtime.Misc.MultiMap<string, LabelElementPair> labelDefs = new Runtime.Misc.MultiMap<string, LabelElementPair>();

        // track all token, rule, label refs in rewrite (right of ->)
        //public List<GrammarAST> rewriteElements = new ArrayList<GrammarAST>();

        /** Track all executable actions other than named actions like @init
         *  and catch/finally (not in an alt). Also tracks predicates, rewrite actions.
         *  We need to examine these actions before code generation so
         *  that we can detect refs to $rule.attr etc...
         *
         *  This tracks per alt
         */
        public IList<ActionAST> actions = new List<ActionAST>();

        public Alternative(Rule r, int altNum)
        {
            this.rule = r;
            this.altNum = altNum;
        }

        public virtual bool ResolvesToToken(string x, ActionAST node)
        {
            if (tokenRefs.ContainsKey(x) && tokenRefs[x] != null)
                return true;

            LabelElementPair anyLabelDef = GetAnyLabelDef(x);
            if (anyLabelDef != null && anyLabelDef.type == LabelType.TOKEN_LABEL)
                return true;

            return false;
        }

        public virtual bool ResolvesToAttributeDict(string x, ActionAST node)
        {
            if (ResolvesToToken(x, node))
                return true;
            if (ruleRefs.ContainsKey(x) && ruleRefs[x] != null)
                return true; // rule ref in this alt?
            LabelElementPair anyLabelDef = GetAnyLabelDef(x);
            if (anyLabelDef != null && anyLabelDef.type == LabelType.RULE_LABEL)
                return true;
            return false;
        }

        /**  $x		Attribute: rule arguments, return values, predefined rule prop.
         */
        public virtual Attribute ResolveToAttribute(string x, ActionAST node)
        {
            return rule.ResolveToAttribute(x, node); // reuse that code
        }

        /** $x.y, x can be surrounding rule, token/rule/label ref. y is visible
         *  attr in that dictionary.  Can't see args on rule refs.
         */
        public virtual Attribute ResolveToAttribute(string x, string y, ActionAST node)
        {
            if (tokenRefs.ContainsKey(x) && tokenRefs[x] != null)
            {
                // token ref in this alt?
                return rule.GetPredefinedScope(LabelType.TOKEN_LABEL).Get(y);
            }

            if (ruleRefs.ContainsKey(x) && ruleRefs[x] != null)
            {
                // rule ref in this alt?
                // look up rule, ask it to resolve y (must be retval or predefined)
                return rule.g.GetRule(x).ResolveRetvalOrProperty(y);
            }

            LabelElementPair anyLabelDef = GetAnyLabelDef(x);
            if (anyLabelDef != null && anyLabelDef.type == LabelType.RULE_LABEL)
            {
                return rule.g.GetRule(anyLabelDef.element.Text).ResolveRetvalOrProperty(y);
            }
            else if (anyLabelDef != null)
            {
                AttributeDict scope = rule.GetPredefinedScope(anyLabelDef.type);
                if (scope == null)
                {
                    return null;
                }

                return scope.Get(y);
            }
            return null;
        }

        public virtual bool ResolvesToLabel(string x, ActionAST node)
        {
            LabelElementPair anyLabelDef = GetAnyLabelDef(x);
            return anyLabelDef != null &&
                   (anyLabelDef.type == LabelType.TOKEN_LABEL ||
                    anyLabelDef.type == LabelType.RULE_LABEL);
        }

        public virtual bool ResolvesToListLabel(string x, ActionAST node)
        {
            LabelElementPair anyLabelDef = GetAnyLabelDef(x);
            return anyLabelDef != null &&
                   (anyLabelDef.type == LabelType.RULE_LIST_LABEL ||
                    anyLabelDef.type == LabelType.TOKEN_LIST_LABEL);
        }

        public virtual LabelElementPair GetAnyLabelDef(string x)
        {
            IList<LabelElementPair> labels;
            if (labelDefs.TryGetValue(x, out labels) && labels != null)
                return labels[0];

            return null;
        }

        /** x can be ruleref or rule label. */
        public virtual Rule ResolveToRule(string x)
        {
            if (ruleRefs.ContainsKey(x) && ruleRefs[x] != null)
                return rule.g.GetRule(x);

            LabelElementPair anyLabelDef = GetAnyLabelDef(x);
            if (anyLabelDef != null && anyLabelDef.type == LabelType.RULE_LABEL)
            {
                return rule.g.GetRule(anyLabelDef.element.Text);
            }

            return null;
        }
    }
}
