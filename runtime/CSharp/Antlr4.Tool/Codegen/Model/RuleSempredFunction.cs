// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using Antlr4.Tool;

    public class RuleSempredFunction : RuleActionFunction
    {
        public RuleSempredFunction(OutputModelFactory factory, Rule r, string ctxType)
            : base(factory, r, ctxType)
        {
        }
    }
}
