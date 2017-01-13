// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using Antlr4.Misc;
    using Antlr4.Tool;

    public class Lexer : Recognizer
    {
        public IDictionary<string, int> channels;
        public LexerFile file;
        public ICollection<string> modes;

        [ModelElement]
        public LinkedHashMap<Rule, RuleActionFunction> actionFuncs =
            new LinkedHashMap<Rule, RuleActionFunction>();

        public Lexer(OutputModelFactory factory, LexerFile file)
            : base(factory)
        {
            this.file = file; // who contains us?

            Grammar g = factory.GetGrammar();
            channels = new LinkedHashMap<string, int>(g.channelNameToValueMap);
            modes = ((LexerGrammar)g).modes.Keys;
        }
    }
}
