// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Reflection;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Misc
{
    /// <summary>
    /// A proxy for the real org.antlr.v4.gui.TestRig that we moved to tool
    /// artifact from runtime.
    /// </summary>
    /// <since>4.5.1</since>
    [System.ObsoleteAttribute]
    public class TestRig
    {
        public static void Main(string[] args)
        {
            try
            {
                Type testRigClass = Sharpen.Runtime.GetType("org.antlr.v4.gui.TestRig");
                System.Console.Error.WriteLine("Warning: TestRig moved to org.antlr.v4.gui.TestRig; calling automatically");
                try
                {
                    MethodInfo mainMethod = testRigClass.GetMethod("main", typeof(string[]));
                    mainMethod.Invoke(null, (object)args);
                }
                catch (Exception)
                {
                    System.Console.Error.WriteLine("Problems calling org.antlr.v4.gui.TestRig.main(args)");
                }
            }
            catch (TypeLoadException)
            {
                System.Console.Error.WriteLine("Use of TestRig now requires the use of the tool jar, antlr-4.X-complete.jar");
                System.Console.Error.WriteLine("Maven users need group ID org.antlr and artifact ID antlr4");
            }
        }
    }
}
