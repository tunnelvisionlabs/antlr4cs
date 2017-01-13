// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Misc;
    using Antlr4.Tool;

    public class RuleActionFunction : OutputModelObject
    {
        public string name;
        public string ctxType;
        public int ruleIndex;

        /** Map actionIndex to Action */
        [ModelElement]
        public LinkedHashMap<int, Action> actions =
            new LinkedHashMap<int, Action>();

        public RuleActionFunction(OutputModelFactory factory, Rule r, string ctxType)
            : base(factory)
        {
            name = r.name;
            ruleIndex = r.index;
            this.ctxType = ctxType;
        }
    }
}
