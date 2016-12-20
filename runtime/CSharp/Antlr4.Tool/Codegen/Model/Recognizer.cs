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

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Codegen.Model.Chunk;
    using Antlr4.Misc;
    using Antlr4.Tool;
    using Array = System.Array;
    using Path = System.IO.Path;

    public abstract class Recognizer : OutputModelObject
    {
        public string name;
        public string grammarName;
        public string grammarFileName;
        public IDictionary<string, int> tokens;

        /**
         * @deprecated This field is provided only for compatibility with code
         * generation targets which have not yet been updated to use
         * {@link #literalNames} and {@link #symbolicNames}.
         */
        [System.Obsolete]
        public string[] tokenNames;

        public string[] literalNames;
        public string[] symbolicNames;
        public ICollection<string> ruleNames;
        public ICollection<Rule> rules;
        [ModelElement]
        public ActionChunk superClass;
        public bool abstractRecognizer;

        [ModelElement]
        public SerializedATN atn;
        [ModelElement]
        public LinkedHashMap<Rule, RuleSempredFunction> sempredFuncs =
            new LinkedHashMap<Rule, RuleSempredFunction>();

        protected Recognizer(OutputModelFactory factory)
            : base(factory)
        {
            Grammar g = factory.GetGrammar();
            grammarFileName = Path.GetFileName(g.fileName);
            grammarName = g.name;
            name = g.GetRecognizerName();
            tokens = new LinkedHashMap<string, int>();
            foreach (KeyValuePair<string, int> entry in g.tokenNameToTypeMap)
            {
                int ttype = entry.Value;
                if (ttype > 0)
                {
                    tokens[entry.Key] = ttype;
                }
            }

            ruleNames = g.rules.Keys;
            rules = g.rules.Values;
            atn = new SerializedATN(factory, g.atn, g.GetRuleNames());
            if (g.GetOptionString("superClass") != null)
            {
                superClass = new ActionText(null, g.GetOptionString("superClass"));
            }
            else
            {
                superClass = null;
            }

#pragma warning disable CS0612 // Type or member is obsolete
            tokenNames = TranslateTokenStringsToTarget(g.GetTokenDisplayNames(), factory);
#pragma warning restore CS0612 // Type or member is obsolete
            literalNames = TranslateTokenStringsToTarget(g.GetTokenLiteralNames(), factory);
            symbolicNames = TranslateTokenStringsToTarget(g.GetTokenSymbolicNames(), factory);
            abstractRecognizer = g.IsAbstract();
        }

        protected static string[] TranslateTokenStringsToTarget(string[] tokenStrings, OutputModelFactory factory)
        {
            string[] result = (string[])tokenStrings.Clone();
            for (int i = 0; i < tokenStrings.Length; i++)
            {
                result[i] = TranslateTokenStringToTarget(tokenStrings[i], factory);
            }

            int lastTrueEntry = result.Length - 1;
            while (lastTrueEntry >= 0 && result[lastTrueEntry] == null)
            {
                lastTrueEntry--;
            }

            if (lastTrueEntry < result.Length - 1)
            {
                Array.Resize(ref result, lastTrueEntry + 1);
            }

            return result;
        }

        protected static string TranslateTokenStringToTarget(string tokenName, OutputModelFactory factory)
        {
            if (tokenName == null)
            {
                return null;
            }

            if (tokenName[0] == '\'')
            {
                bool addQuotes = false;
                string targetString =
                    factory.GetTarget().GetTargetStringLiteralFromANTLRStringLiteral(factory.GetGenerator(), tokenName, addQuotes);
                return "\"'" + targetString + "'\"";
            }
            else
            {
                return factory.GetTarget().GetTargetStringLiteralFromString(tokenName, true);
            }
        }
    }
}
