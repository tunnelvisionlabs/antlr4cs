// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines helper methods to work with command line and arguments.
    /// </summary>
    internal static class CommandLineHelper
    {
        /// <summary>
        /// Joins multiple command line arguments together into one string using space as a separator,
        /// applying all necessary quoting and escaping.
        /// </summary>
        /// <param name="arguments">Command line argument to join</param>
        /// <returns>String that contains all command line arguments joined together.</returns>
        public static string JoinArguments(IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            StringBuilder builder = new StringBuilder();
            foreach (string argument in arguments)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                if (argument.IndexOfAny(new[] { '"', ' ' }) < 0)
                {
                    builder.Append(argument);
                    continue;
                }

                // escape backslashes appearing before a double quote or end of line by doubling them
                string arg = Regex.Replace(argument, @"(\\+)(""|$)", "$1$1$2");
                // escape double quotes
                arg = arg.Replace("\"", "\\\"");

                // wrap the argument in outer quotes
                builder.Append('"').Append(arg).Append('"');
            }

            return builder.ToString();
        }
    }
}
