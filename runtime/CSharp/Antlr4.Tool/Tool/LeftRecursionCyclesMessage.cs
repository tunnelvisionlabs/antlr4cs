// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using System.Collections.Generic;
    using IToken = Antlr.Runtime.IToken;

    public class LeftRecursionCyclesMessage : ANTLRMessage
    {
        public LeftRecursionCyclesMessage(string fileName, IEnumerable<IEnumerable<Rule>> cycles)
            : base(ErrorType.LEFT_RECURSION_CYCLES, GetStartTokenOfFirstRule(cycles), cycles)
        {
            this.fileName = fileName;
        }

        protected static IToken GetStartTokenOfFirstRule(IEnumerable<IEnumerable<Rule>> cycles)
        {
            if (cycles == null)
            {
                return null;
            }

            foreach (IEnumerable<Rule> collection in cycles)
            {
                if (collection == null)
                {
                    return null;
                }

                foreach (Rule rule in collection)
                {
                    if (rule.ast != null)
                    {
                        return rule.ast.Token;
                    }
                }
            }
            return null;
        }
    }
}
