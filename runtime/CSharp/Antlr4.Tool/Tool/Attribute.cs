// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using IToken = Antlr.Runtime.IToken;

    /** Track the names of attributes define in arg lists, return values,
     *  scope blocks etc...
     */
    public class Attribute
    {
        /** The entire declaration such as "String foo;" */
        public string decl;

        /** The type; might be empty such as for Python which has no static typing */
        public string type;

        /** The name of the attribute "foo" */
        public string name;

        /** A {@link Token} giving the position of the name of this attribute in the grammar. */
        public IToken token;

        /** The optional attribute initialization expression */
        public string initValue;

        /** Who contains us? */
        public AttributeDict dict;

        public Attribute()
        {
        }

        public Attribute(string name)
            : this(name, null)
        {
        }

        public Attribute(string name, string decl)
        {
            this.name = name;
            this.decl = decl;
        }

        public override string ToString()
        {
            if (initValue != null)
            {
                return type + " " + name + "=" + initValue;
            }
            return type + " " + name;
        }
    }
}
