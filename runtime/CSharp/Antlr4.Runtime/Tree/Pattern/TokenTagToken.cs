// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree.Pattern
{
    /// <summary>
    /// A
    /// <see cref="Antlr4.Runtime.IToken"/>
    /// object representing a token of a particular type; e.g.,
    /// <c>&lt;ID&gt;</c>
    /// . These tokens are created for
    /// <see cref="TagChunk"/>
    /// chunks where the
    /// tag corresponds to a lexer rule or token type.
    /// </summary>
    [System.Serializable]
    public class TokenTagToken : CommonToken
    {
        /// <summary>
        /// This is the backing field for
        /// <see cref="TokenName()"/>
        /// .
        /// </summary>
        [NotNull]
        private readonly string tokenName;

        /// <summary>
        /// This is the backing field for
        /// <see cref="Label()"/>
        /// .
        /// </summary>
        [Nullable]
        private readonly string label;

        /// <summary>
        /// Constructs a new instance of
        /// <see cref="TokenTagToken"/>
        /// for an unlabeled tag
        /// with the specified token name and type.
        /// </summary>
        /// <param name="tokenName">The token name.</param>
        /// <param name="type">The token type.</param>
        public TokenTagToken([NotNull] string tokenName, int type)
            : this(tokenName, type, null)
        {
        }

        /// <summary>
        /// Constructs a new instance of
        /// <see cref="TokenTagToken"/>
        /// with the specified
        /// token name, type, and label.
        /// </summary>
        /// <param name="tokenName">The token name.</param>
        /// <param name="type">The token type.</param>
        /// <param name="label">
        /// The label associated with the token tag, or
        /// <see langword="null"/>
        /// if
        /// the token tag is unlabeled.
        /// </param>
        public TokenTagToken([NotNull] string tokenName, int type, [Nullable] string label)
            : base(type)
        {
            this.tokenName = tokenName;
            this.label = label;
        }

        /// <summary>Gets the token name.</summary>
        /// <returns>The token name.</returns>
        public string TokenName
        {
            get
            {
                return tokenName;
            }
        }

        /// <summary>Gets the label associated with the rule tag.</summary>
        /// <returns>
        /// The name of the label associated with the rule tag, or
        /// <see langword="null"/>
        /// if this is an unlabeled rule tag.
        /// </returns>
        public string Label
        {
            get
            {
                return label;
            }
        }

        /// <summary>
        /// <inheritDoc/>
        /// <p>The implementation for
        /// <see cref="TokenTagToken"/>
        /// returns the token tag
        /// formatted with
        /// <c>&lt;</c>
        /// and
        /// <c>&gt;</c>
        /// delimiters.</p>
        /// </summary>
        public override string Text
        {
            get
            {
                if (label != null)
                {
                    return "<" + label + ":" + tokenName + ">";
                }
                return "<" + tokenName + ">";
            }
        }

        /// <summary>
        /// <inheritDoc/>
        /// <p>The implementation for
        /// <see cref="TokenTagToken"/>
        /// returns a string of the form
        /// <c>tokenName:type</c>
        /// .</p>
        /// </summary>
        public override string ToString()
        {
            return tokenName + ":" + type;
        }
    }
}
