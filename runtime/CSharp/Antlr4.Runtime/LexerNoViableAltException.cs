// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    [System.Serializable]
    public class LexerNoViableAltException : RecognitionException
    {
        private const long serialVersionUID = -730999203913001726L;

        /// <summary>Matching attempted at what input index?</summary>
        private readonly int startIndex;

        /// <summary>Which configurations did we try at input.index() that couldn't match input.LA(1)?</summary>
        [Nullable]
        private readonly ATNConfigSet deadEndConfigs;

        public LexerNoViableAltException(Lexer lexer, ICharStream input, int startIndex, ATNConfigSet deadEndConfigs)
            : base(lexer, input)
        {
            this.startIndex = startIndex;
            this.deadEndConfigs = deadEndConfigs;
        }

        public virtual int StartIndex
        {
            get
            {
                return startIndex;
            }
        }

        [Nullable]
        public virtual ATNConfigSet DeadEndConfigs
        {
            get
            {
                return deadEndConfigs;
            }
        }

        public override IIntStream InputStream
        {
            get
            {
                return (ICharStream)base.InputStream;
            }
        }

        public override string ToString()
        {
            string symbol = string.Empty;
            if (startIndex >= 0 && startIndex < ((ICharStream)InputStream).Size)
            {
                symbol = ((ICharStream)InputStream).GetText(Interval.Of(startIndex, startIndex));
                symbol = Utils.EscapeWhitespace(symbol, false);
            }
            return string.Format(CultureInfo.CurrentCulture, "{0}('{1}')", typeof(Antlr4.Runtime.LexerNoViableAltException).Name, symbol);
        }
    }
}
