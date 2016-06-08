// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    /// <summary>
    /// A tree that knows about an interval in a token stream
    /// is some kind of syntax tree.
    /// </summary>
    /// <remarks>
    /// A tree that knows about an interval in a token stream
    /// is some kind of syntax tree. Subinterfaces distinguish
    /// between parse trees and other kinds of syntax trees we might want to create.
    /// </remarks>
    public interface ISyntaxTree : ITree
    {
        /// <summary>
        /// Return an
        /// <see cref="Antlr4.Runtime.Misc.Interval"/>
        /// indicating the index in the
        /// <see cref="Antlr4.Runtime.ITokenStream"/>
        /// of the first and last token associated with this
        /// subtree. If this node is a leaf, then the interval represents a single
        /// token.
        /// <p>If source interval is unknown, this returns
        /// <see cref="Antlr4.Runtime.Misc.Interval.Invalid"/>
        /// .</p>
        /// </summary>
        Interval SourceInterval
        {
            get;
        }
    }
}
