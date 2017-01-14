// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Parse
{
    using IToken = Antlr.Runtime.IToken;

    /** */
    public interface ActionSplitterListener
    {
        void QualifiedAttr(string expr, IToken x, IToken y);
        void SetAttr(string expr, IToken x, IToken rhs);
        void Attr(string expr, IToken x);

        void SetNonLocalAttr(string expr, IToken x, IToken y, IToken rhs);
        void NonLocalAttr(string expr, IToken x, IToken y);

        void Text(string text);
    }
}
