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
