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

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Antlr4.Codegen.Model;
    using Antlr4.Codegen.Model.Decl;
    using Antlr4.StringTemplate;
    using Antlr4.Tool;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NotSupportedException = System.NotSupportedException;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;

    /** Create output objects for elements *within* rule functions except
     *  buildOutputModel() which builds outer/root model object and any
     *  objects such as RuleFunction that surround elements in rule
     *  functions.
     */
    public abstract class DefaultOutputModelFactory : BlankOutputModelFactory
    {
        // Interface to outside world
        [NotNull]
        public readonly Grammar g;
        [NotNull]
        public readonly CodeGenerator gen;

        public OutputModelController controller;

        protected DefaultOutputModelFactory([NotNull] CodeGenerator gen)
        {
            this.gen = gen;
            this.g = gen.g;

            if (gen.GetTarget() == null)
            {
                throw new NotSupportedException("Cannot build an output model without a target.");
            }
        }

        public override void SetController(OutputModelController controller)
        {
            this.controller = controller;
        }

        public override OutputModelController GetController()
        {
            return controller;
        }

        public override IList<SrcOp> RulePostamble(RuleFunction function, Rule r)
        {
            if (r.namedActions.ContainsKey("after") || r.namedActions.ContainsKey("finally"))
            {
                // See OutputModelController.buildLeftRecursiveRuleFunction
                // and Parser.exitRule for other places which set stop.
                CodeGenerator gen = GetGenerator();
                TemplateGroup codegenTemplates = gen.GetTemplates();
                Template setStopTokenAST = codegenTemplates.GetInstanceOf("recRuleSetStopToken");
                Action setStopTokenAction = new Action(this, function.ruleCtx, setStopTokenAST);
                IList<SrcOp> ops = new List<SrcOp>(1);
                ops.Add(setStopTokenAction);
                return ops;
            }

            return base.RulePostamble(function, r);
        }

        // Convenience methods

        [return: NotNull]
        public override Grammar GetGrammar()
        {
            return g;
        }

        public override CodeGenerator GetGenerator()
        {
            return gen;
        }

        public override AbstractTarget GetTarget()
        {
            AbstractTarget target = GetGenerator().GetTarget();
            Debug.Assert(target != null);
            return target;
        }

        public override OutputModelObject GetRoot()
        {
            return controller.GetRoot();
        }

        public override RuleFunction GetCurrentRuleFunction()
        {
            return controller.GetCurrentRuleFunction();
        }

        public override Alternative GetCurrentOuterMostAlt()
        {
            return controller.GetCurrentOuterMostAlt();
        }

        public override CodeBlock GetCurrentBlock()
        {
            return controller.GetCurrentBlock();
        }

        public override CodeBlockForOuterMostAlt GetCurrentOuterMostAlternativeBlock()
        {
            return controller.GetCurrentOuterMostAlternativeBlock();
        }

        public override int GetCodeBlockLevel()
        {
            return controller.codeBlockLevel;
        }

        public override int GetTreeLevel()
        {
            return controller.treeLevel;
        }

        // MISC

        [return: NotNull]
        public static IList<SrcOp> List(params SrcOp[] values)
        {
            return new List<SrcOp>(values);
        }

        [return: NotNull]
        public static IList<SrcOp> List(IEnumerable<SrcOp> values)
        {
            return new List<SrcOp>(values);
        }

        [return: Nullable]
        public virtual Decl GetCurrentDeclForName(string name)
        {
            if (GetCurrentBlock().locals == null)
                return null;

            foreach (Decl d in GetCurrentBlock().locals.Elements)
            {
                if (d.name.Equals(name))
                    return d;
            }

            return null;
        }
    }
}
