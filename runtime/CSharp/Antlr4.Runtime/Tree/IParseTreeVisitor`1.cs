// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    /// <summary>This interface defines the basic notion of a parse tree visitor.</summary>
    /// <remarks>
    /// This interface defines the basic notion of a parse tree visitor. Generated
    /// visitors implement this interface and the
    /// <c>XVisitor</c>
    /// interface for
    /// grammar
    /// <c>X</c>
    /// .
    /// </remarks>
    /// <author>Sam Harwell</author>
#if COMPACT
    public interface IParseTreeVisitor<Result>
#else
    public interface IParseTreeVisitor<out Result>
#endif
    {
        /// <summary>Visit a parse tree, and return a user-defined result of the operation.</summary>
        /// <param name="tree">
        /// The
        /// <see cref="IParseTree"/>
        /// to visit.
        /// </param>
        /// <returns>The result of visiting the parse tree.</returns>
        Result Visit(IParseTree tree);

        /// <summary>
        /// Visit the children of a node, and return a user-defined result
        /// of the operation.
        /// </summary>
        /// <param name="node">
        /// The
        /// <see cref="IRuleNode"/>
        /// whose children should be visited.
        /// </param>
        /// <returns>The result of visiting the children of the node.</returns>
        Result VisitChildren(IRuleNode node);

        /// <summary>Visit a terminal node, and return a user-defined result of the operation.</summary>
        /// <param name="node">
        /// The
        /// <see cref="ITerminalNode"/>
        /// to visit.
        /// </param>
        /// <returns>The result of visiting the node.</returns>
        Result VisitTerminal(ITerminalNode node);

        /// <summary>Visit an error node, and return a user-defined result of the operation.</summary>
        /// <param name="node">
        /// The
        /// <see cref="IErrorNode"/>
        /// to visit.
        /// </param>
        /// <returns>The result of visiting the node.</returns>
        Result VisitErrorNode(IErrorNode node);
    }
}
