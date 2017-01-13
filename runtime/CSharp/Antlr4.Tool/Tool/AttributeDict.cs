// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using System.Collections.Generic;
    using Antlr4.Misc;
    using Antlr4.Tool.Ast;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;

    /** Track the attributes within retval, arg lists etc...
     *  <p>
     *  Each rule has potentially 3 scopes: return values,
     *  parameters, and an implicitly-named scope (i.e., a scope defined in a rule).
     *  Implicitly-defined scopes are named after the rule; rules and scopes then
     *  must live in the same name space--no collisions allowed.</p>
     */
    public class AttributeDict
    {
        public string name;
        public GrammarAST ast;
        public DictType type;

        /** All {@link Token} scopes (token labels) share the same fixed scope of
         *  of predefined attributes.  I keep this out of the {@link Token}
         *  interface to avoid a runtime type leakage.
         */
        public static readonly AttributeDict predefinedTokenDict = new AttributeDict(DictType.TOKEN);
        static AttributeDict()
        {
            predefinedTokenDict.Add(new Attribute("text"));
            predefinedTokenDict.Add(new Attribute("type"));
            predefinedTokenDict.Add(new Attribute("line"));
            predefinedTokenDict.Add(new Attribute("index"));
            predefinedTokenDict.Add(new Attribute("pos"));
            predefinedTokenDict.Add(new Attribute("channel"));
            predefinedTokenDict.Add(new Attribute("int"));
        }

        public enum DictType
        {
            ARG, RET, LOCAL, TOKEN,
            PREDEFINED_RULE, PREDEFINED_LEXER_RULE,
        }

        /** The list of {@link Attribute} objects. */
        [NotNull]
        public readonly LinkedHashMap<string, Attribute> attributes =
            new LinkedHashMap<string, Attribute>();

        public AttributeDict()
        {
        }

        public AttributeDict(DictType type)
        {
            this.type = type;
        }

        public virtual Attribute Add(Attribute a)
        {
            a.dict = this;
            return attributes[a.name] = a;
        }

        public virtual Attribute Get(string name)
        {
            Attribute result;
            if (!attributes.TryGetValue(name, out result))
                return null;

            return result;
        }

        public virtual string GetName()
        {
            return name;
        }

        public virtual int Size()
        {
            return attributes.Count;
        }

        /** Return the set of keys that collide from
         *  {@code this} and {@code other}.
         */
        [return: NotNull]
        public ISet<string> Intersection([Nullable] AttributeDict other)
        {
            if (other == null || other.Size() == 0 || Size() == 0)
            {
                return new HashSet<string>();
            }

            ISet<string> result = new HashSet<string>(attributes.Keys);
            result.IntersectWith(other.attributes.Keys);
            return result;
        }

        public override string ToString()
        {
            return GetName() + ":" + attributes;
        }
    }
}
