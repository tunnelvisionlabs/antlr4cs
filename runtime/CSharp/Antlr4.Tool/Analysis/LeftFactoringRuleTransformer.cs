/*
 * [The "BSD license"]
 *  Copyright (c) 2012 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *  1. Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *      derived from this software without specific prior written permission.
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
namespace Antlr4.Analysis
{
    using System.Collections.Generic;
    using Antlr4.Parse;
    using Antlr4.Runtime.Atn;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using ArgumentException = System.ArgumentException;
    using NotImplementedException = System.NotImplementedException;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;
    using System.Diagnostics;
    using InvalidOperationException = System.InvalidOperationException;
    using TokenConstants = Antlr4.Runtime.TokenConstants;
    using ITree = Antlr.Runtime.Tree.ITree;

    /**
     *
     * @author Sam Harwell
     */
    public class LeftFactoringRuleTransformer
    {
        public static readonly string LEFTFACTOR = "leftfactor";
        public static readonly string SUPPRESS_ACCESSOR = "suppressAccessor";

#if false
        private static readonly Logger LOGGER = Logger.getLogger(typeof(LeftFactoringRuleTransformer).Name);
#endif

        public GrammarRootAST _ast;
        public IDictionary<string, Rule> _rules;
        public Grammar _g;
        public AntlrTool _tool;

        private readonly GrammarASTAdaptor adaptor = new GrammarASTAdaptor();

        public LeftFactoringRuleTransformer([NotNull] GrammarRootAST ast, [NotNull] IDictionary<string, Rule> rules, [NotNull] Grammar g)
        {
            this._ast = ast;
            this._rules = rules;
            this._g = g;
            this._tool = g.tool;
        }

        public virtual void TranslateLeftFactoredRules()
        {
            // translate all rules marked for auto left factoring
            foreach (Rule r in _rules.Values)
            {
                if (Grammar.IsTokenName(r.name))
                {
                    continue;
                }

                ActionAST leftFactoredRules;
                if (!r.namedActions.TryGetValue(LEFTFACTOR, out leftFactoredRules) || leftFactoredRules == null)
                {
                    continue;
                }

                string leftFactoredRuleAction = leftFactoredRules.ToString();
                leftFactoredRuleAction = leftFactoredRuleAction.Substring(1, leftFactoredRuleAction.Length - 2);
                string[] rules = leftFactoredRuleAction.Split(',');
                for (int i = 0; i < rules.Length; i++)
                    rules[i] = rules[i].Trim();

                if (rules.Length == 0)
                {
                    continue;
                }

#if false
                LOGGER.log(Level.FINE, "Left factoring {0} out of alts in grammar rule {1}", new object[] { Arrays.toString(rules), r.name });
#endif

                ISet<GrammarAST> translatedBlocks = new HashSet<GrammarAST>();
                IList<GrammarAST> blocks = r.ast.GetNodesWithType(ANTLRParser.BLOCK);
                foreach (GrammarAST block in blocks)
                {
                    for (GrammarAST current = (GrammarAST)block.Parent; current != null; current = (GrammarAST)current.GetAncestor(ANTLRParser.BLOCK))
                    {
                        if (translatedBlocks.Contains(current))
                        {
                            // an enclosing decision was already factored
                            goto continueBlockLoop;
                        }
                    }

                    if (rules.Length != 1)
                    {
                        throw new NotImplementedException("Chained left factoring is not yet implemented.");
                    }

                    if (!TranslateLeftFactoredDecision(block, rules[0], false, DecisionFactorMode.COMBINED_FACTOR, true))
                    {
                        // couldn't translate the decision
                        continue;
                    }

                    translatedBlocks.Add(block);

                    continueBlockLoop:
                    ;
                }
            }
        }

        protected virtual bool ExpandOptionalQuantifiersForBlock(GrammarAST block, bool variant)
        {
            IList<GrammarAST> children = new List<GrammarAST>();
            for (int i = 0; i < block.ChildCount; i++)
            {
                GrammarAST child = (GrammarAST)block.GetChild(i);
                if (child.Type != ANTLRParser.ALT)
                {
                    children.Add(child);
                    continue;
                }

                GrammarAST expandedAlt = ExpandOptionalQuantifiersForAlt(child);
                if (expandedAlt == null)
                {
                    return false;
                }

                children.Add(expandedAlt);
            }

            GrammarAST newChildren = (GrammarAST)adaptor.Nil();
            newChildren.AddChildren(children);
            block.ReplaceChildren(0, block.ChildCount - 1, newChildren);
            block.FreshenParentAndChildIndexesDeeply();

            if (!variant && block.Parent is RuleAST)
            {
                RuleAST ruleAST = (RuleAST)block.Parent;
                string ruleName = ruleAST.GetChild(0).Text;
                Rule r = _rules[ruleName];
                IList<GrammarAST> blockAlts = block.GetAllChildrenWithType(ANTLRParser.ALT);
                r.numberOfAlts = blockAlts.Count;
                r.alt = new Alternative[blockAlts.Count + 1];
                for (int i = 0; i < blockAlts.Count; i++)
                {
                    r.alt[i + 1] = new Alternative(r, i + 1);
                    r.alt[i + 1].ast = (AltAST)blockAlts[i];
                }
            }

            return true;
        }

        protected virtual GrammarAST ExpandOptionalQuantifiersForAlt(GrammarAST alt)
        {
            if (alt.ChildCount == 0)
            {
                return null;
            }

            if (alt.GetChild(0).Type == ANTLRParser.OPTIONAL)
            {
                GrammarAST root = (GrammarAST)adaptor.Nil();

                GrammarAST alt2 = alt.DupTree();
                alt2.DeleteChild(0);
                if (alt2.ChildCount == 0)
                {
                    adaptor.AddChild(alt2, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                }

                alt.SetChild(0, alt.GetChild(0).GetChild(0));
                if (alt.GetChild(0).Type == ANTLRParser.BLOCK && alt.GetChild(0).ChildCount == 1 && alt.GetChild(0).GetChild(0).Type == ANTLRParser.ALT)
                {
                    GrammarAST list = (GrammarAST)adaptor.Nil();
                    foreach (object tree in ((GrammarAST)alt.GetChild(0).GetChild(0)).Children)
                    {
                        adaptor.AddChild(list, tree);
                    }

                    adaptor.ReplaceChildren(alt, 0, 0, list);
                }

                adaptor.AddChild(root, alt);
                adaptor.AddChild(root, alt2);
                return root;
            }
            else if (alt.GetChild(0).Type == ANTLRParser.CLOSURE)
            {
                GrammarAST root = (GrammarAST)adaptor.Nil();

                GrammarAST alt2 = alt.DupTree();
                alt2.DeleteChild(0);
                if (alt2.ChildCount == 0)
                {
                    adaptor.AddChild(alt2, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                }

                PlusBlockAST plusBlockAST = new PlusBlockAST(ANTLRParser.POSITIVE_CLOSURE, adaptor.CreateToken(ANTLRParser.POSITIVE_CLOSURE, "+"), null);
                for (int i = 0; i < alt.GetChild(0).ChildCount; i++)
                {
                    plusBlockAST.AddChild(alt.GetChild(0).GetChild(i));
                }

                alt.SetChild(0, plusBlockAST);

                adaptor.AddChild(root, alt);
                adaptor.AddChild(root, alt2);
                return root;
            }

            return alt;
        }

        protected virtual bool TranslateLeftFactoredDecision(GrammarAST block, string factoredRule, bool variant, DecisionFactorMode mode, bool includeFactoredElement)
        {
            if (mode == DecisionFactorMode.PARTIAL_UNFACTORED && includeFactoredElement)
            {
                throw new ArgumentException("Cannot include the factored element in unfactored alternatives.");
            }
            else if (mode == DecisionFactorMode.COMBINED_FACTOR && !includeFactoredElement)
            {
                throw new ArgumentException("Cannot return a combined answer without the factored element.");
            }

            if (!ExpandOptionalQuantifiersForBlock(block, variant))
            {
                return false;
            }

            IList<GrammarAST> alternatives = block.GetAllChildrenWithType(ANTLRParser.ALT);
            GrammarAST[] factoredAlternatives = new GrammarAST[alternatives.Count];
            GrammarAST[] unfactoredAlternatives = new GrammarAST[alternatives.Count];
            IntervalSet factoredIntervals = new IntervalSet();
            IntervalSet unfactoredIntervals = new IntervalSet();
            for (int i = 0; i < alternatives.Count; i++)
            {
                GrammarAST alternative = alternatives[i];
                if (mode.IncludeUnfactoredAlts())
                {
                    GrammarAST unfactoredAlt = TranslateLeftFactoredAlternative(alternative.DupTree(), factoredRule, variant, DecisionFactorMode.PARTIAL_UNFACTORED, false);
                    unfactoredAlternatives[i] = unfactoredAlt;
                    if (unfactoredAlt != null)
                    {
                        unfactoredIntervals.Add(i);
                    }
                }

                if (mode.IncludeFactoredAlts())
                {
                    GrammarAST factoredAlt = TranslateLeftFactoredAlternative(alternative, factoredRule, variant, mode == DecisionFactorMode.COMBINED_FACTOR ? DecisionFactorMode.PARTIAL_FACTORED : DecisionFactorMode.FULL_FACTOR, includeFactoredElement);
                    factoredAlternatives[i] = factoredAlt;
                    if (factoredAlt != null)
                    {
                        factoredIntervals.Add(alternative.ChildIndex);
                    }
                }
            }

            if (factoredIntervals.IsNil && !mode.IncludeUnfactoredAlts())
            {
                return false;
            }
            else if (unfactoredIntervals.IsNil && !mode.IncludeFactoredAlts())
            {
                return false;
            }

            if (unfactoredIntervals.IsNil && factoredIntervals.Count == alternatives.Count && mode.IncludeFactoredAlts() && !includeFactoredElement)
            {
                for (int i = 0; i < factoredAlternatives.Length; i++)
                {
                    GrammarAST translatedAlt = factoredAlternatives[i];
                    if (translatedAlt.ChildCount == 0)
                    {
                        adaptor.AddChild(translatedAlt, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                    }

                    adaptor.SetChild(block, i, translatedAlt);
                }

                return true;
            }
            else if (factoredIntervals.IsNil && unfactoredIntervals.Count == alternatives.Count && mode.IncludeUnfactoredAlts())
            {
                for (int i = 0; i < unfactoredAlternatives.Length; i++)
                {
                    GrammarAST translatedAlt = unfactoredAlternatives[i];
                    if (translatedAlt.ChildCount == 0)
                    {
                        adaptor.AddChild(translatedAlt, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                    }

                    adaptor.SetChild(block, i, translatedAlt);
                }

                return true;
            }

            if (mode == DecisionFactorMode.FULL_FACTOR)
            {
                return false;
            }

            /* for a, b, c being arbitrary `element` trees, this block performs
             * this transformation:
             *
             * factoredElement a
             * | factoredElement b
             * | factoredElement c
             * | ...
             *
             * ==>
             *
             * factoredElement (a | b | c | ...)
             */
            GrammarAST newChildren = (GrammarAST)adaptor.Nil();
            for (int i = 0; i < alternatives.Count; i++)
            {
                if (mode.IncludeFactoredAlts() && factoredIntervals.Contains(i))
                {
                    bool combineWithPrevious = i > 0 && factoredIntervals.Contains(i - 1) && (!mode.IncludeUnfactoredAlts() || !unfactoredIntervals.Contains(i - 1));
                    if (combineWithPrevious)
                    {
                        GrammarAST translatedAlt = factoredAlternatives[i];
                        if (translatedAlt.ChildCount == 0)
                        {
                            adaptor.AddChild(translatedAlt, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                        }

                        GrammarAST previous = (GrammarAST)newChildren.GetChild(newChildren.ChildCount - 1);

#if false
                        if (LOGGER.isLoggable(Level.FINE))
                        {
                            LOGGER.log(Level.FINE, previous.ToStringTree());
                            LOGGER.log(Level.FINE, translatedAlt.ToStringTree());
                        }
#endif

                        if (previous.ChildCount == 1 || previous.GetChild(1).Type != ANTLRParser.BLOCK)
                        {
                            GrammarAST newBlock = new BlockAST(adaptor.CreateToken(ANTLRParser.BLOCK, "BLOCK"));
                            GrammarAST newAlt = new AltAST(adaptor.CreateToken(ANTLRParser.ALT, "ALT"));
                            adaptor.AddChild(newBlock, newAlt);
                            while (previous.ChildCount > 1)
                            {
                                adaptor.AddChild(newAlt, previous.DeleteChild(1));
                            }

                            if (newAlt.ChildCount == 0)
                            {
                                adaptor.AddChild(newAlt, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                            }

                            adaptor.AddChild(previous, newBlock);
                        }

                        if (translatedAlt.ChildCount == 1 || translatedAlt.GetChild(1).Type != ANTLRParser.BLOCK)
                        {
                            GrammarAST newBlock = new BlockAST(adaptor.CreateToken(ANTLRParser.BLOCK, "BLOCK"));
                            GrammarAST newAlt = new AltAST(adaptor.CreateToken(ANTLRParser.ALT, "ALT"));
                            adaptor.AddChild(newBlock, newAlt);
                            while (translatedAlt.ChildCount > 1)
                            {
                                adaptor.AddChild(newAlt, translatedAlt.DeleteChild(1));
                            }

                            if (newAlt.ChildCount == 0)
                            {
                                adaptor.AddChild(newAlt, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                            }

                            adaptor.AddChild(translatedAlt, newBlock);
                        }

                        GrammarAST combinedBlock = (GrammarAST)previous.GetChild(1);
                        adaptor.AddChild(combinedBlock, translatedAlt.GetChild(1).GetChild(0));

#if false
                        if (LOGGER.isLoggable(Level.FINE))
                        {
                            LOGGER.log(Level.FINE, previous.ToStringTree());
                        }
#endif
                    }
                    else
                    {
                        GrammarAST translatedAlt = factoredAlternatives[i];
                        if (translatedAlt.ChildCount == 0)
                        {
                            adaptor.AddChild(translatedAlt, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                        }

                        adaptor.AddChild(newChildren, translatedAlt);
                    }
                }

                if (mode.IncludeUnfactoredAlts() && unfactoredIntervals.Contains(i))
                {
                    GrammarAST translatedAlt = unfactoredAlternatives[i];
                    if (translatedAlt.ChildCount == 0)
                    {
                        adaptor.AddChild(translatedAlt, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
                    }

                    adaptor.AddChild(newChildren, translatedAlt);
                }
            }

            adaptor.ReplaceChildren(block, 0, block.ChildCount - 1, newChildren);

            if (!variant && block.Parent is RuleAST)
            {
                RuleAST ruleAST = (RuleAST)block.Parent;
                string ruleName = ruleAST.GetChild(0).Text;
                Rule r = _rules[ruleName];
                IList<GrammarAST> blockAlts = block.GetAllChildrenWithType(ANTLRParser.ALT);
                r.numberOfAlts = blockAlts.Count;
                r.alt = new Alternative[blockAlts.Count + 1];
                for (int i = 0; i < blockAlts.Count; i++)
                {
                    r.alt[i + 1] = new Alternative(r, i + 1);
                    r.alt[i + 1].ast = (AltAST)blockAlts[i];
                }
            }

            return true;
        }

        protected virtual GrammarAST TranslateLeftFactoredAlternative(GrammarAST alternative, string factoredRule, bool variant, DecisionFactorMode mode, bool includeFactoredElement)
        {
            if (mode == DecisionFactorMode.PARTIAL_UNFACTORED && includeFactoredElement)
            {
                throw new ArgumentException("Cannot include the factored element in unfactored alternatives.");
            }
            else if (mode == DecisionFactorMode.COMBINED_FACTOR && !includeFactoredElement)
            {
                throw new ArgumentException("Cannot return a combined answer without the factored element.");
            }

            Debug.Assert(alternative.ChildCount > 0);

            if (alternative.GetChild(0).Type == ANTLRParser.EPSILON)
            {
                if (mode == DecisionFactorMode.PARTIAL_UNFACTORED)
                {
                    return alternative;
                }

                return null;
            }

            GrammarAST translatedElement = TranslateLeftFactoredElement((GrammarAST)alternative.GetChild(0), factoredRule, variant, mode, includeFactoredElement);
            if (translatedElement == null)
            {
                return null;
            }

            alternative.ReplaceChildren(0, 0, translatedElement);
            if (alternative.ChildCount == 0)
            {
                adaptor.AddChild(alternative, adaptor.Create(ANTLRParser.EPSILON, "EPSILON"));
            }

            Debug.Assert(alternative.ChildCount > 0);
            return alternative;
        }

        protected virtual GrammarAST TranslateLeftFactoredElement(GrammarAST element, string factoredRule, bool variant, DecisionFactorMode mode, bool includeFactoredElement)
        {
            if (mode == DecisionFactorMode.PARTIAL_UNFACTORED && includeFactoredElement)
            {
                throw new ArgumentException("Cannot include the factored element in unfactored alternatives.");
            }

            if (mode == DecisionFactorMode.COMBINED_FACTOR)
            {
                throw new InvalidOperationException("Cannot return a combined answer.");
            }

            Debug.Assert(!mode.IncludeFactoredAlts() || !mode.IncludeUnfactoredAlts());

            switch (element.Type)
            {
            case ANTLRParser.ASSIGN:
            case ANTLRParser.PLUS_ASSIGN:
                {
                    /* label=a
                     *
                     * ==>
                     *
                     * factoredElement label=a_factored
                     */

                    GrammarAST translatedChildElement = TranslateLeftFactoredElement((GrammarAST)element.GetChild(1), factoredRule, variant, mode, includeFactoredElement);
                    if (translatedChildElement == null)
                    {
                        return null;
                    }

                    RuleAST ruleAST = (RuleAST)element.GetAncestor(ANTLRParser.RULE);

#if false
                    LOGGER.log(Level.WARNING, "Could not left factor ''{0}'' out of decision in rule ''{1}'': labeled rule references are not yet supported.",
                        new object[] { factoredRule, ruleAST.GetChild(0).Text });
#endif

                    return null;
                    //if (!translatedChildElement.IsNil)
                    //{
                    //    GrammarAST root = (GrammarAST)adaptor.Nil();
                    //    object factoredElement = translatedChildElement;
                    //    if (outerRule)
                    //    {
                    //        adaptor.AddChild(root, factoredElement);
                    //    }

                    //    string action = string.Format("_localctx.{0} = (ContextType)_localctx.getParent().getChild(_localctx.getParent().getChildCount() - 1);", element.GetChild(0).Text);
                    //    adaptor.AddChild(root, new ActionAST(adaptor.CreateToken(ANTLRParser.ACTION, action)));
                    //    return root;
                    //}
                    //else
                    //{
                    //    GrammarAST root = (GrammarAST)adaptor.Nil();
                    //    object factoredElement = adaptor.DeleteChild(translatedChildElement, 0);
                    //    if (outerRule)
                    //    {
                    //        adaptor.AddChild(root, factoredElement);
                    //    }

                    //    adaptor.AddChild(root, element);
                    //    adaptor.ReplaceChildren(element, 1, 1, translatedChildElement);
                    //    return root;
                    //}
                }

            case ANTLRParser.RULE_REF:
                {
                    if (factoredRule.Equals(element.Token.Text))
                    {
                        if (!mode.IncludeFactoredAlts())
                        {
                            return null;
                        }

                        if (includeFactoredElement)
                        {
                            // this element is already left factored
                            return element;
                        }

                        GrammarAST root1 = (GrammarAST)adaptor.Nil();
                        root1.AddChild((ITree)adaptor.Create(TokenConstants.Epsilon, "EPSILON"));
                        root1.DeleteChild(0);
                        return root1;
                    }

                    Rule targetRule;
                    if (!_rules.TryGetValue(element.Token.Text, out targetRule))
                    {
                        return null;
                    }

                    RuleVariants ruleVariants = CreateLeftFactoredRuleVariant(targetRule, factoredRule);
                    switch (ruleVariants)
                    {
                    case RuleVariants.NONE:
                        if (!mode.IncludeUnfactoredAlts())
                        {
                            return null;
                        }

                        // just call the original rule (leave the element unchanged)
                        return element;

                    case RuleVariants.FULLY_FACTORED:
                        if (!mode.IncludeFactoredAlts())
                        {
                            return null;
                        }

                        break;

                    case RuleVariants.PARTIALLY_FACTORED:
                        break;

                    default:
                        throw new InvalidOperationException();
                    }

                    string marker = mode.IncludeFactoredAlts() ? ATNSimulator.RuleLfVariantMarker : ATNSimulator.RuleNolfVariantMarker;
                    element.SetText(element.Text + marker + factoredRule);

                    GrammarAST root = (GrammarAST)adaptor.Nil();

                    if (includeFactoredElement)
                    {
                        Debug.Assert(mode.IncludeFactoredAlts());
                        RuleRefAST factoredRuleRef = new RuleRefAST(adaptor.CreateToken(ANTLRParser.RULE_REF, factoredRule));
                        factoredRuleRef.SetOption(SUPPRESS_ACCESSOR, (GrammarAST)adaptor.Create(ANTLRParser.ID, "true"));
                        Rule factoredRuleDef = _rules[factoredRule];
                        if (factoredRuleDef is LeftRecursiveRule)
                        {
                            factoredRuleRef.SetOption(LeftRecursiveRuleTransformer.PRECEDENCE_OPTION_NAME, (GrammarAST)adaptor.Create(ANTLRParser.INT, "0"));
                        }

                        if (factoredRuleDef.args != null && factoredRuleDef.args.Size() > 0)
                        {
                            throw new NotImplementedException("Cannot left-factor rules with arguments yet.");
                        }

                        adaptor.AddChild(root, factoredRuleRef);
                    }

                    adaptor.AddChild(root, element);

                    return root;
                }

            case ANTLRParser.BLOCK:
                {
                    GrammarAST cloned = element.DupTree();
                    if (!TranslateLeftFactoredDecision(cloned, factoredRule, variant, mode, includeFactoredElement))
                    {
                        return null;
                    }

                    if (cloned.ChildCount != 1)
                    {
                        return null;
                    }

                    GrammarAST root = (GrammarAST)adaptor.Nil();
                    for (int i = 0; i < cloned.GetChild(0).ChildCount; i++)
                    {
                        adaptor.AddChild(root, cloned.GetChild(0).GetChild(i));
                    }

                    return root;
                }

            case ANTLRParser.POSITIVE_CLOSURE:
                {
                    /* a+
                     *
                     * =>
                     *
                     * factoredElement a_factored a*
                     */

                    GrammarAST originalChildElement = (GrammarAST)element.GetChild(0);
                    GrammarAST translatedElement = TranslateLeftFactoredElement(originalChildElement.DupTree(), factoredRule, variant, mode, includeFactoredElement);
                    if (translatedElement == null)
                    {
                        return null;
                    }

                    GrammarAST closure = new StarBlockAST(ANTLRParser.CLOSURE, adaptor.CreateToken(ANTLRParser.CLOSURE, "CLOSURE"), null);
                    adaptor.AddChild(closure, originalChildElement);

                    GrammarAST root = (GrammarAST)adaptor.Nil();
                    if (mode.IncludeFactoredAlts())
                    {
                        if (includeFactoredElement)
                        {
                            object factoredElement = adaptor.DeleteChild(translatedElement, 0);
                            adaptor.AddChild(root, factoredElement);
                        }
                    }
                    adaptor.AddChild(root, translatedElement);
                    adaptor.AddChild(root, closure);
                    return root;
                }

            case ANTLRParser.CLOSURE:
            case ANTLRParser.OPTIONAL:
                // not yet supported
                if (mode.IncludeUnfactoredAlts())
                {
                    return element;
                }

                return null;

            case ANTLRParser.DOT:
                // ref to imported grammar, not yet supported
                if (mode.IncludeUnfactoredAlts())
                {
                    return element;
                }

                return null;

            case ANTLRParser.ACTION:
            case ANTLRParser.SEMPRED:
                if (mode.IncludeUnfactoredAlts())
                {
                    return element;
                }

                return null;

            case ANTLRParser.WILDCARD:
            case ANTLRParser.STRING_LITERAL:
            case ANTLRParser.TOKEN_REF:
            case ANTLRParser.NOT:
                // terminals
                if (mode.IncludeUnfactoredAlts())
                {
                    return element;
                }

                return null;

            case ANTLRParser.EPSILON:
                // empty tree
                if (mode.IncludeUnfactoredAlts())
                {
                    return element;
                }

                return null;

            default:
                // unknown
                return null;
            }
        }

        protected virtual RuleVariants CreateLeftFactoredRuleVariant(Rule rule, string factoredElement)
        {
            RuleAST ast = (RuleAST)rule.ast.DupTree();
            BlockAST block = (BlockAST)ast.GetFirstChildWithType(ANTLRParser.BLOCK);

            RuleAST unfactoredAst = null;
            BlockAST unfactoredBlock = null;

            if (TranslateLeftFactoredDecision(block, factoredElement, true, DecisionFactorMode.FULL_FACTOR, false))
            {
                // all alternatives factored
            }
            else
            {
                ast = (RuleAST)rule.ast.DupTree();
                block = (BlockAST)ast.GetFirstChildWithType(ANTLRParser.BLOCK);
                if (!TranslateLeftFactoredDecision(block, factoredElement, true, DecisionFactorMode.PARTIAL_FACTORED, false))
                {
                    // no left factored alts
                    return RuleVariants.NONE;
                }

                unfactoredAst = (RuleAST)rule.ast.DupTree();
                unfactoredBlock = (BlockAST)unfactoredAst.GetFirstChildWithType(ANTLRParser.BLOCK);
                if (!TranslateLeftFactoredDecision(unfactoredBlock, factoredElement, true, DecisionFactorMode.PARTIAL_UNFACTORED, false))
                {
                    throw new InvalidOperationException("expected unfactored alts for partial factorization");
                }
            }

            /*
             * factored elements
             */
            {
                string variantName = ast.GetChild(0).Text + ATNSimulator.RuleLfVariantMarker + factoredElement;
                ((GrammarAST)ast.GetChild(0)).Token = adaptor.CreateToken(ast.GetChild(0).Type, variantName);
                GrammarAST ruleParent = (GrammarAST)rule.ast.Parent;
                ruleParent.InsertChild(rule.ast.ChildIndex + 1, ast);
                ruleParent.FreshenParentAndChildIndexes(rule.ast.ChildIndex);

                IList<GrammarAST> alts = block.GetAllChildrenWithType(ANTLRParser.ALT);
                Rule variant = new Rule(_g, ast.GetChild(0).Text, ast, alts.Count);
                _g.DefineRule(variant);
                for (int i = 0; i < alts.Count; i++)
                {
                    variant.alt[i + 1].ast = (AltAST)alts[i];
                }
            }

            /*
             * unfactored elements
             */
            if (unfactoredAst != null)
            {
                string variantName = unfactoredAst.GetChild(0).Text + ATNSimulator.RuleNolfVariantMarker + factoredElement;
                ((GrammarAST)unfactoredAst.GetChild(0)).Token = adaptor.CreateToken(unfactoredAst.GetChild(0).Type, variantName);
                GrammarAST ruleParent = (GrammarAST)rule.ast.Parent;
                ruleParent.InsertChild(rule.ast.ChildIndex + 1, unfactoredAst);
                ruleParent.FreshenParentAndChildIndexes(rule.ast.ChildIndex);

                IList<GrammarAST> alts = unfactoredBlock.GetAllChildrenWithType(ANTLRParser.ALT);
                Rule variant = new Rule(_g, unfactoredAst.GetChild(0).Text, unfactoredAst, alts.Count);
                _g.DefineRule(variant);
                for (int i = 0; i < alts.Count; i++)
                {
                    variant.alt[i + 1].ast = (AltAST)alts[i];
                }
            }

            /*
             * result
             */
            return unfactoredAst == null ? RuleVariants.FULLY_FACTORED : RuleVariants.PARTIALLY_FACTORED;
        }

        protected sealed class DecisionFactorMode
        {
            /**
             * Alternatives are factored where possible; results are combined, and
             * both factored and unfactored alternatives are included in the result.
             */
            public static readonly DecisionFactorMode COMBINED_FACTOR = new DecisionFactorMode(true, true);
            /**
             * Factors all alternatives of the decision. The factoring fails if the
             * decision contains one or more alternatives which cannot be factored.
             */
            public static readonly DecisionFactorMode FULL_FACTOR = new DecisionFactorMode(true, false);
            /**
             * Attempts to factor all alternatives of the decision. Alternatives
             * which could not be factored are not included in the result.
             */
            public static readonly DecisionFactorMode PARTIAL_FACTORED = new DecisionFactorMode(true, false);
            /**
             * Attempts to factor all alternatives of the decision, and returns the
             * remaining unfactored alternatives. Alternatives which could be
             * factored are not included in the result.
             */
            public static readonly DecisionFactorMode PARTIAL_UNFACTORED = new DecisionFactorMode(false, true);

            private readonly bool includeFactoredAlts;
            private readonly bool includeUnfactoredAlts;

            private DecisionFactorMode(bool includeFactoredAlts, bool includeUnfactoredAlts)
            {
                this.includeFactoredAlts = includeFactoredAlts;
                this.includeUnfactoredAlts = includeUnfactoredAlts;
            }

            public bool IncludeFactoredAlts()
            {
                return includeFactoredAlts;
            }

            public bool IncludeUnfactoredAlts()
            {
                return includeUnfactoredAlts;
            }
        }

        protected enum RuleVariants
        {
            NONE,
            PARTIALLY_FACTORED,
            FULLY_FACTORED,
        }
    }
}
