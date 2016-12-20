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

namespace Antlr4.Codegen.Model.Decl
{
    using Antlr4.Runtime.Misc;

    public abstract class ContextGetterDecl : Decl
    {
        protected ContextGetterDecl(OutputModelFactory factory, string name)
            : base(factory, name)
        {
        }

        /** Not used for output; just used to distinguish between decl types
         *  to avoid dups.
         */
        public virtual string GetArgType()
        {
            // assume no args
            return "";
        }

        public override int GetHashCode()
        {
            int hash = MurmurHash.Initialize();
            hash = MurmurHash.Update(hash, name);
            hash = MurmurHash.Update(hash, GetArgType());
            hash = MurmurHash.Finish(hash, 2);
            return hash;
        }

        /** Make sure that a getter does not equal a label. X() and X are ok.
         *  OTOH, treat X() with two diff return values as the same.  Treat
         *  two X() with diff args as different.
         */
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            // A() and label A are different
            if (!(obj is ContextGetterDecl))
                return false;
            return
                name.Equals(((Decl)obj).name) &&
                    GetArgType().Equals(((ContextGetterDecl)obj).GetArgType());
        }
    }
}
