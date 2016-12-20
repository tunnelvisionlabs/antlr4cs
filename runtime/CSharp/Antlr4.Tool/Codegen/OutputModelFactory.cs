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
