// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using CommonErrorNode = Antlr.Runtime.Tree.CommonErrorNode;
    using IToken = Antlr.Runtime.IToken;
    using ITokenStream = Antlr.Runtime.ITokenStream;
    using RecognitionException = Antlr.Runtime.RecognitionException;

    /** A node representing erroneous token range in token stream */
    public class GrammarASTErrorNode : GrammarAST
    {
        CommonErrorNode @delegate;

        public GrammarASTErrorNode(ITokenStream input, IToken start, IToken stop,
                                   RecognitionException e)
        {
            @delegate = new CommonErrorNode(input, start, stop, e);
        }

        public override bool IsNil
        {
            get
            {
                return @delegate.IsNil;
            }
        }

        public override int Type
        {
            get
            {
                return @delegate.Type;
            }

            set
            {
                base.Type = value;
            }
        }

        public override string Text
        {
            get
            {
                return @delegate.Text;
            }

            set
            {
                base.Text = value;
            }
        }

        public override string ToString()
        {
            return @delegate.ToString();
        }
    }
}
