// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Antlr4.Codegen.Model.Chunk;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool.Ast;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

    /** */
    public class SemPred : Action
    {
        /**
         * The user-specified terminal option {@code fail}, if it was used and the
         * value is a string literal. For example:
         *
         * <p>
         * {@code {pred}?&lt;fail='message'>}</p>
         */
        public string msg;
        /**
         * The predicate string with <code>{</code> and <code>}?</code> stripped from the ends.
         */
        public string predicate;

        /**
         * The translated chunks of the user-specified terminal option {@code fail},
         * if it was used and the value is an action. For example:
         *
         * <p>
         * {@code {pred}?&lt;fail={"Java literal"}>}</p>
         */
        [ModelElement]
        public IList<ActionChunk> failChunks;

        public SemPred(OutputModelFactory factory, [NotNull] ActionAST ast)
            : base(factory, ast)
        {

            Debug.Assert(ast.atnState != null
                && ast.atnState.NumberOfTransitions == 1
                && ast.atnState.Transition(0) is AbstractPredicateTransition);

            GrammarAST failNode = ast.GetOptionAST("fail");
            predicate = ast.Text;
            if (predicate.StartsWith("{") && predicate.EndsWith("}?"))
            {
                predicate = predicate.Substring(1, predicate.Length - 3);
            }
            predicate = factory.GetTarget().GetTargetStringLiteralFromString(predicate);

            if (failNode == null)
                return;

            if (failNode is ActionAST)
            {
                ActionAST failActionNode = (ActionAST)failNode;
                RuleFunction rf = factory.GetCurrentRuleFunction();
                failChunks = ActionTranslator.TranslateAction(factory, rf,
                                                              failActionNode.Token,
                                                              failActionNode);
            }
            else
            {
                msg = factory.GetTarget().GetTargetStringLiteralFromANTLRStringLiteral(factory.GetGenerator(),
                                                                              failNode.Text,
                                                                              true);
            }
        }
    }
}
