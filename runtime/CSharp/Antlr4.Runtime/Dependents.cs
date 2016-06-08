// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <author>Sam Harwell</author>
    [Flags]
    public enum Dependents
    {
        None = 0,
        Self = 1 << 0,
        Parents = 1 << 1,
        Children = 1 << 2,
        Ancestors = 1 << 3,
        Descendants = 1 << 4,
        Siblings = 1 << 5,
        PreceedingSiblings = 1 << 6,
        FollowingSiblings = 1 << 7,
        Preceeding = 1 << 8,
        Following = 1 << 9
    }
}
