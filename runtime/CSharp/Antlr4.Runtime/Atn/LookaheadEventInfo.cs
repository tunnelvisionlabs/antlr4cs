// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// This class represents profiling event information for tracking the lookahead
    /// depth required in order to make a prediction.
    /// </summary>
    /// <since>4.3</since>
    public class LookaheadEventInfo : DecisionEventInfo
    {
        private readonly int predictedAlt;

        /// <summary>
        /// Constructs a new instance of the
        /// <see cref="LookaheadEventInfo"/>
        /// class with
        /// the specified detailed lookahead information.
        /// </summary>
        /// <param name="decision">The decision number</param>
        /// <param name="state">
        /// The final simulator state containing the necessary
        /// information to determine the result of a prediction, or
        /// <see langword="null"/>
        /// if
        /// the final state is not available
        /// </param>
        /// <param name="predictedAlt">The alternative chosen by
        /// <see cref="ParserATNSimulator.AdaptivePredict(ITokenStream, int, ParserRuleContext)"/>.</param>
        /// <param name="input">The input token stream</param>
        /// <param name="startIndex">The start index for the current prediction</param>
        /// <param name="stopIndex">The index at which the prediction was finally made</param>
        /// <param name="fullCtx">
        /// 
        /// <see langword="true"/>
        /// if the current lookahead is part of an LL
        /// prediction; otherwise,
        /// <see langword="false"/>
        /// if the current lookahead is part of
        /// an SLL prediction
        /// </param>
        public LookaheadEventInfo(int decision, SimulatorState state, int predictedAlt, ITokenStream input, int startIndex, int stopIndex, bool fullCtx)
            : base(decision, state, input, startIndex, stopIndex, fullCtx)
        {
            this.predictedAlt = predictedAlt;
        }

        /// <summary>
        /// The alternative chosen by adaptivePredict(), not necessarily
        /// the outermost alt shown for a rule; left-recursive rules have
        /// user-level alts that differ from the rewritten rule with a (...) block
        /// and a (..)* loop.
        /// </summary>
        public int PredictedAlternative
        {
            get
            {
                return predictedAlt;
            }
        }
    }
}
