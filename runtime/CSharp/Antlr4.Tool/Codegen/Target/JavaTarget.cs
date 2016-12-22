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

namespace Antlr4.Codegen.Target
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using Antlr4.StringTemplate;
    using Antlr4.Tool.Ast;
    using ArgumentException = System.ArgumentException;
    using Convert = System.Convert;

    /**
     *
     * @author Sam Harwell
     */
    public class JavaTarget : AbstractTarget
    {
        /**
         * The Java target can cache the code generation templates.
         */
        private static readonly ThreadLocal<TemplateGroup> targetTemplates = new ThreadLocal<TemplateGroup>();

        protected static readonly string[] javaKeywords =
            {
                "abstract", "assert", "boolean", "break", "byte", "case", "catch",
                "char", "class", "const", "continue", "default", "do", "double", "else",
                "enum", "extends", "false", "final", "finally", "float", "for", "goto",
                "if", "implements", "import", "instanceof", "int", "interface",
                "long", "native", "new", "null", "package", "private", "protected",
                "public", "return", "short", "static", "strictfp", "super", "switch",
                "synchronized", "this", "throw", "throws", "transient", "true", "try",
                "void", "volatile", "while"
            };

        /** Avoid grammar symbols in this set to prevent conflicts in gen'd code. */
        protected readonly ISet<string> badWords = new HashSet<string>();

        public JavaTarget(CodeGenerator gen)
            : base(gen, "Java")
        {
        }

        public virtual ISet<string> GetBadWords()
        {
            if (badWords.Count == 0)
            {
                AddBadWords();
            }

            return badWords;
        }

        protected virtual void AddBadWords()
        {
            badWords.UnionWith(javaKeywords);
            badWords.Add("rule");
            badWords.Add("parserRule");
        }

        /**
         * {@inheritDoc}
         * <p>
         * For Java, this is the translation {@code 'a\n"'} → {@code "a\n\""}.
         * Expect single quotes around the incoming literal. Just flip the quotes
         * and replace double quotes with {@code \"}.
         * </p>
         * <p>
         * Note that we have decided to allow people to use '\"' without penalty, so
         * we must build the target string in a loop as {@link String#replace}
         * cannot handle both {@code \"} and {@code "} without a lot of messing
         * around.
         * </p>
         */
        public override string GetTargetStringLiteralFromANTLRStringLiteral(
            CodeGenerator generator,
            string literal, bool addQuotes)
        {
            StringBuilder sb = new StringBuilder();
            string @is = literal;

            if (addQuotes)
                sb.Append('"');

            for (int i = 1; i < @is.Length - 1; i++)
            {
                if (@is[i] == '\\')
                {
                    // Anything escaped is what it is! We assume that
                    // people know how to escape characters correctly. However
                    // we catch anything that does not need an escape in Java (which
                    // is what the default implementation is dealing with and remove
                    // the escape. The C target does this for instance.
                    //
                    switch (@is[i + 1])
                    {
                    // Pass through any escapes that Java also needs
                    //
                    case '"':
                    case 'n':
                    case 'r':
                    case 't':
                    case 'b':
                    case 'f':
                    case '\\':
                        // Pass the escape through
                        sb.Append('\\');
                        break;

                    case 'u':    // Assume uNNNN
                                 // Pass the escape through as double \\
                                 // so that Java leaves as \u0000 string not char
                        sb.Append('\\');
                        sb.Append('\\');
                        break;

                    default:
                        // Remove the escape by virtue of not adding it here
                        // Thus \' becomes ' and so on
                        break;
                    }

                    // Go past the \ character
                    i++;
                }
                else
                {
                    // Characters that don't need \ in ANTLR 'strings' but do in Java
                    if (@is[i] == '"')
                    {
                        // We need to escape " in Java
                        sb.Append('\\');
                    }
                }
                // Add in the next character, which may have been escaped
                sb.Append(@is[i]);
            }

            if (addQuotes)
                sb.Append('"');

            return sb.ToString();
        }

        public override string EncodeIntAsCharEscape(int v)
        {
            if (v < char.MinValue || v > char.MaxValue)
            {
                throw new ArgumentException(string.Format("Cannot encode the specified value: {0}", v));
            }

            if (v >= 0 && v < targetCharValueEscape.Length && targetCharValueEscape[v] != null)
            {
                return targetCharValueEscape[v];
            }

            if (v >= 0x20 && v < 127 && (!char.IsDigit((char)v) || v == '8' || v == '9'))
            {
                return ((char)v).ToString();
            }

            if (v >= 0 && v <= 127)
            {
                string oct = Convert.ToString(v, 8);
                return "\\" + oct;
            }

            string hex = v.ToString("x4");
            return "\\u" + hex;
        }

        public override int GetSerializedATNSegmentLimit()
        {
            // 65535 is the class file format byte limit for a UTF-8 encoded string literal
            // 3 is the maximum number of bytes it takes to encode a value in the range 0-0xFFFF
            return 65535 / 3;
        }

        protected override bool VisibleGrammarSymbolCausesIssueInGeneratedCode(GrammarAST idNode)
        {
            return GetBadWords().Contains(idNode.Text);
        }

        protected override TemplateGroup LoadTemplates()
        {
            TemplateGroup result = targetTemplates.Value;
            if (result == null)
            {
                result = base.LoadTemplates();
                result.RegisterRenderer(typeof(string), new JavaStringRenderer(), true);
                targetTemplates.Value = result;
            }

            return result;
        }

        protected class JavaStringRenderer : StringRenderer
        {
            public override string ToString(object o, string formatString, CultureInfo culture)
            {
                if ("java-escape".Equals(formatString))
                {
                    // 5C is the hex code for the \ itself
                    return ((string)o).Replace("\\u", "\\u005Cu");
                }

                return base.ToString(o, formatString, culture);
            }
        }
    }
}
