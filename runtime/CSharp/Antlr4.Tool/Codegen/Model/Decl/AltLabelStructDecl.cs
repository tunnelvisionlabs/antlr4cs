// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model.Decl
{
    using System.Collections.Generic;
    using Antlr4.Tool;

    /** A StructDecl to handle a '#' label on alt */
    public class AltLabelStructDecl : StructDecl
    {
        public AltLabelStructDecl(OutputModelFactory factory, Rule r, string label)
            : base(factory, r)
        {
            this.name = // override name set in super to the label ctx
                factory.GetTarget().GetAltLabelContextStructName(label);
            derivedFromName = label;
        }

        public override void AddDispatchMethods(Rule r)
        {
            dispatchMethods = new List<DispatchMethod>();
            if (factory.GetGrammar().tool.gen_listener)
            {
                dispatchMethods.Add(new ListenerDispatchMethod(factory, true));
                dispatchMethods.Add(new ListenerDispatchMethod(factory, false));
            }
            if (factory.GetGrammar().tool.gen_visitor)
            {
                dispatchMethods.Add(new VisitorDispatchMethod(factory));
            }
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (!(obj is AltLabelStructDecl))
                return false;

            return name.Equals(((AltLabelStructDecl)obj).name);
        }
    }
}
