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

namespace Antlr4.Tool
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Antlr4.StringTemplate;
    using Console = System.Console;
    using ErrorBuffer = Antlr4.StringTemplate.Misc.ErrorBuffer;
    using Exception = System.Exception;
    using File = System.IO.File;
    using Path = System.IO.Path;
    using StringSplitOptions = System.StringSplitOptions;

    public class ErrorManager
    {
        public static readonly string FORMATS_DIR = Path.Combine("Tool", "Templates", "Messages", "Formats");

        public AntlrTool tool;
        public int errors;
        public int warnings;

        /** All errors that have been generated */
        public ISet<ErrorType> errorTypes = new HashSet<ErrorType>();

        /** The group of templates that represent the current message format. */
        TemplateGroup format;

        /** Messages should be sensitive to the locale. */
        CultureInfo locale;
        string formatName;

        ErrorBuffer initSTListener = new ErrorBuffer();

        public ErrorManager(AntlrTool tool)
        {
            this.tool = tool;
        }

        public virtual void ResetErrorState()
        {
            errors = 0;
            warnings = 0;
        }

        public virtual Template GetMessageTemplate(ANTLRMessage msg)
        {
            Template messageST = msg.GetMessageTemplate(tool.longMessages);
            Template locationST = GetLocationFormat();
            Template reportST = GetReportFormat(msg.GetErrorType().severity);
            Template messageFormatST = GetMessageFormat();

            bool locationValid = false;
            if (msg.line != -1)
            {
                locationST.Add("line", msg.line);
                locationValid = true;
            }
            if (msg.charPosition != -1)
            {
                locationST.Add("column", msg.charPosition);
                locationValid = true;
            }
            if (msg.fileName != null)
            {
                string f = msg.fileName;
                // Don't show path to file in messages; too long.
                string displayFileName = msg.fileName;
                if (File.Exists(f))
                {
                    displayFileName = Path.GetFileName(f);
                }
                locationST.Add("file", displayFileName);
                locationValid = true;
            }

            messageFormatST.Add("id", msg.GetErrorType().code);
            messageFormatST.Add("text", messageST);

            if (locationValid)
                reportST.Add("location", locationST);
            reportST.Add("message", messageFormatST);
            //((DebugST)reportST).inspect();
            //		reportST.impl.dump();
            return reportST;
        }

        /** Return a StringTemplate that refers to the current format used for
         * emitting messages.
         */
        public virtual Template GetLocationFormat()
        {
            return format.GetInstanceOf("location");
        }

        public virtual Template GetReportFormat(ErrorSeverity severity)
        {
            Template st = format.GetInstanceOf("report");
            st.Add("type", severity.GetText());
            return st;
        }

        public virtual Template GetMessageFormat()
        {
            return format.GetInstanceOf("message");
        }
        public virtual bool FormatWantsSingleLineMessage()
        {
            return format.GetInstanceOf("wantsSingleLineMessage").Render().Equals("true");
        }

        public virtual void Info(string msg)
        {
            tool.Info(msg);
        }

        public virtual void SyntaxError(ErrorType etype,
                                       string fileName,
                                       Antlr.Runtime.IToken token,
                                       Antlr.Runtime.RecognitionException antlrException,
                                       params object[] args)
        {
            ANTLRMessage msg = new GrammarSyntaxMessage(etype, fileName, token, antlrException, args);
            Emit(etype, msg);
        }

        public static void FatalInternalError(string error, Exception e)
        {
            InternalError(error, e);
            throw new Exception(error, e);
        }

        public static void InternalError(string error, Exception e)
        {
            string location = GetLastNonErrorManagerCodeLocation(e);
            InternalError("Exception " + e + "@" + location + ": " + error);
        }

        public static void InternalError(string error)
        {
            string location =
                GetLastNonErrorManagerCodeLocation(new Exception());
            string msg = location + ": " + error;
            Console.Error.WriteLine("internal error: " + msg);
        }

        /**
         * Raise a predefined message with some number of paramters for the StringTemplate but for which there
         * is no location information possible.
         * @param errorType The Message Descriptor
         * @param args The arguments to pass to the StringTemplate
         */
        public virtual void ToolError(ErrorType errorType, params object[] args)
        {
            ToolError(errorType, null, args);
        }

        public virtual void ToolError(ErrorType errorType, Exception e, params object[] args)
        {
            ToolMessage msg = new ToolMessage(errorType, e, args);
            Emit(errorType, msg);
        }

        public virtual void GrammarError(ErrorType etype,
                                 string fileName,
                                 Antlr.Runtime.IToken token,
                                 params object[] args)
        {
            ANTLRMessage msg = new GrammarSemanticsMessage(etype, fileName, token, args);
            Emit(etype, msg);

        }

        public virtual void LeftRecursionCycles(string fileName, IEnumerable<IEnumerable<Rule>> cycles)
        {
            errors++;
            ANTLRMessage msg = new LeftRecursionCyclesMessage(fileName, cycles);
            tool.Error(msg);
        }

        public virtual int GetNumErrors()
        {
            return errors;
        }

        /** Return first non ErrorManager code location for generating messages */
        private static string GetLastNonErrorManagerCodeLocation(Exception e)
        {
            string[] stack = e.StackTrace.Split(new[] { '\n', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            for (; i < stack.Length; i++)
            {
                string t = stack[i];
                if (!t.Contains(nameof(ErrorManager)))
                    return t;
            }

            return stack.LastOrDefault() ?? "<Uknown>";
        }

        // S U P P O R T  C O D E

        public virtual void Emit(ErrorType etype, ANTLRMessage msg)
        {
            var severity = etype.severity;
            if (severity == ErrorSeverity.WARNING_ONE_OFF || severity == ErrorSeverity.WARNING)
            {
                if (severity == ErrorSeverity.WARNING || !errorTypes.Contains(etype))
                {
                    warnings++;
                    tool.Warning(msg);
                }
            }
            else if (severity == ErrorSeverity.ERROR_ONE_OFF || severity == ErrorSeverity.ERROR)
            {
                if (severity == ErrorSeverity.ERROR || !errorTypes.Contains(etype))
                {
                    errors++;
                    tool.Error(msg);
                }
            }

            errorTypes.Add(etype);
        }

        /** The format gets reset either from the Tool if the user supplied a command line option to that effect
         *  Otherwise we just use the default "antlr".
         */
        public virtual void SetFormat(string formatName)
        {
            this.formatName = formatName;
            string fileName = Path.Combine(FORMATS_DIR, formatName + TemplateGroup.GroupFileExtension);
            if (!File.Exists(fileName) && formatName != "antlr")
            {
                SetFormat("antlr");
                return;
            }

            //format.EnableCache = AntlrTool.EnableTemplateCache;
            if (!File.Exists(fileName))
            {
                RawError("ANTLR installation corrupted; cannot find ANTLR messages format file " + fileName);
                Panic();
            }
            //else if (url == null)
            //{
            //    RawError("no such message format file " + fileName + " retrying with default ANTLR format");
            //    SetFormat("antlr"); // recurse on this rule, trying the default message format
            //    return;
            //}

            format = new TemplateGroupFile(Path.GetFullPath(fileName), Encoding.UTF8);
            format.Load();

            if (initSTListener.Errors.Count > 0)
            {
                RawError("ANTLR installation corrupted; can't load messages format file:\n" +
                         initSTListener.ToString());
                Panic();
            }

            bool formatOK = VerifyFormat();
            if (!formatOK && formatName.Equals("antlr"))
            {
                RawError("ANTLR installation corrupted; ANTLR messages format file " + formatName + ".stg incomplete");
                Panic();
            }
            else if (!formatOK)
            {
                SetFormat("antlr"); // recurse on this rule, trying the default message format
            }
        }

        /** Verify the message format template group */
        protected virtual bool VerifyFormat()
        {
            bool ok = true;
            if (!format.IsDefined("location"))
            {
                Console.Error.WriteLine("Format template 'location' not found in " + formatName);
                ok = false;
            }
            if (!format.IsDefined("message"))
            {
                Console.Error.WriteLine("Format template 'message' not found in " + formatName);
                ok = false;
            }
            if (!format.IsDefined("report"))
            {
                Console.Error.WriteLine("Format template 'report' not found in " + formatName);
                ok = false;
            }
            return ok;
        }

        /** If there are errors during ErrorManager init, we have no choice
         *  but to go to System.err.
         */
        internal static void RawError(string msg)
        {
            Console.Error.WriteLine(msg);
        }

        internal static void RawError(string msg, Exception e)
        {
            RawError(msg);
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine(e.StackTrace);
        }

        public virtual void Panic(ErrorType errorType, params object[] args)
        {
            ToolMessage msg = new ToolMessage(errorType, args);
            Template msgST = GetMessageTemplate(msg);
            string outputMsg = msgST.Render();
            if (FormatWantsSingleLineMessage())
            {
                outputMsg = outputMsg.Replace('\n', ' ');
            }
            Panic(outputMsg);
        }

        public static void Panic(string msg)
        {
            RawError(msg);
            Panic();
        }

        public static void Panic()
        {
            // can't call tool.panic since there may be multiple tools; just
            // one error manager
            throw new Exception("ANTLR ErrorManager panic");
        }
    }
}
