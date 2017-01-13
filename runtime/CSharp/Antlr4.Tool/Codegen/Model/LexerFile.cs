// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;

    public class LexerFile : OutputFile
    {
        public string genPackage; // from -package cmd-line
        [ModelElement]
        public Lexer lexer;
        [ModelElement]
        public IDictionary<string, Action> namedActions;

        public LexerFile(OutputModelFactory factory, string fileName)
            : base(factory, fileName)
        {
            namedActions = new Dictionary<string, Action>();
            Grammar g = factory.GetGrammar();
            foreach (string name in g.namedActions.Keys)
            {
                ActionAST ast;
                g.namedActions.TryGetValue(name, out ast);
                namedActions[name] = new Action(factory, ast);
            }

            genPackage = factory.GetGrammar().tool.genPackage;
        }
    }
}
