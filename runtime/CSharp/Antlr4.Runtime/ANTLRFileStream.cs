// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>
    /// This is an
    /// <see cref="AntlrInputStream"/>
    /// that is loaded from a file all at once
    /// when you construct the object.
    /// </summary>
    public class AntlrFileStream : AntlrInputStream
    {
        protected internal string fileName;

        /// <exception cref="System.IO.IOException"/>
        public AntlrFileStream([NotNull] string fileName)
            : this(fileName, null)
        {
        }

        /// <exception cref="System.IO.IOException"/>
        public AntlrFileStream([NotNull] string fileName, string encoding)
        {
            this.fileName = fileName;
            Load(fileName, encoding);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void Load([NotNull] string fileName, [Nullable] string encoding)
        {
            data = Utils.ReadFile(fileName, encoding);
            this.n = data.Length;
        }

        public override string SourceName
        {
            get
            {
                return fileName;
            }
        }
    }
}
