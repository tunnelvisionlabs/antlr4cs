// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Semantics
{
    using Antlr4.Parse;
    using IToken = Antlr.Runtime.IToken;

    public class BlankActionSplitterListener : ActionSplitterListener
    {
        public virtual void QualifiedAttr(string expr, IToken x, IToken y)
        {
        }

        public virtual void SetAttr(string expr, IToken x, IToken rhs)
        {
        }

        public virtual void Attr(string expr, IToken x)
        {
        }

        public virtual void TemplateInstance(string expr)
        {
        }

        public virtual void NonLocalAttr(string expr, IToken x, IToken y)
        {
        }

        public virtual void SetNonLocalAttr(string expr, IToken x, IToken y, IToken rhs)
        {
        }

        public virtual void IndirectTemplateInstance(string expr)
        {
        }

        public virtual void SetExprAttribute(string expr)
        {
        }

        public virtual void SetSTAttribute(string expr)
        {
        }

        public virtual void TemplateExpr(string expr)
        {
        }

        public virtual void Text(string text)
        {
        }
    }
}
