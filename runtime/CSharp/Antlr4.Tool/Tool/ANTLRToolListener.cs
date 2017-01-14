// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    /** Defines behavior of object able to handle error messages from ANTLR including
     *  both tool errors like "can't write file" and grammar ambiguity warnings.
     *  To avoid having to change tools that use ANTLR (like GUIs), I am
     *  wrapping error data in Message objects and passing them to the listener.
     *  In this way, users of this interface are less sensitive to changes in
     *  the info I need for error messages.
     */
    public interface ANTLRToolListener
    {
        void Info(string msg);
        void Error(ANTLRMessage msg);
        void Warning(ANTLRMessage msg);
    }
}
