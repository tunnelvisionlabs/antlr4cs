// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    /** the various kinds of labels. t=type, id=ID, types+=type ids+=ID */
    public enum LabelType
    {
        RULE_LABEL,
        TOKEN_LABEL,
        RULE_LIST_LABEL,
        TOKEN_LIST_LABEL,
        LEXER_STRING_LABEL         // used in lexer for x='a'
    }
}
