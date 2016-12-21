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

namespace Antlr4.Misc
{
    using System.Globalization;
    using System.Text;
    using Lexer = Antlr4.Runtime.Lexer;

    /** */
    public static class CharSupport
    {
        /** When converting ANTLR char and string literals, here is the
         *  value set of escape chars.
         */
        public static int[] ANTLRLiteralEscapedCharValue = new int[255];

        /** Given a char, we need to be able to show as an ANTLR literal.
         */
        public static string[] ANTLRLiteralCharValueEscape = new string[255];

        static CharSupport()
        {
            ANTLRLiteralEscapedCharValue['n'] = '\n';
            ANTLRLiteralEscapedCharValue['r'] = '\r';
            ANTLRLiteralEscapedCharValue['t'] = '\t';
            ANTLRLiteralEscapedCharValue['b'] = '\b';
            ANTLRLiteralEscapedCharValue['f'] = '\f';
            ANTLRLiteralEscapedCharValue['\\'] = '\\';
            ANTLRLiteralEscapedCharValue['\''] = '\'';
            ANTLRLiteralEscapedCharValue['"'] = '"';
            ANTLRLiteralCharValueEscape['\n'] = "\\n";
            ANTLRLiteralCharValueEscape['\r'] = "\\r";
            ANTLRLiteralCharValueEscape['\t'] = "\\t";
            ANTLRLiteralCharValueEscape['\b'] = "\\b";
            ANTLRLiteralCharValueEscape['\f'] = "\\f";
            ANTLRLiteralCharValueEscape['\\'] = "\\\\";
            ANTLRLiteralCharValueEscape['\''] = "\\'";
        }

        /** Return a string representing the escaped char for code c.  E.g., If c
         *  has value 0x100, you will get "\u0100".  ASCII gets the usual
         *  char (non-hex) representation.  Control characters are spit out
         *  as unicode.  While this is specially set up for returning Java strings,
         *  it can be used by any language target that has the same syntax. :)
         */
        public static string GetANTLRCharLiteralForChar(int c)
        {
            if (c < Lexer.MinCharValue)
            {
                return "'<INVALID>'";
            }
            if (c < ANTLRLiteralCharValueEscape.Length && ANTLRLiteralCharValueEscape[c] != null)
            {
                return '\'' + ANTLRLiteralCharValueEscape[c] + '\'';
            }
            if (c >= 0x20 && c <= 0x7f)
            {
                if (c == '\\')
                {
                    return "'\\\\'";
                }
                if (c == '\'')
                {
                    return "'\\''";
                }
                return '\'' + ((char)c).ToString() + '\'';
            }
            // turn on the bit above max "\uFFFF" value so that we pad with zeros
            // then only take last 4 digits
            string hex = c.ToString("x4");
            string unicodeStr = "'\\u" + hex + "'";
            return unicodeStr;
        }

        /** Given a literal like (the 3 char sequence with single quotes) 'a',
         *  return the int value of 'a'. Convert escape sequences here also.
         *  Return -1 if not single char.
         */
        public static int GetCharValueFromGrammarCharLiteral(string literal)
        {
            if (literal == null || literal.Length < 3)
                return -1;

            return GetCharValueFromCharInGrammarLiteral(literal.Substring(1, literal.Length - 2));
        }

        /** Given char x or \t or \u1234 return the char value;
         *  Unnecessary escapes like '\{' yield -1.
         */
        public static int GetCharValueFromCharInGrammarLiteral(string cstr)
        {
            switch (cstr.Length)
            {
            case 1:
                // 'x'
                return cstr[0]; // no escape char
            case 2:
                if (cstr[0] != '\\')
                    return -1;
                // '\x'  (antlr lexer will catch invalid char)
                if (char.IsDigit(cstr[1]))
                    return -1;
                int escChar = cstr[1];
                int charVal = ANTLRLiteralEscapedCharValue[escChar];
                if (charVal == 0)
                    return -1;
                return charVal;
            case 6:
                // '\u1234'
                if (!cstr.StartsWith("\\u"))
                    return -1;
                string unicodeChars = cstr.Substring(2, cstr.Length - 2);
                return int.Parse(unicodeChars, NumberStyles.AllowHexSpecifier);
            default:
                return -1;
            }
        }

        public static string GetStringFromGrammarStringLiteral(string literal)
        {
            StringBuilder buf = new StringBuilder();
            int i = 1; // skip first quote
            int n = literal.Length - 1; // skip last quote
            while (i < n)
            { // scan all but last quote
                int end = i + 1;
                if (literal[i] == '\\')
                {
                    end = i + 2;
                    if ((i + 1) >= n)
                        break; // ignore spurious \ on end
                    if (literal[i + 1] == 'u')
                        end = i + 6;
                }
                if (end > n)
                    break;
                string esc = literal.Substring(i, end - i);
                int c = GetCharValueFromCharInGrammarLiteral(esc);
                if (c == -1)
                {
                    buf.Append(esc);
                }
                else
                    buf.Append((char)c);
                i = end;
            }
            return buf.ToString();
        }

        public static string Capitalize(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
