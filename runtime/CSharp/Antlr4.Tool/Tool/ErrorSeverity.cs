// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    /**
     * Abstracts away the definition of Message severity and the text that should
     * display to represent that severity if there is no StringTemplate available
     * to do it.
     *
     * @author Jim Idle - Temporal Wave LLC (jimi@temporal-wave.com)
     */
    public sealed class ErrorSeverity
    {
        public static readonly ErrorSeverity INFO = new ErrorSeverity("info");
        public static readonly ErrorSeverity WARNING = new ErrorSeverity("warning");
        public static readonly ErrorSeverity WARNING_ONE_OFF = new ErrorSeverity("warning");

        public static readonly ErrorSeverity ERROR = new ErrorSeverity("error");
        public static readonly ErrorSeverity ERROR_ONE_OFF = new ErrorSeverity("error");
        public static readonly ErrorSeverity FATAL = new ErrorSeverity("fatal"); // TODO: add fatal for which phase? sync with ErrorManager

        /**
         * The text version of the ENUM value, used for display purposes
         */
        private readonly string text;

        /**
         * Standard getter method for the text that should be displayed in order to
         * represent the severity to humans and product modelers.
         *
         * @return The human readable string representing the severity level
         */
        public string GetText()
        {
            return text;
        }

        /**
         * Standard constructor to build an instance of the Enum entries
         *
         * @param text The human readable string representing the severity level
         */
        private ErrorSeverity(string text)
        {
            this.text = text;
        }
    }
}
