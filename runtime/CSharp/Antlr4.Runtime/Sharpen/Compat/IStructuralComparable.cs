#if !NET40PLUS

// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Collections
{
    internal interface IStructuralComparable {
        Int32 CompareTo(Object other, IComparer comparer);
    }
}

#endif
