// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Misc
{
    using NotImplementedException = System.NotImplementedException;

    public class LogManager
    {
        internal void Log(string component, string message)
        {
            // Currently doesn't do anything.
        }

        public string Save()
        {
            throw new NotImplementedException();
        }
    }
}
