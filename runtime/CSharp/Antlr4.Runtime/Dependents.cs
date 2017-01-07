// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <author>Sam Harwell</author>
    public enum Dependents
    {
        Self,
        Parents,
        Children,
        Ancestors,
        Descendants,
        Siblings,
        PreceedingSiblings,
        FollowingSiblings,
        Preceeding,
        Following
    }
}
