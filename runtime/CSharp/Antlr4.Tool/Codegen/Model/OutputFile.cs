// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;

    public abstract class OutputFile : OutputModelObject
    {
        public readonly string fileName;
        public readonly string grammarFileName;
        public readonly string ANTLRVersion;
        public readonly string TokenLabelType;
        public readonly string InputSymbolType;
        public readonly string AccessModifier;
        public readonly bool IncludeDebuggerNonUserCodeAttribute; // from -DincludeDebuggerNonUserCodeAttribute
        public readonly bool IncludeClsCompliantAttribute;

        protected OutputFile(OutputModelFactory factory, string fileName)
            : base(factory)
        {
            this.fileName = fileName;
            Grammar g = factory.GetGrammar();
            grammarFileName = g.fileName;
            ANTLRVersion = AntlrTool.VERSION;
            TokenLabelType = g.GetOptionString("TokenLabelType");
            InputSymbolType = TokenLabelType;
            AccessModifier = System.StringComparer.OrdinalIgnoreCase.Equals( g.GetOptionString("useInternalAccessModifier"), "true" ) ? "internal" : "public";
            IncludeDebuggerNonUserCodeAttribute = System.StringComparer.OrdinalIgnoreCase.Equals( g.GetOptionString("includeDebuggerNonUserCodeAttribute"), "true" );
            IncludeClsCompliantAttribute = !System.StringComparer.OrdinalIgnoreCase.Equals( g.GetOptionString("excludeClsCompliantAttribute"), "true"); // default is to include this to maintain existing behaviour
        }

        public virtual IDictionary<string, Action> BuildNamedActions(Grammar g)
        {
            IDictionary<string, Action> namedActions = new Dictionary<string, Action>();
            foreach (string name in g.namedActions.Keys)
            {
                ActionAST ast = g.namedActions[name];
                namedActions[name] = new Action(factory, ast);
            }
            return namedActions;
        }
    }
}
