// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Tool.Ast;

    /** */
    public abstract class SrcOp : OutputModelObject
    {
        /** Used to create unique var names etc... */
        public int uniqueID; // TODO: do we need?

        /** All operations know in which block they live:
         *
         *  	CodeBlock, CodeBlockForAlt
         *
         *  Templates might need to know block nesting level or find
         *  a specific declaration, etc...
         */
        public CodeBlock enclosingBlock;

        public RuleFunction enclosingRuleRunction;

        protected SrcOp(OutputModelFactory factory)
            : this(factory, null)
        {
        }

        protected SrcOp(OutputModelFactory factory, GrammarAST ast)
            : base(factory, ast)
        {
            if (ast != null)
                uniqueID = ast.Token.TokenIndex;
            enclosingBlock = factory.GetCurrentBlock();
            enclosingRuleRunction = factory.GetCurrentRuleFunction();
        }

        /** Walk upwards in model tree, looking for outer alt's code block */
        public virtual CodeBlockForOuterMostAlt GetOuterMostAltCodeBlock()
        {
            if (this is CodeBlockForOuterMostAlt)
            {
                return (CodeBlockForOuterMostAlt)this;
            }
            CodeBlock p = enclosingBlock;
            while (p != null)
            {
                if (p is CodeBlockForOuterMostAlt)
                {
                    return (CodeBlockForOuterMostAlt)p;
                }
                p = p.enclosingBlock;
            }
            return null;
        }

        /** Return label alt or return name of rule */
        public virtual string GetContextName()
        {
            CodeBlockForOuterMostAlt alt = GetOuterMostAltCodeBlock();
            if (alt != null && alt.altLabel != null)
                return alt.altLabel;
            return enclosingRuleRunction.name;
        }
    }
}
