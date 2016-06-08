// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* [The "BSD license"]
*  Copyright (c) 2012 Terence Parr
*  Copyright (c) 2012 Sam Harwell
*  All rights reserved.
*
*  Redistribution and use in source and binary forms, with or without
*  modification, are permitted provided that the following conditions
*  are met:
*
*  1. Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*  2. Redistributions in binary form must reproduce the above copyright
*     notice, this list of conditions and the following disclaimer in the
*     documentation and/or other materials provided with the distribution.
*  3. The name of the author may not be used to endorse or promote products
*     derived from this software without specific prior written permission.
*
*  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
*  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
*  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
*  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
*  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
*  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
*  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
*  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
*  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
*  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
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
