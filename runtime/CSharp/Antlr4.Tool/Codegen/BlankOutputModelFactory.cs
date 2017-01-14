// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using Antlr4.Codegen.Model;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

    public abstract class BlankOutputModelFactory : OutputModelFactory
    {
        public abstract Grammar GetGrammar();

        [return: NotNull]
        public abstract CodeGenerator GetGenerator();

        [return: NotNull]
        public abstract AbstractTarget GetTarget();

        public abstract void SetController(OutputModelController controller);

        public abstract OutputModelController GetController();

        public abstract OutputModelObject GetRoot();

        public abstract RuleFunction GetCurrentRuleFunction();

        public abstract Alternative GetCurrentOuterMostAlt();

        public abstract CodeBlock GetCurrentBlock();

        public abstract CodeBlockForOuterMostAlt GetCurrentOuterMostAlternativeBlock();

        public abstract int GetCodeBlockLevel();

        public abstract int GetTreeLevel();

        public virtual ParserFile ParserFile(string fileName)
        {
            return null;
        }

        public virtual Parser Parser(ParserFile file)
        {
            return null;
        }

        public virtual RuleFunction Rule(Rule r)
        {
            return null;
        }

        public virtual IList<SrcOp> RulePostamble(RuleFunction function, Rule r)
        {
            return null;
        }

        public virtual LexerFile LexerFile(string fileName)
        {
            return null;
        }

        public virtual Lexer Lexer(LexerFile file)
        {
            return null;
        }

        // ALTERNATIVES / ELEMENTS

        public virtual CodeBlockForAlt Alternative(Alternative alt, bool outerMost)
        {
            return null;
        }

        public virtual CodeBlockForAlt FinishAlternative(CodeBlockForAlt blk, IList<SrcOp> ops)
        {
            return blk;
        }

        public virtual CodeBlockForAlt Epsilon(Alternative alt, bool outerMost)
        {
            return null;
        }

        public virtual IList<SrcOp> RuleRef(GrammarAST ID, GrammarAST label, GrammarAST args)
        {
            return null;
        }

        public virtual IList<SrcOp> TokenRef(GrammarAST ID, GrammarAST label, GrammarAST args)
        {
            return null;
        }

        public virtual IList<SrcOp> StringRef(GrammarAST ID, GrammarAST label)
        {
            return TokenRef(ID, label, null);
        }

        public virtual IList<SrcOp> Set(GrammarAST setAST, GrammarAST label, bool invert)
        {
            return null;
        }

        public virtual IList<SrcOp> Wildcard(GrammarAST ast, GrammarAST labelAST)
        {
            return null;
        }

        // ACTIONS

        public virtual IList<SrcOp> Action(ActionAST ast)
        {
            return null;
        }

        public virtual IList<SrcOp> Sempred(ActionAST ast)
        {
            return null;
        }

        // BLOCKS

        public virtual Choice GetChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts, GrammarAST label)
        {
            return null;
        }

        public virtual Choice GetEBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts)
        {
            return null;
        }

        public virtual Choice GetLL1ChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts)
        {
            return null;
        }

        public virtual Choice GetComplexChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts)
        {
            return null;
        }

        public virtual Choice GetLL1EBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts)
        {
            return null;
        }

        public virtual Choice GetComplexEBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts)
        {
            return null;
        }

        public virtual IList<SrcOp> GetLL1Test(IntervalSet look, GrammarAST blkAST)
        {
            return null;
        }

        public virtual bool NeedsImplicitLabel(GrammarAST ID, LabeledOp op)
        {
            return false;
        }
    }
}
