// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4
{
    using Environment = System.Environment;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Environment.ExitCode = AntlrTool.Main(args);
        }
    }
}
