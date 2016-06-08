// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// This is the base class for gathering detailed information about prediction
    /// events which occur during parsing.
    /// </summary>
    /// <remarks>
    /// This is the base class for gathering detailed information about prediction
    /// events which occur during parsing.
    /// Note that we could record the parser call stack at the time this event
    /// occurred but in the presence of left recursive rules, the stack is kind of
    /// meaningless. It's better to look at the individual configurations for their
    /// individual stacks. Of course that is a
    /// <see cref="PredictionContext"/>
    /// object
    /// not a parse tree node and so it does not have information about the extent
    /// (start...stop) of the various subtrees. Examining the stack tops of all
    /// configurations provide the return states for the rule invocations.
    /// From there you can get the enclosing rule.
    /// </remarks>
    /// <since>4.3</since>
    public class DecisionEventInfo
    {
        /// <summary>The invoked decision number which this event is related to.</summary>
        /// <seealso cref="ATN.decisionToState"/>
        public readonly int decision;

        /// <summary>
        /// The simulator state containing additional information relevant to the
        /// prediction state when the current event occurred, or
        /// <see langword="null"/>
        /// if no
        /// additional information is relevant or available.
        /// </summary>
        [Nullable]
        public readonly SimulatorState state;

        /// <summary>The input token stream which is being parsed.</summary>
        [NotNull]
        public readonly ITokenStream input;

        /// <summary>
        /// The token index in the input stream at which the current prediction was
        /// originally invoked.
        /// </summary>
        public readonly int startIndex;

        /// <summary>The token index in the input stream at which the current event occurred.</summary>
        public readonly int stopIndex;

        /// <summary>
        /// <see langword="true"/>
        /// if the current event occurred during LL prediction;
        /// otherwise,
        /// <see langword="false"/>
        /// if the input occurred during SLL prediction.
        /// </summary>
        public readonly bool fullCtx;

        public DecisionEventInfo(int decision, SimulatorState state, ITokenStream input, int startIndex, int stopIndex, bool fullCtx)
        {
            this.decision = decision;
            this.fullCtx = fullCtx;
            this.stopIndex = stopIndex;
            this.input = input;
            this.startIndex = startIndex;
            this.state = state;
        }
    }
}
