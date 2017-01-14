// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
