// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public sealed class EmptyPredictionContext : PredictionContext
    {
        public static readonly Antlr4.Runtime.Atn.EmptyPredictionContext LocalContext = new Antlr4.Runtime.Atn.EmptyPredictionContext(false);

        public static readonly Antlr4.Runtime.Atn.EmptyPredictionContext FullContext = new Antlr4.Runtime.Atn.EmptyPredictionContext(true);

        private readonly bool fullContext;

        private EmptyPredictionContext(bool fullContext)
            : base(CalculateEmptyHashCode())
        {
            this.fullContext = fullContext;
        }

        public bool IsFullContext
        {
            get
            {
                return fullContext;
            }
        }

        protected internal override PredictionContext AddEmptyContext()
        {
            return this;
        }

        protected internal override PredictionContext RemoveEmptyContext()
        {
            throw new NotSupportedException("Cannot remove the empty context from itself.");
        }

        public override PredictionContext GetParent(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override int GetReturnState(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override int FindReturnState(int returnState)
        {
            return -1;
        }

        public override int Size
        {
            get
            {
                return 0;
            }
        }

        public override PredictionContext AppendContext(int returnContext, PredictionContextCache contextCache)
        {
            return contextCache.GetChild(this, returnContext);
        }

        public override PredictionContext AppendContext(PredictionContext suffix, PredictionContextCache contextCache)
        {
            return suffix;
        }

        public override bool IsEmpty
        {
            get
            {
                return true;
            }
        }

        public override bool HasEmpty
        {
            get
            {
                return true;
            }
        }

        public override bool Equals(object o)
        {
            return this == o;
        }

        public override string[] ToStrings<_T0>(Recognizer<_T0> recognizer, int currentState)
        {
            return new string[] { "[]" };
        }

        public override string[] ToStrings<_T0>(Recognizer<_T0> recognizer, PredictionContext stop, int currentState)
        {
            return new string[] { "[]" };
        }
    }
}
