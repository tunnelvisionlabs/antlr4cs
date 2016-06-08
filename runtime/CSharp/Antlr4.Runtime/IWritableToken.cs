// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    public interface IWritableToken : IToken
    {
        new string Text
        {
            set;
        }

        new int Type
        {
            set;
        }

        new int Line
        {
            set;
        }

        new int Column
        {
            set;
        }

        new int Channel
        {
            set;
        }

        new int TokenIndex
        {
            set;
        }
    }
}
