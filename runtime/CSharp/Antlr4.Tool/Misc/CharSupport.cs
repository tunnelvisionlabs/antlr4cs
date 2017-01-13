// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
            ANTLRLiteralEscapedCharValue['-'] = '-';
            ANTLRLiteralEscapedCharValue[']'] = ']';
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
                    if (i + 1 < n && literal[i + 1] == 'u')
                    {
                        for (end = i + 2; end < i + 6; end++)
                        {
                            if (end > n)
                                return null; // invalid escape sequence.
                            char charAt = literal[end];
                            if (!char.IsDigit(charAt) && !(charAt >= 'a' && charAt <= 'f') && !(charAt >= 'A' && charAt <= 'F'))
                            {
                                return null; // invalid escape sequence.
                            }
                        }
                    }
                }
                if (end > n)
                    return null; // invalid escape sequence.

                string esc = literal.Substring(i, end - i);
                int c = GetCharValueFromCharInGrammarLiteral(esc);
                if (c == -1)
                {
                    return null; // invalid escape sequence.
                }
                else
                    buf.Append((char)c);
                i = end;
            }

            return buf.ToString();
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
                int result;
                if (!int.TryParse(unicodeChars, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out result))
                    return -1;

                return result;
            default:
                return -1;
            }
        }

        public static string Capitalize(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
