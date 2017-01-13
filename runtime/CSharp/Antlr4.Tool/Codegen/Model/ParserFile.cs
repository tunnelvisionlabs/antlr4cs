// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
