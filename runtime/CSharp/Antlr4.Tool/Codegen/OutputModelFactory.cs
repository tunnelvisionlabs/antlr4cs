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

    public interface OutputModelFactory
    {
        Grammar GetGrammar();

        [return: NotNull]
        CodeGenerator GetGenerator();

        [return: NotNull]
        AbstractTarget GetTarget();

        void SetController(OutputModelController controller);

        OutputModelController GetController();

        ParserFile ParserFile(string fileName);

        Parser Parser(ParserFile file);

        LexerFile LexerFile(string fileName);

        Lexer Lexer(LexerFile file);

        RuleFunction Rule(Rule r);

        IList<SrcOp> RulePostamble(RuleFunction function, Rule r);

        // ELEMENT TRIGGERS

        CodeBlockForAlt Alternative(Alternative alt, bool outerMost);

        CodeBlockForAlt FinishAlternative(CodeBlockForAlt blk, IList<SrcOp> ops);

        CodeBlockForAlt Epsilon(Alternative alt, bool outerMost);

        IList<SrcOp> RuleRef(GrammarAST ID, GrammarAST label, GrammarAST args);

        IList<SrcOp> TokenRef(GrammarAST ID, GrammarAST label, GrammarAST args);

        IList<SrcOp> StringRef(GrammarAST ID, GrammarAST label);

        IList<SrcOp> Set(GrammarAST setAST, GrammarAST label, bool invert);

        IList<SrcOp> Wildcard(GrammarAST ast, GrammarAST labelAST);

        IList<SrcOp> Action(ActionAST ast);

        IList<SrcOp> Sempred(ActionAST ast);

        Choice GetChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts, GrammarAST label);

        Choice GetEBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts);

        Choice GetLL1ChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts);

        Choice GetComplexChoiceBlock(BlockAST blkAST, IList<CodeBlockForAlt> alts);

        Choice GetLL1EBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts);

        Choice GetComplexEBNFBlock(GrammarAST ebnfRoot, IList<CodeBlockForAlt> alts);

        IList<SrcOp> GetLL1Test(IntervalSet look, GrammarAST blkAST);

        bool NeedsImplicitLabel(GrammarAST ID, LabeledOp op);

        // CONTEXT INFO

        OutputModelObject GetRoot();

        RuleFunction GetCurrentRuleFunction();

        Alternative GetCurrentOuterMostAlt();

        CodeBlock GetCurrentBlock();

        CodeBlockForOuterMostAlt GetCurrentOuterMostAlternativeBlock();

        int GetCodeBlockLevel();

        int GetTreeLevel();
    }
}
