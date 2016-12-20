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
