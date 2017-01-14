// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using Antlr4.Runtime;

    /** An {@link InterpreterRuleContext} that knows which alternative
     *  for a rule was matched.
     *
     *  @see GrammarParserInterpreter
     *  @since 4.5.1
     */
    public class GrammarInterpreterRuleContext : InterpreterRuleContext
    {
        protected int outerAltNum = 1;

        public GrammarInterpreterRuleContext(ParserRuleContext parent, int invokingStateNumber, int ruleIndex)
            : base(parent, invokingStateNumber, ruleIndex)
        {
        }

        /** The predicted outermost alternative for the rule associated
         *  with this context object.  If this node left recursive, the true original
         *  outermost alternative is returned.
         */
        public override int OuterAlternative
        {
            get
            {
                return outerAltNum;
            }

            set
            {
                outerAltNum = value;
            }
        }
    }
}
