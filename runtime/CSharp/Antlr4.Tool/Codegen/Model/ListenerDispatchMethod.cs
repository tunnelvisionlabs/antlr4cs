// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    public class ListenerDispatchMethod : DispatchMethod
    {
        public bool isEnter;

        public ListenerDispatchMethod(OutputModelFactory factory, bool isEnter)
            : base(factory)
        {
            this.isEnter = isEnter;
        }
    }
}
