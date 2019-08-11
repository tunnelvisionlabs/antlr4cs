// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4BuildTasks.Test
{
    using Antlr4.Build.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines test cases for <see cref="CommandLineHelper"/>
    /// </summary>
    [TestClass]
    public class TestCommandLineHelper
    {
        [TestMethod]
        [Description("Verifies that there are no unnecessary double quotes added.")]
        public void JoinArgumentsDoesNotAddUnnecessaryQuotes()
        {
            Assert.AreEqual(
                @"-o C:\somedir\ grammar.g4",
                CommandLineHelper.JoinArguments(new[] { "-o", @"C:\somedir\", "grammar.g4"}));
        }

        [TestMethod]
        [Description("Verifies that arguments with spaces in them are being enclosed in double quotes.")]
        public void JoinArgumentsQuotesArgsWithSpaces()
        {
            Assert.AreEqual(
                @"-o ""C:\some dir"" grammar.g4",
                CommandLineHelper.JoinArguments(new[] { "-o", @"C:\some dir", "grammar.g4" }));
        }

        [TestMethod]
        [Description("Verifies that arguments with double quotes in them are being enclosed in double quotes and double quotes within the argument are properly escaped.")]
        public void JoinArgumentsAddsQuotesToArgsWithQuotes()
        {
            Assert.AreEqual(
                @"-o ""C:\some\""dir"" grammar.g4",
                CommandLineHelper.JoinArguments(new[] { "-o", @"C:\some""dir", "grammar.g4" }));
        }

        [TestMethod]
        [Description("Verifies that arguments with backslash followed by double quote are being enclosed in double quotes and all special characters within the argument are properly escaped.")]
        public void JoinArgumentsEscapesBackSlashBeforeDoubleQuote()
        {
            Assert.AreEqual(
                @"-o ""C:\some\\\""dir"" grammar.g4",
                CommandLineHelper.JoinArguments(new[] { "-o", @"C:\some\""dir", "grammar.g4" }));
        }

        [TestMethod]
        [Description("Verifies that arguments with multiple backslashes followed by double quote are being enclosed in double quotes and all special characters within the argument are properly escaped.")]
        public void JoinArgumentsEscapesMultipleBackSlashesBeforeDoubleQuote()
        {
            Assert.AreEqual(
                @"-o ""C:\some\\\\\""dir"" grammar.g4",
                CommandLineHelper.JoinArguments(new[] { "-o", @"C:\some\\""dir", "grammar.g4" }));
        }

        [TestMethod]
        [Description("Verifies that arguments with spaces and trailing backslash are being enclosed in double quotes and backslash within the argument is properly escaped.")]
        public void JoinArgumentsEscapesTrailingBackSlashesInQuotedArgs()
        {
            Assert.AreEqual(
                @"-o ""C:\some dir\\"" grammar.g4",
                CommandLineHelper.JoinArguments(new[] { "-o", @"C:\some dir\", "grammar.g4" }));
        }
    }
}
