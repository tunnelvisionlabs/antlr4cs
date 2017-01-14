// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Parse
{
    using IToken = Antlr.Runtime.IToken;
    using ParseCanceledException = Antlr4.Runtime.Misc.ParseCanceledException;

    public class v3TreeGrammarException : ParseCanceledException
    {
        public IToken location;

        public v3TreeGrammarException(IToken location)
        {
            this.location = location;
        }
    }
}
