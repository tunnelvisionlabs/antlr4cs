// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public LexerInterpreter(string grammarFileName, IEnumerable<string> tokenNames, IEnumerable<string> ruleNames, IEnumerable<string> modeNames, ATN atn, ICharStream input)
            : this(grammarFileName, Antlr4.Runtime.Vocabulary.FromTokenNames(tokenNames.ToArray()), ruleNames, modeNames, atn, input)
        {
        }

        public LexerInterpreter(string grammarFileName, IVocabulary vocabulary, IEnumerable<string> ruleNames, IEnumerable<string> modeNames, ATN atn, ICharStream input)
            : base(input)
        {
            if (atn.grammarType != ATNType.Lexer)
            {
                throw new ArgumentException("The ATN must be a lexer ATN.");
            }
            this.grammarFileName = grammarFileName;
            this.atn = atn;
#pragma warning disable 612 // 'fieldName' is obsolete
            this.tokenNames = new string[atn.maxTokenType];
            for (int i = 0; i < tokenNames.Length; i++)
            {
                tokenNames[i] = vocabulary.GetDisplayName(i);
            }
#pragma warning restore 612
            this.ruleNames = ruleNames.ToArray();
            this.modeNames = modeNames.ToArray();
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

        [Obsolete("Use IRecognizer.Vocabulary instead.")]
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
