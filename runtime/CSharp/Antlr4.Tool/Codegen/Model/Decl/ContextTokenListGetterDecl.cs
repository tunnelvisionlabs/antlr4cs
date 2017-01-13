// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    /** {@code public List&lt;Token&gt; X() { }
     *  public Token X(int i) { }}
     */
    public class ContextTokenListGetterDecl : ContextGetterDecl
    {
        public ContextTokenListGetterDecl(OutputModelFactory factory, string name)
            : base(factory, name)
        {
        }
    }
}
