// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    /** A simple visitor, based upon the classic double dispatch method,
     *  for walking GrammarAST trees resulting from parsing ANTLR grammars.
     *  There is also the GrammarTreeVisitor.g tree grammar that looks for
     *  subtree patterns and fires off high-level events as opposed to
     *  "found node" events like this visitor does. Also, like all
     *  visitors, the users of this interface are required to implement
     *  the node visitation of the children. The GrammarTreeVisitor mechanism
     *  fires events and the user is not required to do any walking code.
     *
     *  GrammarAST t = ...;
     *  GrammarASTVisitor v = new ...;
     *  t.visit(v);
     */
    public interface GrammarASTVisitor
    {
        /** This is the generic visitor method that will be invoked
         *  for any other kind of AST node not covered by the other visit methods.
         */
        object Visit(GrammarAST node);

        object Visit(GrammarRootAST node);
        object Visit(RuleAST node);

        object Visit(BlockAST node);
        object Visit(OptionalBlockAST node);
        object Visit(PlusBlockAST node);
        object Visit(StarBlockAST node);

        object Visit(AltAST node);

        object Visit(NotAST node);
        object Visit(PredAST node);
        object Visit(RangeAST node);
        object Visit(SetAST node);
        object Visit(RuleRefAST node);
        object Visit(TerminalAST node);
    }
}
