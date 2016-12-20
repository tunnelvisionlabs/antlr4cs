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
    using System.Collections.Generic;
    using Antlr4.Misc;
    using Antlr4.Tool;

    /** This object models the structure holding all of the parameters,
     *  return values, local variables, and labels associated with a rule.
     */
    public class StructDecl : Decl
    {
        public string derivedFromName; // rule name or label name
        public bool provideCopyFrom;
        [ModelElement]
        public OrderedHashSet<Decl> attrs = new OrderedHashSet<Decl>();
        [ModelElement]
        public OrderedHashSet<Decl> getters = new OrderedHashSet<Decl>();
        [ModelElement]
        public ICollection<AttributeDecl> ctorAttrs;
        [ModelElement]
        public IList<DispatchMethod> dispatchMethods;
        [ModelElement]
        public IList<OutputModelObject> interfaces;
        [ModelElement]
        public IList<OutputModelObject> extensionMembers;

        public StructDecl(OutputModelFactory factory, Rule r)
            : base(factory, factory.GetTarget().GetRuleFunctionContextStructName(r))
        {
            AddDispatchMethods(r);
            derivedFromName = r.name;
            provideCopyFrom = r.HasAltSpecificContexts();
        }

        public virtual void AddDispatchMethods(Rule r)
        {
            dispatchMethods = new List<DispatchMethod>();
            if (!r.HasAltSpecificContexts())
            {
                // no enter/exit for this ruleContext if rule has labels
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
        }

        public virtual void AddDecl(Decl d)
        {
            d.ctx = this;
            if (d is ContextGetterDecl)
                getters.Add(d);
            else
                attrs.Add(d);
        }

        public virtual void AddDecl(Attribute a)
        {
            AddDecl(new AttributeDecl(factory, a));
        }

        public virtual void AddDecls(ICollection<Attribute> attrList)
        {
            foreach (Attribute a in attrList)
                AddDecl(a);
        }

        public virtual void ImplementInterface(OutputModelObject value)
        {
            if (interfaces == null)
            {
                interfaces = new List<OutputModelObject>();
            }

            interfaces.Add(value);
        }

        public virtual void AddExtensionMember(OutputModelObject member)
        {
            if (extensionMembers == null)
            {
                extensionMembers = new List<OutputModelObject>();
            }

            extensionMembers.Add(member);
        }

        public virtual bool IsEmpty()
        {
            return attrs.Count == 0;
        }
    }
}
