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

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr.Runtime;
    using Antlr4.Misc;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;

    /** A model object representing a parse tree listener file.
     *  These are the rules specific events triggered by a parse tree visitor.
     */
    public class ListenerFile : OutputFile
    {
        public string genPackage; // from -package cmd-line
        public string grammarName;
        public string parserName;
        /**
         * The names of all listener contexts.
         */
        public ISet<string> listenerNames = new LinkedHashSet<string>();
        /**
         * For listener contexts created for a labeled outer alternative, maps from
         * a listener context name to the name of the rule which defines the
         * context.
         */
        public IDictionary<string, string> listenerLabelRuleNames = new LinkedHashMap<string, string>();

        [ModelElement]
        public Action header;

        public ListenerFile(OutputModelFactory factory, string fileName)
            : base(factory, fileName)
        {
            Grammar g = factory.GetGrammar();
            parserName = g.GetRecognizerName();
            grammarName = g.name;

            foreach (KeyValuePair<string, IList<RuleAST>> entry in g.contextASTs)
            {
                foreach (RuleAST ruleAST in entry.Value)
                {
                    try
                    {
                        IDictionary<string, IList<System.Tuple<int, AltAST>>> labeledAlternatives = g.GetLabeledAlternatives(ruleAST);
                        listenerNames.UnionWith(labeledAlternatives.Keys);
                    }
                    catch (RecognitionException)
                    {
                    }
                }
            }

            foreach (Rule r in g.rules.Values)
            {
                listenerNames.Add(r.GetBaseContext());
            }

            foreach (Rule r in g.rules.Values)
            {
                IDictionary<string, IList<System.Tuple<int, AltAST>>> labels = r.GetAltLabels();
                if (labels != null)
                {
                    foreach (KeyValuePair<string, IList<System.Tuple<int, AltAST>>> pair in labels)
                    {
                        listenerLabelRuleNames[pair.Key] = r.name;
                    }
                }
            }

            ActionAST ast;
            if (g.namedActions.TryGetValue("header", out ast) && ast != null)
                header = new Action(factory, ast);

            genPackage = factory.GetGrammar().tool.genPackage;
        }
    }
}
