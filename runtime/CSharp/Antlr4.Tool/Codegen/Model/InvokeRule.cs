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

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Codegen.Model.Chunk;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;

    /** */
    public class InvokeRule : RuleElement, LabeledOp
    {
        public string name;
        public OrderedHashSet<Decl.Decl> labels = new OrderedHashSet<Decl.Decl>(); // TODO: should need just 1
        public string ctxName;

        [ModelElement]
        public IList<ActionChunk> argExprsChunks;

        public InvokeRule(ParserFactory factory, GrammarAST ast, GrammarAST labelAST)
            : base(factory, ast)
        {
            if (ast.atnState != null)
            {
                RuleTransition ruleTrans = (RuleTransition)ast.atnState.Transition(0);
                stateNumber = ast.atnState.stateNumber;
            }

            this.name = ast.Text;
            Rule r = factory.GetGrammar().GetRule(name);
            ctxName = factory.GetTarget().GetRuleFunctionContextStructName(r);

            // TODO: move to factory
            RuleFunction rf = factory.GetCurrentRuleFunction();
            if (labelAST != null)
            {
                // for x=r, define <rule-context-type> x and list_x
                string label = labelAST.Text;
                if (labelAST.Parent.Type == ANTLRParser.PLUS_ASSIGN)
                {
                    factory.DefineImplicitLabel(ast, this);
                    string listLabel = factory.GetTarget().GetListLabel(label);
                    RuleContextListDecl l = new RuleContextListDecl(factory, listLabel, ctxName);
                    rf.AddContextDecl(ast.GetAltLabel(), l);
                }
                else
                {
                    RuleContextDecl d = new RuleContextDecl(factory, label, ctxName);
                    labels.Add(d);
                    rf.AddContextDecl(ast.GetAltLabel(), d);
                }
            }

            ActionAST arg = (ActionAST)ast.GetFirstChildWithType(ANTLRParser.ARG_ACTION);
            if (arg != null)
            {
                argExprsChunks = ActionTranslator.TranslateAction(factory, rf, arg.Token, arg);
            }

            // If action refs rule as rulename not label, we need to define implicit label
            if (factory.GetCurrentOuterMostAlt().ruleRefsInActions.ContainsKey(ast.Text))
            {
                string label = factory.GetTarget().GetImplicitRuleLabel(ast.Text);
                RuleContextDecl d = new RuleContextDecl(factory, label, ctxName);
                labels.Add(d);
                rf.AddContextDecl(ast.GetAltLabel(), d);
            }
        }

        public virtual IList<Decl.Decl> GetLabels()
        {
            return labels.Elements;
        }
    }
}
