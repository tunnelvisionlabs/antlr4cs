// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
