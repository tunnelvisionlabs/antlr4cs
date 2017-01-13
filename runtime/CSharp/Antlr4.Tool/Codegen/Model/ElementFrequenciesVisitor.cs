// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Antlr4.Analysis;
    using Antlr4.Misc;
    using Antlr4.Parse;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using ITreeNodeStream = Antlr.Runtime.Tree.ITreeNodeStream;
    using Math = System.Math;

    public class ElementFrequenciesVisitor : GrammarTreeVisitor
    {
        /**
         * This special value means "no set", and is used by {@link #minFrequencies}
         * to ensure that {@link #combineMin} doesn't merge an empty set (all zeros)
         * with the results of the first alternative.
         */
        private static readonly FrequencySet<string> SENTINEL = new FrequencySet<string>();

        internal readonly Grammar grammar;
        internal readonly Stack<FrequencySet<string>> frequencies;
        private readonly Stack<FrequencySet<string>> minFrequencies;

        public ElementFrequenciesVisitor(Grammar grammar, ITreeNodeStream input)
            : base(input)
        {
            this.grammar = grammar;
            frequencies = new Stack<FrequencySet<string>>();
            frequencies.Push(new FrequencySet<string>());
            minFrequencies = new Stack<FrequencySet<string>>();
            minFrequencies.Push(SENTINEL);
        }

        internal FrequencySet<string> GetMinFrequencies()
        {
            Debug.Assert(minFrequencies.Count == 1);
            Debug.Assert(minFrequencies.Peek() != SENTINEL);
            Debug.Assert(SENTINEL.Count == 0);

            return minFrequencies.Peek();
        }

        /*
         * Common
         */

        /**
         * Generate a frequency set as the union of two input sets. If an
         * element is contained in both sets, the value for the output will be
         * the maximum of the two input values.
         *
         * @param a The first set.
         * @param b The second set.
         * @return The union of the two sets, with the maximum value chosen
         * whenever both sets contain the same key.
         */
        protected static FrequencySet<string> CombineMax(FrequencySet<string> a, FrequencySet<string> b)
        {
            FrequencySet<string> result = CombineAndClip(a, b, 1);
            foreach (KeyValuePair<string, StrongBox<int>> entry in a)
            {
                result[entry.Key].Value = entry.Value.Value;
            }

            foreach (KeyValuePair<string, StrongBox<int>> entry in b)
            {
                StrongBox<int> slot = result[entry.Key];
                slot.Value = Math.Max(slot.Value, entry.Value.Value);
            }

            return result;
        }

        /**
         * Generate a frequency set as the union of two input sets. If an
         * element is contained in both sets, the value for the output will be
         * the minimum of the two input values.
         *
         * @param a The first set.
         * @param b The second set. If this set is {@link #SENTINEL}, it is treated
         * as though no second set were provided.
         * @return The union of the two sets, with the minimum value chosen
         * whenever both sets contain the same key.
         */
        protected static FrequencySet<string> combineMin(FrequencySet<string> a, FrequencySet<string> b)
        {
            if (b == SENTINEL)
            {
                return a;
            }

            Debug.Assert(a != SENTINEL);
            FrequencySet<string> result = CombineAndClip(a, b, 1);
            foreach (KeyValuePair<string, StrongBox<int>> entry in result)
            {
                entry.Value.Value = Math.Min(a.GetCount(entry.Key), b.GetCount(entry.Key));
            }

            return result;
        }

        /**
         * Generate a frequency set as the union of two input sets, with the
         * values clipped to a specified maximum value. If an element is
         * contained in both sets, the value for the output, prior to clipping,
         * will be the sum of the two input values.
         *
         * @param a The first set.
         * @param b The second set.
         * @param clip The maximum value to allow for any output.
         * @return The sum of the two sets, with the individual elements clipped
         * to the maximum value given by {@code clip}.
         */
        protected static FrequencySet<string> CombineAndClip(FrequencySet<string> a, FrequencySet<string> b, int clip)
        {
            FrequencySet<string> result = new FrequencySet<string>();
            foreach (KeyValuePair<string, StrongBox<int>> entry in a)
            {
                for (int i = 0; i < entry.Value.Value; i++)
                {
                    result.Add(entry.Key);
                }
            }

            foreach (KeyValuePair<string, StrongBox<int>> entry in b)
            {
                for (int i = 0; i < entry.Value.Value; i++)
                {
                    result.Add(entry.Key);
                }
            }

            foreach (KeyValuePair<string, StrongBox<int>> entry in result)
            {
                entry.Value.Value = Math.Min(entry.Value.Value, clip);
            }

            return result;
        }

        public override void TokenRef(TerminalAST @ref)
        {
            frequencies.Peek().Add(@ref.Text);
            minFrequencies.Peek().Add(@ref.Text);
        }

        public override void RuleRef(GrammarAST @ref, ActionAST arg)
        {
            if (@ref is GrammarASTWithOptions)
            {
                GrammarASTWithOptions grammarASTWithOptions = (GrammarASTWithOptions)@ref;
                if (bool.Parse(grammarASTWithOptions.GetOptionString(LeftFactoringRuleTransformer.SUPPRESS_ACCESSOR) ?? "false"))
                {
                    return;
                }
            }

            frequencies.Peek().Add(RuleFunction.GetLabelName(grammar, @ref));
            minFrequencies.Peek().Add(RuleFunction.GetLabelName(grammar, @ref));
        }

        /*
         * Parser rules
         */

        protected override void EnterAlternative(AltAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
            minFrequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitAlternative(AltAST tree)
        {
            frequencies.Push(CombineMax(frequencies.Pop(), frequencies.Pop()));
            minFrequencies.Push(combineMin(minFrequencies.Pop(), minFrequencies.Pop()));
        }

        protected override void EnterElement(GrammarAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
            minFrequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitElement(GrammarAST tree)
        {
            frequencies.Push(CombineAndClip(frequencies.Pop(), frequencies.Pop(), 2));
            minFrequencies.Push(CombineAndClip(minFrequencies.Pop(), minFrequencies.Pop(), 2));
        }

        protected override void ExitSubrule(GrammarAST tree)
        {
            if (tree.Type == CLOSURE || tree.Type == POSITIVE_CLOSURE)
            {
                foreach (KeyValuePair<string, StrongBox<int>> entry in frequencies.Peek())
                {
                    entry.Value.Value = 2;
                }
            }

            if (tree.Type == CLOSURE)
            {
                // Everything inside a closure is optional, so the minimum
                // number of occurrences for all elements is 0.
                minFrequencies.Peek().Clear();
            }
        }

        /*
         * Lexer rules
         */

        protected override void EnterLexerAlternative(GrammarAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
            minFrequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitLexerAlternative(GrammarAST tree)
        {
            frequencies.Push(CombineMax(frequencies.Pop(), frequencies.Pop()));
            minFrequencies.Push(combineMin(minFrequencies.Pop(), minFrequencies.Pop()));
        }

        protected override void EnterLexerElement(GrammarAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
            minFrequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitLexerElement(GrammarAST tree)
        {
            frequencies.Push(CombineAndClip(frequencies.Pop(), frequencies.Pop(), 2));
            minFrequencies.Push(CombineAndClip(minFrequencies.Pop(), minFrequencies.Pop(), 2));
        }

        protected override void ExitLexerSubrule(GrammarAST tree)
        {
            if (tree.Type == CLOSURE || tree.Type == POSITIVE_CLOSURE)
            {
                foreach (KeyValuePair<string, StrongBox<int>> entry in frequencies.Peek())
                {
                    entry.Value.Value = 2;
                }
            }

            if (tree.Type == CLOSURE)
            {
                // Everything inside a closure is optional, so the minimum
                // number of occurrences for all elements is 0.
                minFrequencies.Peek().Clear();
            }
        }
    }
}
