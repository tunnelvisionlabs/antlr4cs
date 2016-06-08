// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    public interface ITerminalNode : IParseTree
    {
        IToken Symbol
        {
            get;
        }

        new IRuleNode Parent
        {
            get;
        }
    }
}
