// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e79060d9-d211-4367-925a-5541943d3857")]

// Expose internal members to tests assembly
[assembly: InternalsVisibleTo("Antlr4BuildTasks.Test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a91484c425c1d6692e335e2406a91dbbbb27fd18b1add6beda60c4ebebe4264a7caa41f4d407d66fb8ae6b3544b169a46180e134a4bb441b690369f47bac90ecfeae6ed0e6513e2bdc69459e6bf89ddfc6425477413eea5396779757d95ec19d61c25b330fa464d192184f3ac2b32b1005962345e42aff4794628b4b733e4db4")]