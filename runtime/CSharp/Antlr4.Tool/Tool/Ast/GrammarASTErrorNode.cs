/*
 * [The "BSD license"]
 *  Copyright (c) 2012 Terence Parr
 *  Copyright (c) 2012 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
