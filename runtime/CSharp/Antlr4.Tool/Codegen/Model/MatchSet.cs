// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;

    public class MatchSet : MatchToken
    {
        [ModelElement]
        public TestSetInline expr;
        [ModelElement]
        public CaptureNextTokenType capture;

        public MatchSet(OutputModelFactory factory, GrammarAST ast)
            : base(factory, ast)
        {
            SetTransition st = (SetTransition)ast.atnState.Transition(0);
            int wordSize = factory.GetGenerator().GetTarget().GetInlineTestSetWordSize();
            expr = new TestSetInline(factory, null, st.set, wordSize);
            Decl.Decl d = new TokenTypeDecl(factory, expr.varName);
            factory.GetCurrentRuleFunction().AddLocalDecl(d);
            capture = new CaptureNextTokenType(factory, expr.varName);
        }
    }
}
