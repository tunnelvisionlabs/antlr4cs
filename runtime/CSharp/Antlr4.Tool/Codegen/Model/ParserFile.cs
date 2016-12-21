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
    using Antlr4.Codegen.Model.Chunk;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;

    /** */
    public class ParserFile : OutputFile
    {
        public string genPackage; // from -package cmd-line
        [ModelElement]
        public Parser parser;
        [ModelElement]
        public IDictionary<string, Action> namedActions;
        [ModelElement]
        public ActionChunk contextSuperClass;
        public bool genListener = false;
        public bool genVisitor = false;
        public string grammarName;

        public ParserFile(OutputModelFactory factory, string fileName)
            : base(factory, fileName)
        {
            Grammar g = factory.GetGrammar();
            namedActions = new Dictionary<string, Action>();
            foreach (string name in g.namedActions.Keys)
            {
                ActionAST ast;
                g.namedActions.TryGetValue(name, out ast);
                namedActions[name] = new Action(factory, ast);
            }

            genPackage = g.tool.genPackage;
            // need the below members in the ST for Python
            genListener = g.tool.gen_listener;
            genVisitor = g.tool.gen_visitor;
            grammarName = g.name;

            if (g.GetOptionString("contextSuperClass") != null)
            {
                contextSuperClass = new ActionText(null, g.GetOptionString("contextSuperClass"));
            }
        }
    }
}