// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr.Runtime;
    using Antlr4.Codegen.Model.Chunk;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Parse;
    using Antlr4.StringTemplate;
    using Antlr4.Tool.Ast;

    /** */
    public class Action : RuleElement
    {
        [ModelElement]
        public IList<ActionChunk> chunks;

        public Action(OutputModelFactory factory, ActionAST ast)
            : base(factory, ast)
        {
            RuleFunction rf = factory.GetCurrentRuleFunction();
            if (ast != null)
            {
                chunks = ActionTranslator.TranslateAction(factory, rf, ast.Token, ast);
            }
            else
            {
                chunks = new List<ActionChunk>();
            }
            //System.out.println("actions="+chunks);
        }

        public Action(OutputModelFactory factory, StructDecl ctx, string action)
            : base(factory, null)
        {
            ActionAST ast = new ActionAST(new CommonToken(ANTLRParser.ACTION, action));
            RuleFunction rf = factory.GetCurrentRuleFunction();
            if (rf != null)
            { // we can translate
                ast.resolver = rf.rule;
                chunks = ActionTranslator.TranslateActionChunk(factory, rf, action, ast);
            }
            else
            {
                chunks = new List<ActionChunk>();
                chunks.Add(new ActionText(ctx, action));
            }
        }

        public Action(OutputModelFactory factory, StructDecl ctx, Template actionST)
            : base(factory, null)
        {
            chunks = new List<ActionChunk>();
            chunks.Add(new ActionTemplate(ctx, actionST));
        }
    }
}
