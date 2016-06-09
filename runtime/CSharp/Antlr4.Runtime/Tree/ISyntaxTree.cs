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
        /// token and has interval i..i for token index i.
        /// <p>An interval of i..i-1 indicates an empty interval at position
        /// i in the input stream, where 0 &lt;= i &lt;= the size of the input
        /// token stream.  Currently, the code base can only have i=0..n-1 but
        /// in concept one could have an empty interval after EOF. </p>
        /// <p>If source interval is unknown, this returns
        /// <see cref="Antlr4.Runtime.Misc.Interval.Invalid"/>
        /// .</p>
        /// <p>As a weird special case, the source interval for rules matched after
        /// EOF is unspecified.</p>
        /// </summary>
        Interval SourceInterval
        {
            get;
        }
    }
}
