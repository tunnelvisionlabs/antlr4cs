// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Analysis
{
    using Antlr4.Tool.Ast;

    public class LeftRecursiveRuleAltInfo
    {
        public int altNum; // original alt index (from 1)
        public string leftRecursiveRuleRefLabel;
        public string altLabel;
        public readonly bool isListLabel;
        public string altText;
        public AltAST altAST; // transformed ALT
        public AltAST originalAltAST;
        public int nextPrec;

        public LeftRecursiveRuleAltInfo(int altNum, string altText)
            : this(altNum, altText, null, null, false, null)
        {
        }

        public LeftRecursiveRuleAltInfo(int altNum, string altText,
                                        string leftRecursiveRuleRefLabel,
                                        string altLabel,
                                        bool isListLabel,
                                        AltAST originalAltAST)
        {
            this.altNum = altNum;
            this.altText = altText;
            this.leftRecursiveRuleRefLabel = leftRecursiveRuleRefLabel;
            this.altLabel = altLabel;
            this.isListLabel = isListLabel;
            this.originalAltAST = originalAltAST;
        }
    }
}
