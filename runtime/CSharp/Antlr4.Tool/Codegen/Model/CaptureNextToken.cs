// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    public class CaptureNextToken : SrcOp
    {
        public string varName;

        public CaptureNextToken(OutputModelFactory factory, string varName)
            : base(factory)
        {
            this.varName = varName;
        }
    }
}
