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

namespace Antlr4.Parse
{
    using Antlr4.Tool.Ast;
    using CommonToken = Antlr.Runtime.CommonToken;
    using CommonTreeAdaptor = Antlr.Runtime.Tree.CommonTreeAdaptor;
    using IToken = Antlr.Runtime.IToken;

    public class GrammarASTAdaptor : CommonTreeAdaptor
    {
        Antlr.Runtime.ICharStream input; // where we can find chars ref'd by tokens in tree
        public GrammarASTAdaptor()
        {
        }

        public GrammarASTAdaptor(Antlr.Runtime.ICharStream input)
        {
            this.input = input;
        }

        public override object Nil()
        {
            return (GrammarAST)base.Nil();
        }

        public override object Create(IToken token)
        {
            return new GrammarAST(token);
        }

        /** Make sure even imaginary nodes know the input stream */
        public override object Create(int tokenType, string text)
        {
            GrammarAST t;
            if (tokenType == ANTLRParser.RULE)
            {
                // needed by TreeWizard to make RULE tree
                t = new RuleAST(new CommonToken(tokenType, text));
            }
            else if (tokenType == ANTLRParser.STRING_LITERAL)
            {
                // implicit lexer construction done with wizard; needs this node type
                // whereas grammar ANTLRParser.g can use token option to spec node type
                t = new TerminalAST(new CommonToken(tokenType, text));
            }
            else
            {
                t = (GrammarAST)base.Create(tokenType, text);
            }
            t.Token.InputStream = input;
            return t;
        }

        public override object Create(int tokenType, IToken fromToken, string text)
        {
            return (GrammarAST)base.Create(tokenType, fromToken, text);
        }

        public override object Create(int tokenType, IToken fromToken)
        {
            return (GrammarAST)base.Create(tokenType, fromToken);
        }

        public override object DupNode(object t)
        {
            if (t == null)
                return null;
            return ((GrammarAST)t).DupNode(); //Create(((GrammarAST)t).Token);
        }

        public override object ErrorNode(Antlr.Runtime.ITokenStream input, IToken start, IToken stop,
                                Antlr.Runtime.RecognitionException e)
        {
            return new GrammarASTErrorNode(input, start, stop, e);
        }
    }
}
