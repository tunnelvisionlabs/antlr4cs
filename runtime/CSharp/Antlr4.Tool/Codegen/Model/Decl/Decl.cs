// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    /** */
    public class Decl : SrcOp
    {
        public string name;
        public string decl;     // whole thing if copied from action
        public bool isLocal; // if local var (not in RuleContext struct)
        public StructDecl ctx;  // which context contains us? set by addDecl

        public Decl(OutputModelFactory factory, string name, string decl)
            : this(factory, name)
        {
            this.decl = decl;
        }

        public Decl(OutputModelFactory factory, string name)
            : base(factory)
        {
            this.name = name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        /** If same name, can't redefine, unless it's a getter */
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (!(obj is Decl))
                return false;
            // A() and label A are different
            if (obj is ContextGetterDecl)
                return false;
            return name.Equals(((Decl)obj).name);
        }
    }
}
