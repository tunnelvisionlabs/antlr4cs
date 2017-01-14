// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using Antlr4.StringTemplate;
    using Console = System.Console;

    /** */
    public class DefaultToolListener : ANTLRToolListener
    {
        public AntlrTool tool;

        public DefaultToolListener(AntlrTool tool)
        {
            this.tool = tool;
        }

        public virtual void Info(string msg)
        {
            if (tool.errMgr.FormatWantsSingleLineMessage())
            {
                msg = msg.Replace('\n', ' ');
            }

            Console.WriteLine(msg);
        }

        public virtual void Error(ANTLRMessage msg)
        {
            Template msgST = tool.errMgr.GetMessageTemplate(msg);
            string outputMsg = msgST.Render();
            if (tool.errMgr.FormatWantsSingleLineMessage())
            {
                outputMsg = outputMsg.Replace('\n', ' ');
            }

            Console.Error.WriteLine(outputMsg);
        }

        public virtual void Warning(ANTLRMessage msg)
        {
            Template msgST = tool.errMgr.GetMessageTemplate(msg);
            string outputMsg = msgST.Render();
            if (tool.errMgr.FormatWantsSingleLineMessage())
            {
                outputMsg = outputMsg.Replace('\n', ' ');
            }

            Console.Error.WriteLine(outputMsg);
        }
    }
}
