// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Parse
{
    using Exception = System.Exception;

    /** Used to throw us out of deeply nested element back to end of a rule's
     *  alt list. Note it's not under RecognitionException.
     */
    public class ResyncToEndOfRuleBlock : Exception
    {
        public ResyncToEndOfRuleBlock()
        {
        }

        public ResyncToEndOfRuleBlock(string message)
            : base(message)
        {
        }

        public ResyncToEndOfRuleBlock(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
