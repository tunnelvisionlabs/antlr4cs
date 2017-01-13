// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    /** x=ID or implicit _tID label */
    public class TokenDecl : Decl
    {
        public bool isImplicit;

        public TokenDecl(OutputModelFactory factory, string varName)
            : base(factory, varName)
        {
        }
    }
}
