// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* [The "BSD license"]
*  Copyright (c) 2014 Terence Parr
*  Copyright (c) 2014 Sam Harwell
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
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>
    /// This interface provides information about the vocabulary used by a
    /// recognizer.
    /// </summary>
    /// <seealso cref="Recognizer{Symbol, ATNInterpreter}.Vocabulary()"/>
    /// <author>Sam Harwell</author>
    public interface IVocabulary
    {
        /// <summary>Returns the highest token type value.</summary>
        /// <remarks>
        /// Returns the highest token type value. It can be used to iterate from
        /// zero to that number, inclusively, thus querying all stored entries.
        /// </remarks>
        /// <returns>the highest token type value</returns>
        int MaxTokenType
        {
            get;
        }

        /// <summary>Gets the string literal associated with a token type.</summary>
        /// <remarks>
        /// Gets the string literal associated with a token type. The string returned
        /// by this method, when not
        /// <see langword="null"/>
        /// , can be used unaltered in a parser
        /// grammar to represent this token type.
        /// <p>The following table shows examples of lexer rules and the literal
        /// names assigned to the corresponding token types.</p>
        /// <table>
        /// <tr>
        /// <th>Rule</th>
        /// <th>Literal Name</th>
        /// <th>Java String Literal</th>
        /// </tr>
        /// <tr>
        /// <td>
        /// <c>THIS : 'this';</c>
        /// </td>
        /// <td>
        /// <c>'this'</c>
        /// </td>
        /// <td>
        /// <c>"'this'"</c>
        /// </td>
        /// </tr>
        /// <tr>
        /// <td>
        /// <c>SQUOTE : '\'';</c>
        /// </td>
        /// <td>
        /// <c>'\''</c>
        /// </td>
        /// <td>
        /// <c>"'\\''"</c>
        /// </td>
        /// </tr>
        /// <tr>
        /// <td>
        /// <c>ID : [A-Z]+;</c>
        /// </td>
        /// <td>n/a</td>
        /// <td>
        /// <see langword="null"/>
        /// </td>
        /// </tr>
        /// </table>
        /// </remarks>
        /// <param name="tokenType">The token type.</param>
        /// <returns>
        /// The string literal associated with the specified token type, or
        /// <see langword="null"/>
        /// if no string literal is associated with the type.
        /// </returns>
        [Nullable]
        string GetLiteralName(int tokenType);

        /// <summary>Gets the symbolic name associated with a token type.</summary>
        /// <remarks>
        /// Gets the symbolic name associated with a token type. The string returned
        /// by this method, when not
        /// <see langword="null"/>
        /// , can be used unaltered in a parser
        /// grammar to represent this token type.
        /// <p>This method supports token types defined by any of the following
        /// methods:</p>
        /// <ul>
        /// <li>Tokens created by lexer rules.</li>
        /// <li>Tokens defined in a <code>tokens{}</code> block in a lexer or parser
        /// grammar.</li>
        /// <li>The implicitly defined
        /// <c>EOF</c>
        /// token, which has the token type
        /// <see cref="IToken.Eof"/>
        /// .</li>
        /// </ul>
        /// <p>The following table shows examples of lexer rules and the literal
        /// names assigned to the corresponding token types.</p>
        /// <table>
        /// <tr>
        /// <th>Rule</th>
        /// <th>Symbolic Name</th>
        /// </tr>
        /// <tr>
        /// <td>
        /// <c>THIS : 'this';</c>
        /// </td>
        /// <td>
        /// <c>THIS</c>
        /// </td>
        /// </tr>
        /// <tr>
        /// <td>
        /// <c>SQUOTE : '\'';</c>
        /// </td>
        /// <td>
        /// <c>SQUOTE</c>
        /// </td>
        /// </tr>
        /// <tr>
        /// <td>
        /// <c>ID : [A-Z]+;</c>
        /// </td>
        /// <td>
        /// <c>ID</c>
        /// </td>
        /// </tr>
        /// </table>
        /// </remarks>
        /// <param name="tokenType">The token type.</param>
        /// <returns>
        /// The symbolic name associated with the specified token type, or
        /// <see langword="null"/>
        /// if no symbolic name is associated with the type.
        /// </returns>
        [Nullable]
        string GetSymbolicName(int tokenType);

        /// <summary>Gets the display name of a token type.</summary>
        /// <remarks>
        /// Gets the display name of a token type.
        /// <p>ANTLR provides a default implementation of this method, but
        /// applications are free to override the behavior in any manner which makes
        /// sense for the application. The default implementation returns the first
        /// result from the following list which produces a non-
        /// <see langword="null"/>
        /// result.</p>
        /// <ol>
        /// <li>The result of
        /// <see cref="GetLiteralName(int)"/>
        /// </li>
        /// <li>The result of
        /// <see cref="GetSymbolicName(int)"/>
        /// </li>
        /// <li>The result of
        /// <see cref="int.ToString()"/>
        /// </li>
        /// </ol>
        /// </remarks>
        /// <param name="tokenType">The token type.</param>
        /// <returns>
        /// The display name of the token type, for use in error reporting or
        /// other user-visible messages which reference specific token types.
        /// </returns>
        [NotNull]
        string GetDisplayName(int tokenType);
    }
}
