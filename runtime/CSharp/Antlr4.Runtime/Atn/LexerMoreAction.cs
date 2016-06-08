// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// Implements the
    /// <c>more</c>
    /// lexer action by calling
    /// <see cref="Antlr4.Runtime.Lexer.More()"/>
    /// .
    /// <p>The
    /// <c>more</c>
    /// command does not have any parameters, so this action is
    /// implemented as a singleton instance exposed by
    /// <see cref="Instance"/>
    /// .</p>
    /// </summary>
    /// <author>Sam Harwell</author>
    /// <since>4.2</since>
    public sealed class LexerMoreAction : ILexerAction
    {
        /// <summary>Provides a singleton instance of this parameterless lexer action.</summary>
        public static readonly Antlr4.Runtime.Atn.LexerMoreAction Instance = new Antlr4.Runtime.Atn.LexerMoreAction();

        /// <summary>
        /// Constructs the singleton instance of the lexer
        /// <c>more</c>
        /// command.
        /// </summary>
        private LexerMoreAction()
        {
        }

        /// <summary><inheritDoc/></summary>
        /// <returns>
        /// This method returns
        /// <see cref="LexerActionType.More"/>
        /// .
        /// </returns>
        public LexerActionType ActionType
        {
            get
            {
                return LexerActionType.More;
            }
        }

        /// <summary><inheritDoc/></summary>
        /// <returns>
        /// This method returns
        /// <see langword="false"/>
        /// .
        /// </returns>
        public bool IsPositionDependent
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// <inheritDoc/>
        /// <p>This action is implemented by calling
        /// <see cref="Antlr4.Runtime.Lexer.More()"/>
        /// .</p>
        /// </summary>
        public void Execute(Lexer lexer)
        {
            lexer.More();
        }

        public override int GetHashCode()
        {
            int hash = MurmurHash.Initialize();
            hash = MurmurHash.Update(hash, (int)(ActionType));
            return MurmurHash.Finish(hash, 1);
        }

        public override bool Equals(object obj)
        {
            return obj == this;
        }

        public override string ToString()
        {
            return "more";
        }
    }
}
