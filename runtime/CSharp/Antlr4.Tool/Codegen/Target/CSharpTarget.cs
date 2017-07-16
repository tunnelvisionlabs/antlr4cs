/*
 * [The "BSD license"]
 *  Copyright (c) 2013 Terence Parr
 *  Copyright (c) 2013 Sam Harwell
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
    using System.Reflection;
    using System.Text;
    using Antlr4.StringTemplate;
    using Antlr4.Tool.Ast;
    using ArgumentException = System.ArgumentException;
    using Path = System.IO.Path;
    using Uri = System.Uri;

    public abstract class CSharpTarget : AbstractTarget
    {

        protected CSharpTarget(CodeGenerator gen, string language)
            : base(gen, language)
        {
            targetCharValueEscape[0] = "\\0";
            targetCharValueEscape[0x0007] = "\\a";
            targetCharValueEscape[0x000B] = "\\v";
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

            if (v >= 0x20 && v < 127 && (v < '0' || v > '9') && (v < 'a' || v > 'f') && (v < 'A' || v > 'F'))
            {
                return new string((char)v, 1);
            }

            return string.Format("\\x{0:X}", v);
        }

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

                    case 'u':    // Assume unnnn
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

        protected override bool VisibleGrammarSymbolCausesIssueInGeneratedCode(GrammarAST idNode)
        {
            return false;
        }

        protected override TemplateGroup LoadTemplates()
        {
            // override the superclass behavior to put all C# templates in the same folder
            string codeBaseLocation = new Uri(typeof(AntlrTool).GetTypeInfo().Assembly.CodeBase).LocalPath;
            string baseDirectory = Path.GetDirectoryName(codeBaseLocation);
            TemplateGroup result = new TemplateGroupFile(
                Path.Combine(
                    baseDirectory,
                    Path.Combine(CodeGenerator.TEMPLATE_ROOT, "CSharp", GetLanguage() + TemplateGroup.GroupFileExtension)),
                Encoding.UTF8);
            result.RegisterRenderer(typeof(int), new NumberRenderer());
            result.RegisterRenderer(typeof(string), new StringRenderer());
            result.Listener = new ErrorListener(this);

            return result;
        }
    }
}
