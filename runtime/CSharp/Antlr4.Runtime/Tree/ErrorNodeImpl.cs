// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    /// <summary>
    /// Represents a token that was consumed during resynchronization
    /// rather than during a valid match operation.
    /// </summary>
    /// <remarks>
    /// Represents a token that was consumed during resynchronization
    /// rather than during a valid match operation. For example,
    /// we will create this kind of a node during single token insertion
    /// and deletion as well as during "consume until error recovery set"
    /// upon no viable alternative exceptions.
    /// </remarks>
    public class ErrorNodeImpl : TerminalNodeImpl, IErrorNode
    {
        public ErrorNodeImpl(IToken token)
            : base(token)
        {
        }

        public override T Accept<T>(IParseTreeVisitor<T> visitor)
        {
            return visitor.VisitErrorNode(this);
        }
    }
}
