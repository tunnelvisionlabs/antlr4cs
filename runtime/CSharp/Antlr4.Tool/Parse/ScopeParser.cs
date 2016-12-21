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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using BaseRecognizer = Antlr.Runtime.BaseRecognizer;
    using CommonToken = Antlr.Runtime.CommonToken;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using Tuple = System.Tuple;

    /** Parse args, return values, locals
     *
     *  rule[arg1, arg2, ..., argN] returns [ret1, ..., retN]
     *
     *  text is target language dependent.  Java/C#/C/C++ would
     *  use "int i" but ruby/python would use "i".
     */
    public class ScopeParser
    {
        /** Given an arg or retval scope definition list like
         *
         *  <code>
         *  Map&lt;String, String&gt;, int[] j3, char *foo32[3]
         *  </code>
         *
         *  or
         *
         *  <code>
         *  int i=3, j=a[34]+20
         *  </code>
         *
         *  convert to an attribute scope.
         */
        public static AttributeDict ParseTypedArgList([Nullable] ActionAST action, string s, Grammar g)
        {
            return Parse(action, s, ',', g);
        }

        public static AttributeDict Parse([Nullable] ActionAST action, string s, char separator, Grammar g)
        {
            AttributeDict dict = new AttributeDict();
            IList<System.Tuple<string, int>> decls = SplitDecls(s, separator);
            foreach (System.Tuple<string, int> decl in decls)
            {
                //System.Console.WriteLine("decl=" + decl);
                if (decl.Item1.Trim().Length > 0)
                {
                    Attribute a = ParseAttributeDef(action, decl, g);
                    dict.Add(a);
                }
            }
            return dict;
        }

        /** For decls like "String foo" or "char *foo32[]" compute the ID
         *  and type declarations.  Also handle "int x=3" and 'T t = new T("foo")'
         *  but if the separator is ',' you cannot use ',' in the initvalue
         *  unless you escape use "\," escape.
         */
        public static Attribute ParseAttributeDef([Nullable] ActionAST action, [NotNull] System.Tuple<string, int> decl, Grammar g)
        {
            if (decl.Item1 == null)
                return null;

            Attribute attr = new Attribute();
            bool inID = false;
            int start = -1;
            int rightEdgeOfDeclarator = decl.Item1.Length - 1;
            int equalsIndex = decl.Item1.IndexOf('=');
            if (equalsIndex > 0)
            {
                // everything after the '=' is the init value
                attr.initValue = decl.Item1.Substring(equalsIndex + 1);
                rightEdgeOfDeclarator = equalsIndex - 1;
            }
            // walk backwards looking for start of an ID
            for (int i = rightEdgeOfDeclarator; i >= 0; i--)
            {
                // if we haven't found the end yet, keep going
                if (!inID && char.IsLetterOrDigit(decl.Item1[i]))
                {
                    inID = true;
                }
                else if (inID &&
                          !(char.IsLetterOrDigit(decl.Item1[i]) ||
                           decl.Item1[i] == '_'))
                {
                    start = i + 1;
                    break;
                }
            }
            if (start < 0 && inID)
            {
                start = 0;
            }
            if (start < 0)
            {
                g.tool.errMgr.GrammarError(ErrorType.CANNOT_FIND_ATTRIBUTE_NAME_IN_DECL, g.fileName, action.Token, decl);
            }
            // walk forwards looking for end of an ID
            int stop = -1;
            for (int i = start; i <= rightEdgeOfDeclarator; i++)
            {
                // if we haven't found the end yet, keep going
                if (!(char.IsLetterOrDigit(decl.Item1[i]) ||
                    decl.Item1[i] == '_'))
                {
                    stop = i;
                    break;
                }
                if (i == rightEdgeOfDeclarator)
                {
                    stop = i + 1;
                }
            }

            // the name is the last ID
            attr.name = decl.Item1.Substring(start, stop - start);

            // the type is the decl minus the ID (could be empty)
            attr.type = decl.Item1.Substring(0, start);
            if (stop <= rightEdgeOfDeclarator)
            {
                attr.type += decl.Item1.Substring(stop, rightEdgeOfDeclarator + 1 - stop);
            }
            attr.type = attr.type.Trim();
            if (attr.type.Length == 0)
            {
                attr.type = null;
            }

            attr.decl = decl.Item1;

            if (action != null)
            {
                string actionText = action.Text;
                int[] lines = new int[actionText.Length];
                int[] charPositionInLines = new int[actionText.Length];
                for (int i = 0, line1 = 0, col = 0; i < actionText.Length; i++, col++)
                {
                    lines[i] = line1;
                    charPositionInLines[i] = col;
                    if (actionText[i] == '\n')
                    {
                        line1++;
                        col = -1;
                    }
                }

                int[] charIndexes = new int[actionText.Length];
                for (int i = 0, j = 0; i < actionText.Length; i++, j++)
                {
                    charIndexes[j] = i;
                    if (i < actionText.Length - 1 && actionText[i] == '/' && actionText[i + 1] == '/')
                    {
                        while (i < actionText.Length && actionText[i] != '\n')
                        {
                            i++;
                        }
                    }
                }

                int declOffset = charIndexes[decl.Item2];
                int declLine = lines[declOffset + start];

                int line = action.Token.Line + declLine;
                int charPositionInLine = charPositionInLines[declOffset + start];
                if (declLine == 0)
                {
                    /* offset for the start position of the ARG_ACTION token, plus 1
                     * since the ARG_ACTION text had the leading '[' stripped before
                     * reaching the scope parser.
                     */
                    charPositionInLine += action.Token.CharPositionInLine + 1;
                }

                int offset = ((CommonToken)action.Token).StartIndex;
                attr.token = new CommonToken(action.Token.InputStream, ANTLRParser.ID, BaseRecognizer.DefaultTokenChannel, offset + declOffset + start + 1, offset + declOffset + stop);
                attr.token.Line = line;
                attr.token.CharPositionInLine = charPositionInLine;
                Debug.Assert(attr.name.Equals(attr.token.Text), "Attribute text should match the pseudo-token text at this point.");
            }

            return attr;
        }

        /** Given an argument list like
         *
         *  x, (*a).foo(21,33), 3.2+1, '\n',
         *  "a,oo\nick", {bl, "fdkj"eck}, ["cat\n,", x, 43]
         *
         *  convert to a list of attributes.  Allow nested square brackets etc...
         *  Set separatorChar to ';' or ',' or whatever you want.
         */
        public static IList<System.Tuple<string, int>> SplitDecls(string s, int separatorChar)
        {
            IList<System.Tuple<string, int>> args = new List<System.Tuple<string, int>>();
            _SplitArgumentList(s, 0, -1, separatorChar, args);
            return args;
        }

        public static int _SplitArgumentList(string actionText,
                                             int start,
                                             int targetChar,
                                             int separatorChar,
                                             IList<System.Tuple<string, int>> args)
        {
            if (actionText == null)
            {
                return -1;
            }

            actionText = Regex.Replace(actionText, "//[^\\n]*", "");
            int n = actionText.Length;
            //System.Console.WriteLine("actionText@" + start + "->" + (char)targetChar + "=" + actionText.Substring(start, n - start));
            int p = start;
            int last = p;
            while (p < n && actionText[p] != targetChar)
            {
                int c = actionText[p];
                switch (c)
                {
                case '\'':
                    p++;
                    while (p < n && actionText[p] != '\'')
                    {
                        if (actionText[p] == '\\' && (p + 1) < n &&
                             actionText[p + 1] == '\'')
                        {
                            p++; // skip escaped quote
                        }
                        p++;
                    }
                    p++;
                    break;
                case '"':
                    p++;
                    while (p < n && actionText[p] != '\"')
                    {
                        if (actionText[p] == '\\' && (p + 1) < n &&
                             actionText[p + 1] == '\"')
                        {
                            p++; // skip escaped quote
                        }
                        p++;
                    }
                    p++;
                    break;
                case '(':
                    p = _SplitArgumentList(actionText, p + 1, ')', separatorChar, args);
                    break;
                case '{':
                    p = _SplitArgumentList(actionText, p + 1, '}', separatorChar, args);
                    break;
                case '<':
                    if (actionText.IndexOf('>', p + 1) >= p)
                    {
                        // do we see a matching '>' ahead?  if so, hope it's a generic
                        // and not less followed by expr with greater than
                        p = _SplitArgumentList(actionText, p + 1, '>', separatorChar, args);
                    }
                    else
                    {
                        p++; // treat as normal char
                    }
                    break;
                case '[':
                    p = _SplitArgumentList(actionText, p + 1, ']', separatorChar, args);
                    break;
                default:
                    if (c == separatorChar && targetChar == -1)
                    {
                        string arg = actionText.Substring(last, p - last);
                        int index = last;
                        while (index < p && char.IsWhiteSpace(actionText[index]))
                        {
                            index++;
                        }
                        //System.out.println("arg="+arg);
                        args.Add(Tuple.Create(arg.Trim(), index));
                        last = p + 1;
                    }
                    p++;
                    break;
                }
            }
            if (targetChar == -1 && p <= n)
            {
                string arg = actionText.Substring(last, p - last).Trim();
                int index = last;
                while (index < p && char.IsWhiteSpace(actionText[index]))
                {
                    index++;
                }
                //System.out.println("arg="+arg);
                if (arg.Length > 0)
                {
                    args.Add(Tuple.Create(arg.Trim(), index));
                }
            }
            p++;
            return p;
        }

    }
}
