// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Codegen.Model
{
    using System;

    /** Indicate field of OutputModelObject is an element to be walked by
     *  OutputModelWalker.
     */
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class ModelElementAttribute : Attribute
    {
    }
}
