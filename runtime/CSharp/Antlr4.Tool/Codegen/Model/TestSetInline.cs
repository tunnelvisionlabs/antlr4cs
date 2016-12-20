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
    using Antlr4.Tool.Ast;
    using IntervalSet = Antlr4.Runtime.Misc.IntervalSet;

    /** */
    public class TestSetInline : SrcOp
    {
        public int bitsetWordSize;
        public string varName;
        public Bitset[] bitsets;

        public TestSetInline(OutputModelFactory factory, GrammarAST ast, IntervalSet set, int wordSize)
            : base(factory, ast)
        {
            bitsetWordSize = wordSize;
            Bitset[] withZeroOffset = CreateBitsets(factory, set, wordSize, true);
            Bitset[] withoutZeroOffset = CreateBitsets(factory, set, wordSize, false);
            this.bitsets = withZeroOffset.Length <= withoutZeroOffset.Length ? withZeroOffset : withoutZeroOffset;
            this.varName = "_la";
        }

        private static Bitset[] CreateBitsets(OutputModelFactory factory,
                                              IntervalSet set,
                                              int wordSize,
                                              bool useZeroOffset)
        {
            IList<Bitset> bitsetList = new List<Bitset>();
            foreach (int ttype in set.ToArray())
            {
                Bitset current = bitsetList.Count > 0 ? bitsetList[bitsetList.Count - 1] : null;
                if (current == null || ttype > (current.shift + wordSize - 1))
                {
                    current = new Bitset();
                    if (useZeroOffset && ttype >= 0 && ttype < wordSize - 1)
                    {
                        current.shift = 0;
                    }
                    else
                    {
                        current.shift = ttype;
                    }

                    bitsetList.Add(current);
                }

                current.ttypes.Add(factory.GetTarget().GetTokenTypeAsTargetLabel(factory.GetGrammar(), ttype));
            }

            return bitsetList.ToArray();
        }

        public sealed class Bitset
        {
            public int shift;
            public readonly IList<string> ttypes = new List<string>();
        }
    }
}
