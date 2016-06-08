// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections.Generic;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;

namespace Antlr4.Runtime.Tree.Xpath
{
    /// <summary>
    /// Either
    /// <c>ID</c>
    /// at start of path or
    /// <c>...//ID</c>
    /// in middle of path.
    /// </summary>
    public class XPathRuleAnywhereElement : XPathElement
    {
        protected internal int ruleIndex;

        public XPathRuleAnywhereElement(string ruleName, int ruleIndex)
            : base(ruleName)
        {
            this.ruleIndex = ruleIndex;
        }

        public override ICollection<IParseTree> Evaluate(IParseTree t)
        {
            return Trees.FindAllRuleNodes(t, ruleIndex);
        }
    }
}
