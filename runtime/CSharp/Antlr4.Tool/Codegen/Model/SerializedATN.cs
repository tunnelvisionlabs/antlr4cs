// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Antlr4.Runtime.Atn;
    using Math = System.Math;

    public class SerializedATN : OutputModelObject
    {
        // TODO: make this into a kind of decl or multiple?
        public IList<string> serialized;

        public SerializedATN(OutputModelFactory factory, ATN atn, IList<string> ruleNames)
            : base(factory)
        {
            List<int> data = ATNSerializer.GetSerialized(atn, ruleNames);
            serialized = new List<string>(data.Count);
            foreach (int c in data)
            {
                string encoded = factory.GetTarget().EncodeIntAsCharEscape(c == -1 ? char.MaxValue : c);
                serialized.Add(encoded);
            }
            //System.Console.WriteLine(ATNSerializer.GetDecoded(factory.GetGrammar(), atn));
        }

        public virtual string[][] GetSegments()
        {
            IList<string[]> segments = new List<string[]>();
            int segmentLimit = factory.GetTarget().GetSerializedATNSegmentLimit();
            for (int i = 0; i < serialized.Count; i += segmentLimit)
            {
                IList<string> currentSegment = new System.ArraySegment<string>(serialized.ToArray(), i, Math.Min(i + segmentLimit, serialized.Count) - i);
                segments.Add(currentSegment.ToArray());
            }

            return segments.ToArray();
        }
    }
}
