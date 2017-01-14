// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Runtime
{
    using System;
    using Antlr4.Runtime.Atn;

    public interface IRecognizer
    {
        [Obsolete("Use IRecognizer.Vocabulary instead.")]
        string[] TokenNames
        {
            get;
        }

        IVocabulary Vocabulary
        {
            get;
        }

        string[] RuleNames
        {
            get;
        }

        string GrammarFileName
        {
            get;
        }

        ATN Atn
        {
            get;
        }

        int State
        {
            get;
        }

        IIntStream InputStream
        {
            get;
        }
    }
}
