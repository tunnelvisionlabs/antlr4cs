// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Runtime.Misc
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class NotNullAttribute : Attribute
    {
    }
}
