﻿using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Microsoft.VisualStudio.Shell.ProvideCodeBase(CodeBase = @"$PackageFolder$\SmaliVS.dll")]
[assembly: Microsoft.VisualStudio.Shell.ProvideCodeBase(CodeBase = @"$PackageFolder$\Irony.dll")]
[assembly: Microsoft.VisualStudio.Shell.ProvideCodeBase(CodeBase = @"$PackageFolder$\SharpAdbClient.dll")]
[assembly: Microsoft.VisualStudio.Shell.ProvideCodeBase(CodeBase = @"$PackageFolder$\Mono.Posix.dll")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SmaliVS.ProjectType")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SmaliVS.ProjectType")]
[assembly: AssemblyCopyright("Copyright ©  2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("845e42d0-0d55-4f83-8919-1f47cadf70b1")]

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
