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

namespace Antlr4.Tool
{
    using Antlr4.Tool.Ast;

    /** Grammars, rules, and alternatives all have symbols visible to
     *  actions.  To evaluate attr exprs, ask action for its resolver
     *  then ask resolver to look up various symbols. Depending on the context,
     *  some symbols are available at some aren't.
     *
     *  Alternative level:
     *
     *  $x		Attribute: rule arguments, return values, predefined rule prop.
     * 			AttributeDict: references to tokens and token labels in the
     * 			current alt (including any elements within subrules contained
     * 			in that outermost alt). x can be rule with scope or a global scope.
     * 			List label: x is a token/rule list label.
     *  $x.y	Attribute: x is surrounding rule, rule/token/label ref
     *  $s::y	Attribute: s is any rule with scope or global scope; y is prop within
     *
     *  Rule level:
     *
     *  $x		Attribute: rule arguments, return values, predefined rule prop.
     * 			AttributeDict: references to token labels in *any* alt. x can
     * 			be any rule with scope or global scope.
     * 			List label: x is a token/rule list label.
     *  $x.y	Attribute: x is surrounding rule, label ref (in any alts)
     *  $s::y	Attribute: s is any rule with scope or global scope; y is prop within
     *
     *  Grammar level:
     *
     *  $s		AttributeDict: s is a global scope
     *  $s::y	Attribute: s is a global scope; y is prop within
     */
    public interface AttributeResolver
    {
        bool ResolvesToListLabel(string x, ActionAST node);
        bool ResolvesToLabel(string x, ActionAST node);
        bool ResolvesToAttributeDict(string x, ActionAST node);
        bool ResolvesToToken(string x, ActionAST node);
        Attribute ResolveToAttribute(string x, ActionAST node);
        Attribute ResolveToAttribute(string x, string y, ActionAST node);
    }
}
