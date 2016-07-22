// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Antlr4.Runtime")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tunnel Vision Laboratories, LLC")]
[assembly: AssemblyProduct("Antlr4.Runtime")]
[assembly: AssemblyCopyright("Copyright © Sam Harwell 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)]

#if !PORTABLE || NET45PLUS
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

#if !PORTABLE
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("bc228eb9-e79c-4e5a-a1b9-0434ea566bab")]
#endif
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("4.5.0.0")]
#if !COMPACT
[assembly: AssemblyFileVersion("4.5.3.1")]
[assembly: AssemblyInformationalVersion("4.5.3.1-alpha001")]
#endif
