// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Runtime.Sharpen
{
    using System;

    internal static class Runtime
    {
        public static string Substring(string str, int beginOffset, int endOffset)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            return str.Substring(beginOffset, endOffset - beginOffset);
        }
    }
}
