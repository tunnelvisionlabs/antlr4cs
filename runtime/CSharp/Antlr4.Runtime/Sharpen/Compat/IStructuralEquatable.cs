#if !NET40PLUS

// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Collections {

    public interface IStructuralEquatable {
        Boolean Equals(Object other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }
}

#endif
