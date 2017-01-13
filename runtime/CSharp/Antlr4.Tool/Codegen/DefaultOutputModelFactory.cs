// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
