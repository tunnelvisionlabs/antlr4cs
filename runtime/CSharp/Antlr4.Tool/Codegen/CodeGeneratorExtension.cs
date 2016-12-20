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
    using Antlr4.Codegen.Model;
    using Antlr4.Tool.Ast;

    /** Filter list of SrcOps and return; default is pass-through filter */
    public class CodeGeneratorExtension
    {
        public OutputModelFactory factory;

        public CodeGeneratorExtension(OutputModelFactory factory)
        {
            this.factory = factory;
        }

        public virtual ParserFile ParserFile(ParserFile f)
        {
            return f;
        }

        public virtual Parser Parser(Parser p)
        {
            return p;
        }

        public virtual LexerFile LexerFile(LexerFile f)
        {
            return f;
        }

        public virtual Lexer Lexer(Lexer l)
        {
            return l;
        }

        public virtual RuleFunction Rule(RuleFunction rf)
        {
            return rf;
        }

        public virtual IList<SrcOp> RulePostamble(IList<SrcOp> ops)
        {
            return ops;
        }

        public virtual CodeBlockForAlt Alternative(CodeBlockForAlt blk, bool outerMost)
        {
            return blk;
        }

        public virtual CodeBlockForAlt FinishAlternative(CodeBlockForAlt blk, bool outerMost)
        {
            return blk;
        }

        public virtual CodeBlockForAlt Epsilon(CodeBlockForAlt blk)
        {
            return blk;
        }

        public virtual IList<SrcOp> RuleRef(IList<SrcOp> ops)
        {
            return ops;
        }

        public virtual IList<SrcOp> TokenRef(IList<SrcOp> ops)
        {
            return ops;
        }

        public virtual IList<SrcOp> Set(IList<SrcOp> ops)
        {
            return ops;
        }

        public virtual IList<SrcOp> StringRef(IList<SrcOp> ops)
        {
            return ops;
        }

        public virtual IList<SrcOp> Wildcard(IList<SrcOp> ops)
        {
            return ops;
        }

        // ACTIONS

        public virtual IList<SrcOp> Action(IList<SrcOp> ops)
        {
            return ops;
        }

        public virtual IList<SrcOp> Sempred(IList<SrcOp> ops)
        {
            return ops;
        }

        // BLOCKS

        public virtual Choice GetChoiceBlock(Choice c)
        {
            return c;
        }

        public virtual Choice GetEBNFBlock(Choice c)
        {
            return c;
        }

        public virtual bool NeedsImplicitLabel(GrammarAST ID, LabeledOp op)
        {
            return false;
        }
    }
}
