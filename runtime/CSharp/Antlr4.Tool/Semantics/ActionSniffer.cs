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

namespace Antlr4.Semantics
{
    using System.Collections.Generic;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using ANTLRStringStream = Antlr.Runtime.ANTLRStringStream;
    using IToken = Antlr.Runtime.IToken;

    /** Find token and rule refs plus refs to them in actions;
     *  side-effect: update Alternatives
     */
    public class ActionSniffer : BlankActionSplitterListener
    {
        public Grammar g;
        public Rule r;          // null if action outside of rule
        public Alternative alt; // null if action outside of alt; could be in rule
        public ActionAST node;
        public IToken actionToken; // token within action
        public ErrorManager errMgr;

        public ActionSniffer(Grammar g, Rule r, Alternative alt, ActionAST node, IToken actionToken)
        {
            this.g = g;
            this.r = r;
            this.alt = alt;
            this.node = node;
            this.actionToken = actionToken;
            this.errMgr = g.tool.errMgr;
        }

        public virtual void ExamineAction()
        {
            //System.out.println("examine "+actionToken);
            ANTLRStringStream @in = new ANTLRStringStream(actionToken.Text);
            @in.Line = actionToken.Line;
            @in.CharPositionInLine = actionToken.CharPositionInLine;
            ActionSplitter splitter = new ActionSplitter(@in, this);
            // forces eval, triggers listener methods
            node.chunks = splitter.GetActionTokens();
        }

        public virtual void ProcessNested(IToken actionToken)
        {
            ANTLRStringStream @in = new ANTLRStringStream(actionToken.Text);
            @in.Line = actionToken.Line;
            @in.CharPositionInLine = actionToken.CharPositionInLine;
            ActionSplitter splitter = new ActionSplitter(@in, this);
            // forces eval, triggers listener methods
            splitter.GetActionTokens();
        }

        public override void Attr(string expr, IToken x)
        {
            TrackRef(x);
        }

        public override void QualifiedAttr(string expr, IToken x, IToken y)
        {
            TrackRef(x);
        }

        public override void SetAttr(string expr, IToken x, IToken rhs)
        {
            TrackRef(x);
            ProcessNested(rhs);
        }

        public override void SetNonLocalAttr(string expr, IToken x, IToken y, IToken rhs)
        {
            ProcessNested(rhs);
        }

        public virtual void TrackRef(IToken x)
        {
            IList<TerminalAST> xRefs;
            if (alt.tokenRefs.TryGetValue(x.Text, out xRefs) && xRefs != null)
            {
                alt.tokenRefsInActions.Map(x.Text, node);
            }

            IList<GrammarAST> rRefs;
            if (alt.ruleRefs.TryGetValue(x.Text, out rRefs) && rRefs != null)
            {
                alt.ruleRefsInActions.Map(x.Text, node);
            }
        }
    }
}
