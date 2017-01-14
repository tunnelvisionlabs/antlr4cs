// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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

    /**
     * Parse args, return values, locals
     * <p>
     * rule[arg1, arg2, ..., argN] returns [ret1, ..., retN]</p>
     * <p>
     * text is target language dependent.  Java/C#/C/C++ would
     * use "int i" but ruby/python would use "i". Languages with
     * postfix types like Go, Swift use "x : T" notation or "T x".</p>
     */
    public class ScopeParser
    {
        /**
         * Given an arg or retval scope definition list like
         * <p>
         * <code>
         * Map&lt;String, String&gt;, int[] j3, char *foo32[3]
         * </code></p>
         * <p>
         * or</p>
         * <p>
         * <code>
         * int i=3, j=a[34]+20
         * </code></p>
         * <p>
         * convert to an attribute scope.</p>
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
                if (decl.Item1.Trim().Length > 0)
                {
                    Attribute a = ParseAttributeDef(action, decl, g);
                    dict.Add(a);
                }
            }
            return dict;
        }

        /**
         * For decls like "String foo" or "char *foo32[]" compute the ID
         * and type declarations.  Also handle "int x=3" and 'T t = new T("foo")'
         * but if the separator is ',' you cannot use ',' in the initvalue
         * unless you escape use "\," escape.
         */
        public static Attribute ParseAttributeDef([Nullable] ActionAST action, [NotNull] System.Tuple<string, int> decl, Grammar g)
        {
            if (decl.Item1 == null)
                return null;

            Attribute attr = new Attribute();
            int rightEdgeOfDeclarator = decl.Item1.Length - 1;
            int equalsIndex = decl.Item1.IndexOf('=');
            if (equalsIndex > 0)
            {
                // everything after the '=' is the init value
                attr.initValue = decl.Item1.Substring(equalsIndex + 1).Trim();
                rightEdgeOfDeclarator = equalsIndex - 1;
            }

            string declarator = decl.Item1.Substring(0, rightEdgeOfDeclarator + 1);
            System.Tuple<int, int> p;
            string text = decl.Item1;
            text = text.Replace("::", "");
            if (text.Contains(":"))
            {
                // declarator has type appearing after the name like "x:T"
                p = _ParsePostfixDecl(attr, declarator, action, g);
            }
            else
            {
                // declarator has type appearing before the name like "T x"
                p = _ParsePrefixDecl(attr, declarator, action, g);
            }
            int idStart = p.Item1;
            int idStop = p.Item2;

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
                    // skip comments
                    if (i < actionText.Length - 1 && actionText[i] == '/' && actionText[i + 1] == '/')
                    {
                        while (i < actionText.Length && actionText[i] != '\n')
                        {
                            i++;
                        }
                    }
                }

                int declOffset = charIndexes[decl.Item2];
                int declLine = lines[declOffset + idStart];

                int line = action.Token.Line + declLine;
                int charPositionInLine = charPositionInLines[declOffset + idStart];
                if (declLine == 0)
                {
                    /* offset for the start position of the ARG_ACTION token, plus 1
                     * since the ARG_ACTION text had the leading '[' stripped before
                     * reaching the scope parser.
                     */
                    charPositionInLine += action.Token.CharPositionInLine + 1;
                }

                int offset = ((CommonToken)action.Token).StartIndex;
                attr.token = new CommonToken(action.Token.InputStream, ANTLRParser.ID, BaseRecognizer.DefaultTokenChannel, offset + declOffset + idStart + 1, offset + declOffset + idStop);
                attr.token.Line = line;
                attr.token.CharPositionInLine = charPositionInLine;
                Debug.Assert(attr.name.Equals(attr.token.Text), "Attribute text should match the pseudo-token text at this point.");
            }

            return attr;
        }

        public static System.Tuple<int, int> _ParsePrefixDecl(Attribute attr, string decl, ActionAST a, Grammar g)
        {
            // walk backwards looking for start of an ID
            bool inID = false;
            int start = -1;
            for (int i = decl.Length - 1; i >= 0; i--)
            {
                char ch = decl[i];
                // if we haven't found the end yet, keep going
                if (!inID && char.IsLetterOrDigit(ch))
                {
                    inID = true;
                }
                else if (inID && !(char.IsLetterOrDigit(ch) || ch == '_'))
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
                g.tool.errMgr.GrammarError(ErrorType.CANNOT_FIND_ATTRIBUTE_NAME_IN_DECL, g.fileName, a.Token, decl);
            }

            // walk forward looking for end of an ID
            int stop = -1;
            for (int i = start; i < decl.Length; i++)
            {
                char ch = decl[i];
                // if we haven't found the end yet, keep going
                if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                {
                    stop = i;
                    break;
                }
                if (i == decl.Length - 1)
                {
                    stop = i + 1;
                }
            }

            // the name is the last ID
            attr.name = decl.Substring(start, stop - start);

            // the type is the decl minus the ID (could be empty)
            attr.type = decl.Substring(0, start);
            if (stop <= decl.Length - 1)
            {
                attr.type += decl.Substring(stop, decl.Length - stop);
            }

            attr.type = attr.type.Trim();
            if (attr.type.Length == 0)
            {
                attr.type = null;
            }

            return Tuple.Create(start, stop);
        }

        public static System.Tuple<int, int> _ParsePostfixDecl(Attribute attr, string decl, ActionAST a, Grammar g)
        {
            int start = -1;
            int stop = -1;
            int colon = decl.IndexOf(':');
            int namePartEnd = colon == -1 ? decl.Length : colon;

            // look for start of name
            for (int i = 0; i < namePartEnd; ++i)
            {
                char ch = decl[i];
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    start = i;
                    break;
                }
            }

            if (start == -1)
            {
                start = 0;
                g.tool.errMgr.GrammarError(ErrorType.CANNOT_FIND_ATTRIBUTE_NAME_IN_DECL, g.fileName, a.Token, decl);
            }

            // look for stop of name
            for (int i = start; i < namePartEnd; ++i)
            {
                char ch = decl[i];
                if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                {
                    stop = i;
                    break;
                }
                if (i == namePartEnd - 1)
                {
                    stop = namePartEnd;
                }
            }

            if (stop == -1)
            {
                stop = start;
            }

            // extract name from decl
            attr.name = decl.Substring(start, stop - start);

            // extract type from decl (could be empty)
            if (colon == -1)
            {
                attr.type = "";
            }
            else
            {
                attr.type = decl.Substring(colon + 1, decl.Length - colon - 1);
            }
            attr.type = attr.type.Trim();

            if (attr.type.Length == 0)
            {
                attr.type = null;
            }
            return Tuple.Create(start, stop);
        }

        /**
         * Given an argument list like
         * <p>
         * x, (*a).foo(21,33), 3.2+1, '\n',
         * "a,oo\nick", {bl, "fdkj"eck}, ["cat\n,", x, 43]</p>
         * <p>
         * convert to a list of attributes.  Allow nested square brackets etc...
         * Set separatorChar to ';' or ',' or whatever you want.</p>
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
