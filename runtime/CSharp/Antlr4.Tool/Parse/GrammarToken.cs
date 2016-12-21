/*
 * [The "BSD license"]
 *  Copyright (c) 2014 Terence Parr
 *  Copyright (c) 2014 Sam Harwell
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
    using Antlr.Runtime;
    using Antlr4.Tool;
    using CommonToken = Antlr.Runtime.CommonToken;
    using IToken = Antlr.Runtime.IToken;

    /** A CommonToken that can also track it's original location,
     *  derived from options on the element ref like BEGIN&lt;line=34,...&gt;.
     */
    public class GrammarToken : IToken
    {
        public Grammar g;
        public int originalTokenIndex = -1;
        private CommonToken _token;

        public GrammarToken(Grammar g, IToken oldToken)
        {
            this.g = g;
            _token = new CommonToken(oldToken);
        }

        public int CharPositionInLine
        {
            get
            {
                if (originalTokenIndex >= 0)
                    return g.originalTokenStream.Get(originalTokenIndex).CharPositionInLine;

                return _token.CharPositionInLine;
            }

            set
            {
                _token.CharPositionInLine = value;
            }
        }

        public int Line
        {
            get
            {
                if (originalTokenIndex >= 0)
                    return g.originalTokenStream.Get(originalTokenIndex).Line;

                return _token.Line;
            }

            set
            {
                _token.Line = value;
            }
        }

        public int TokenIndex
        {
            get
            {
                return originalTokenIndex;
            }

            set
            {
                _token.TokenIndex = value;
            }
        }

        public int StartIndex
        {
            get
            {
                if (originalTokenIndex >= 0)
                    return g.originalTokenStream.Get(originalTokenIndex).StartIndex;

                return _token.StartIndex;
            }

            set
            {
                _token.StartIndex = value;
            }
        }

        public int StopIndex
        {
            get
            {
                int n = _token.StopIndex - _token.StartIndex + 1;
                return StartIndex + n - 1;
            }

            set
            {
                _token.StopIndex = value;
            }
        }

        public int Channel
        {
            get
            {
                return _token.Channel;
            }

            set
            {
                _token.Channel = value;
            }
        }

        public ICharStream InputStream
        {
            get
            {
                return _token.InputStream;
            }

            set
            {
                _token.InputStream = value;
            }
        }

        public string Text
        {
            get
            {
                return _token.Text;
            }

            set
            {
                _token.Text = value;
            }
        }

        public int Type
        {
            get
            {
                return _token.Type;
            }

            set
            {
                _token.Type = value;
            }
        }

        public override string ToString()
        {
            string channelStr = "";
            if (Channel > 0)
            {
                channelStr = ",channel=" + Channel;
            }
            string txt = Text;
            if (txt != null)
            {
                txt = txt.Replace("\n", "\\n");
                txt = txt.Replace("\r", "\\r");
                txt = txt.Replace("\t", "\\t");
            }
            else
            {
                txt = "<no text>";
            }
            return "[@" + TokenIndex + "," + StartIndex + ":" + StopIndex +
                   "='" + txt + "',<" + Type + ">" + channelStr + "," + Line + ":" + CharPositionInLine + "]";
        }
    }
}
