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
    using Antlr4.StringTemplate;
    using CommonToken = Antlr.Runtime.CommonToken;
    using Exception = System.Exception;
    using IToken = Antlr.Runtime.IToken;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;
    using NullableAttribute = Antlr4.Runtime.Misc.NullableAttribute;
    using TokenTypes = Antlr.Runtime.TokenTypes;

    public class ANTLRMessage
    {
        private static readonly object[] EMPTY_ARGS = new object[0];

        [NotNull]
        private readonly ErrorType errorType;
        [Nullable]
        private readonly object[] args;
        [Nullable]
        private readonly Exception e;

        // used for location template
        public string fileName;
        public int line = -1;
        public int charPosition = -1;

        public Grammar g;
        /** Most of the time, we'll have a token such as an undefined rule ref
         *  and so this will be set.
         */
        public IToken offendingToken;

        public ANTLRMessage([NotNull] ErrorType errorType)
            : this(errorType, (Exception)null, new CommonToken(TokenTypes.Invalid))
        {
        }

        public ANTLRMessage([NotNull] ErrorType errorType, IToken offendingToken, params object[] args)
            : this(errorType, null, offendingToken, args)
        {
        }

        public ANTLRMessage([NotNull] ErrorType errorType, [Nullable] Exception e, IToken offendingToken, params object[] args)
        {
            this.errorType = errorType;
            this.e = e;
            this.args = args;
            this.offendingToken = offendingToken;
        }

        [return: NotNull]
        public virtual ErrorType GetErrorType()
        {
            return errorType;
        }

        [return: NotNull]
        public virtual object[] GetArgs()
        {
            if (args == null)
            {
                return EMPTY_ARGS;
            }

            return args;
        }

        public virtual Template GetMessageTemplate(bool verbose)
        {
            Template messageST = new Template(GetErrorType().msg);
            messageST.impl.Name = errorType.Name;

            messageST.Add("verbose", verbose);
            object[] args = GetArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string attr = "arg";
                if (i > 0)
                    attr += i + 1;
                messageST.Add(attr, args[i]);
            }
            if (args.Length < 2)
                messageST.Add("arg2", null); // some messages ref arg2

            Exception cause = GetCause();
            if (cause != null)
            {
                messageST.Add("exception", cause);
                messageST.Add("stackTrace", cause.StackTrace);
            }
            else
            {
                messageST.Add("exception", null); // avoid ST error msg
                messageST.Add("stackTrace", null);
            }

            return messageST;
        }

        [return: Nullable]
        public virtual Exception GetCause()
        {
            return e;
        }

        public override string ToString()
        {
            return "Message{" +
                   "errorType=" + GetErrorType() +
                   ", args=" + GetArgs() +
                   ", e=" + GetCause() +
                   ", fileName='" + fileName + '\'' +
                   ", line=" + line +
                   ", charPosition=" + charPosition +
                   '}';
        }
    }
}
