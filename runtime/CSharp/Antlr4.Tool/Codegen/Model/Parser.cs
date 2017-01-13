// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;

    public class Parser : Recognizer
    {
        public ParserFile file;

        [ModelElement]
        public IList<RuleFunction> funcs = new List<RuleFunction>();

        public Parser(OutputModelFactory factory, ParserFile file)
            : base(factory)
        {
            this.file = file; // who contains us?
        }
    }
}
