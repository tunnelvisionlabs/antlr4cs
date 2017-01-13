// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
