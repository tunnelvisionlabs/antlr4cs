// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
                        if (altLabel != null && altLabelCtxs.TryGetValue(altLabel, out s) && s != null)
                            @struct = s; // if alt label, use subctx
                    }

                    @struct.AddDecl(d); // stick in overall rule's ctx
                }
            }
        }
    }
}
