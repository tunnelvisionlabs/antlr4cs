// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
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
        internal readonly Grammar grammar;
        internal readonly Stack<FrequencySet<string>> frequencies;

        public ElementFrequenciesVisitor(Grammar grammar, ITreeNodeStream input)
            : base(input)
        {
            this.grammar = grammar;
            frequencies = new Stack<FrequencySet<string>>();
            frequencies.Push(new FrequencySet<string>());
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
         * Generate a frequency set as the union of two input sets, with the
         * values clipped to a specified maximum value. If an element is
         * contained in both sets, the value for the output, prior to clipping,
         * will be the sum of the two input values.
         *
         * @param a The first set.
         * @param b The second set.
         * @param clip The maximum value to allow for any output.
         * @return The sum of the two sets, with the individual elements clipped
         * to the maximum value gived by {@code clip}.
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
        }

        /*
         * Parser rules
         */

        protected override void EnterAlternative(AltAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitAlternative(AltAST tree)
        {
            frequencies.Push(CombineMax(frequencies.Pop(), frequencies.Pop()));
        }

        protected override void EnterElement(GrammarAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitElement(GrammarAST tree)
        {
            frequencies.Push(CombineAndClip(frequencies.Pop(), frequencies.Pop(), 2));
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
        }

        /*
         * Lexer rules
         */

        protected override void EnterLexerAlternative(GrammarAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitLexerAlternative(GrammarAST tree)
        {
            frequencies.Push(CombineMax(frequencies.Pop(), frequencies.Pop()));
        }

        protected override void EnterLexerElement(GrammarAST tree)
        {
            frequencies.Push(new FrequencySet<string>());
        }

        protected override void ExitLexerElement(GrammarAST tree)
        {
            frequencies.Push(CombineAndClip(frequencies.Pop(), frequencies.Pop(), 2));
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
        }
    }
}
