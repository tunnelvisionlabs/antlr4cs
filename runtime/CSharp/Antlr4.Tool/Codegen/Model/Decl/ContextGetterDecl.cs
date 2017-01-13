// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
