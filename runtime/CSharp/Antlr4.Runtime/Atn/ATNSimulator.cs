// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public abstract class ATNSimulator
    {
        [System.ObsoleteAttribute(@"Use ATNDeserializer.SerializedVersion instead.")]
        public static readonly int SerializedVersion;

        static ATNSimulator()
        {
            SerializedVersion = ATNDeserializer.SerializedVersion;
        }

        /// <summary>This is the current serialized UUID.</summary>
        [System.ObsoleteAttribute(@"Use ATNDeserializer.CheckCondition(bool) instead.")]
        public static readonly Guid SerializedUuid;

        static ATNSimulator()
        {
            SerializedUuid = ATNDeserializer.SerializedUuid;
        }

        public const char RuleVariantDelimiter = '$';

        public const string RuleLfVariantMarker = "$lf$";

        public const string RuleNolfVariantMarker = "$nolf$";

        /// <summary>Must distinguish between missing edge and edge we know leads nowhere</summary>
        [NotNull]
        public static readonly DFAState Error;

        [NotNull]
        public readonly ATN atn;

        static ATNSimulator()
        {
            Error = new DFAState(new EmptyEdgeMap<DFAState>(0, -1), new EmptyEdgeMap<DFAState>(0, -1), new ATNConfigSet());
            Error.stateNumber = int.MaxValue;
        }

        public ATNSimulator(ATN atn)
        {
            this.atn = atn;
        }

        public abstract void Reset();

        /// <summary>Clear the DFA cache used by the current instance.</summary>
        /// <remarks>
        /// Clear the DFA cache used by the current instance. Since the DFA cache may
        /// be shared by multiple ATN simulators, this method may affect the
        /// performance (but not accuracy) of other parsers which are being used
        /// concurrently.
        /// </remarks>
        /// <exception cref="System.NotSupportedException">
        /// if the current instance does not
        /// support clearing the DFA.
        /// </exception>
        /// <since>4.3</since>
        public virtual void ClearDFA()
        {
            atn.ClearDFA();
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.Deserialize(char[]) instead.")]
        public static ATN Deserialize(char[] data)
        {
            return new ATNDeserializer().Deserialize(data);
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.CheckCondition(bool) instead.")]
        public static void CheckCondition(bool condition)
        {
            new ATNDeserializer().CheckCondition(condition);
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.CheckCondition(bool, string) instead.")]
        public static void CheckCondition(bool condition, string message)
        {
            new ATNDeserializer().CheckCondition(condition, message);
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.ToInt(char) instead.")]
        public static int ToInt(char c)
        {
            return ATNDeserializer.ToInt(c);
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.ToInt32(char[], int) instead.")]
        public static int ToInt32(char[] data, int offset)
        {
            return ATNDeserializer.ToInt32(data, offset);
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.ToLong(char[], int) instead.")]
        public static long ToLong(char[] data, int offset)
        {
            return ATNDeserializer.ToLong(data, offset);
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.ToUUID(char[], int) instead.")]
        public static Guid ToUUID(char[] data, int offset)
        {
            return ATNDeserializer.ToUUID(data, offset);
        }

        [NotNull]
        [System.ObsoleteAttribute(@"Use ATNDeserializer.EdgeFactory(ATN, TransitionType, int, int, int, int, int, System.Collections.Generic.IList{E}) instead.")]
        public static Transition EdgeFactory(ATN atn, TransitionType type, int src, int trg, int arg1, int arg2, int arg3, IList<IntervalSet> sets)
        {
            return new ATNDeserializer().EdgeFactory(atn, type, src, trg, arg1, arg2, arg3, sets);
        }

        [System.ObsoleteAttribute(@"Use ATNDeserializer.StateFactory(StateType, int) instead.")]
        public static ATNState StateFactory(StateType type, int ruleIndex)
        {
            return new ATNDeserializer().StateFactory(type, ruleIndex);
        }
        /*
        public static void dump(DFA dfa, Grammar g) {
        DOTGenerator dot = new DOTGenerator(g);
        String output = dot.getDOT(dfa, false);
        System.out.println(output);
        }
        
        public static void dump(DFA dfa) {
        dump(dfa, null);
        }
        */
    }
}
