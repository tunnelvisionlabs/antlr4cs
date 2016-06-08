// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections.Generic;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;

namespace Antlr4.Runtime.Tree.Xpath
{
    public class XPathWildcardElement : XPathElement
    {
        public XPathWildcardElement()
            : base(XPath.Wildcard)
        {
        }

        public override ICollection<IParseTree> Evaluate(IParseTree t)
        {
            if (invert)
            {
                return new List<IParseTree>();
            }
            // !* is weird but valid (empty)
            IList<IParseTree> kids = new List<IParseTree>();
            foreach (ITree c in Trees.GetChildren(t))
            {
                kids.Add((IParseTree)c);
            }
            return kids;
        }
    }
}
