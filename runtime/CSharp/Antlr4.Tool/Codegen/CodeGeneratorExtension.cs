// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
