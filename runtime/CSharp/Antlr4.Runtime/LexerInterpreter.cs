// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    public class LexerInterpreter : Lexer
    {
        protected internal readonly string grammarFileName;

        protected internal readonly ATN atn;

        [Obsolete]
        protected internal readonly string[] tokenNames;

        protected internal readonly string[] ruleNames;

        protected internal readonly string[] modeNames;

        [NotNull]
        private readonly IVocabulary vocabulary;

        [Obsolete]
        public LexerInterpreter(string grammarFileName, ICollection<string> tokenNames, ICollection<string> ruleNames, ICollection<string> modeNames, ATN atn, ICharStream input)
            : this(grammarFileName, Antlr4.Runtime.Vocabulary.FromTokenNames(Sharpen.Collections.ToArray(tokenNames, new string[tokenNames.Count])), ruleNames, modeNames, atn, input)
        {
        }

        public LexerInterpreter(string grammarFileName, IVocabulary vocabulary, ICollection<string> ruleNames, ICollection<string> modeNames, ATN atn, ICharStream input)
            : base(input)
        {
            if (atn.grammarType != ATNType.Lexer)
            {
                throw new ArgumentException("The ATN must be a lexer ATN.");
            }
            this.grammarFileName = grammarFileName;
            this.atn = atn;
            this.tokenNames = new string[atn.maxTokenType];
            for (int i = 0; i < tokenNames.Length; i++)
            {
                tokenNames[i] = vocabulary.GetDisplayName(i);
            }
            this.ruleNames = Sharpen.Collections.ToArray(ruleNames, new string[ruleNames.Count]);
            this.modeNames = Sharpen.Collections.ToArray(modeNames, new string[modeNames.Count]);
            this.vocabulary = vocabulary;
            this._interp = new LexerATNSimulator(this, atn);
        }

        public override ATN Atn
        {
            get
            {
                return atn;
            }
        }

        public override string GrammarFileName
        {
            get
            {
                return grammarFileName;
            }
        }

        public override string[] TokenNames
        {
            get
            {
                return tokenNames;
            }
        }

        public override string[] RuleNames
        {
            get
            {
                return ruleNames;
            }
        }

        public override string[] ModeNames
        {
            get
            {
                return modeNames;
            }
        }

        public override IVocabulary Vocabulary
        {
            get
            {
                if (vocabulary != null)
                {
                    return vocabulary;
                }
                return base.Vocabulary;
            }
        }
    }
}
