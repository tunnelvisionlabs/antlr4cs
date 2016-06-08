// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree.Pattern
{
    /// <summary>
    /// A chunk is either a token tag, a rule tag, or a span of literal text within a
    /// tree pattern.
    /// </summary>
    /// <remarks>
    /// A chunk is either a token tag, a rule tag, or a span of literal text within a
    /// tree pattern.
    /// <p>The method
    /// <see cref="ParseTreePatternMatcher.Split(string)"/>
    /// returns a list of
    /// chunks in preparation for creating a token stream by
    /// <see cref="ParseTreePatternMatcher.Tokenize(string)"/>
    /// . From there, we get a parse
    /// tree from with
    /// <see cref="ParseTreePatternMatcher.Compile(string, int)"/>
    /// . These
    /// chunks are converted to
    /// <see cref="RuleTagToken"/>
    /// ,
    /// <see cref="TokenTagToken"/>
    /// , or the
    /// regular tokens of the text surrounding the tags.</p>
    /// </remarks>
    internal abstract class Chunk
    {
    }
}
