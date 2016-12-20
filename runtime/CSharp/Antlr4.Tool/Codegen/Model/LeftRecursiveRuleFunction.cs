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
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;

    public class LeftRecursiveRuleFunction : RuleFunction
    {
        public LeftRecursiveRuleFunction(OutputModelFactory factory, LeftRecursiveRule r)
            : base(factory, r)
        {
            // Since we delete x=lr, we have to manually add decls for all labels
            // on left-recur refs to proper structs
            foreach (System.Tuple<GrammarAST, string> pair in r.leftRecursiveRuleRefLabels)
            {
                GrammarAST idAST = pair.Item1;
                string altLabel = pair.Item2;
                string label = idAST.Text;
                GrammarAST rrefAST = (GrammarAST)idAST.Parent.GetChild(1);
                if (rrefAST.Type == ANTLRParser.RULE_REF)
                {
                    Rule targetRule = factory.GetGrammar().GetRule(rrefAST.Text);
                    string ctxName = factory.GetTarget().GetRuleFunctionContextStructName(targetRule);
                    RuleContextDecl d;
                    if (idAST.Parent.Type == ANTLRParser.ASSIGN)
                    {
                        d = new RuleContextDecl(factory, label, ctxName);
                    }
                    else
                    {
                        d = new RuleContextListDecl(factory, label, ctxName);
                    }

                    StructDecl @struct = ruleCtx;
                    if (altLabelCtxs != null)
                    {
                        AltLabelStructDecl s;
                        if (altLabelCtxs.TryGetValue(altLabel, out s) && s != null)
                            @struct = s; // if alt label, use subctx
                    }

                    @struct.AddDecl(d); // stick in overall rule's ctx
                }
            }
        }
    }
}
