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
