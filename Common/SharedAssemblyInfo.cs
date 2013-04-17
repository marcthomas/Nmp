#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


[assembly: AssemblyCompany( "netmacros.net" )]
[assembly: AssemblyProduct( "Net Macro Processor(NMP) 3.0.1" )]
[assembly: AssemblyCopyright( "Copyright © Joe McLain 2013" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]


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

[assembly: AssemblyVersion( "3.0.1" )]

[assembly: AssemblyProductVersion( "3.0.1" )]


/////////////////////////////////////////////////////////////////////////////

namespace Global {

	internal static class AssemblyInfo {
		public static string Title {
			get {
				var attr = GetAssemblyAttribute<AssemblyTitleAttribute>();
				if (attr != null)
					return attr.Title;
				return string.Empty;
			}
		}

		public static string Company {
			get {
				var attr = GetAssemblyAttribute<AssemblyCompanyAttribute>();
				if (attr != null)
					return attr.Company;
				return string.Empty;
			}
		}

		public static string Copyright {
			get {
				var attr = GetAssemblyAttribute<AssemblyCopyrightAttribute>();
				if (attr != null)
					return attr.Copyright;
				return string.Empty;
			}
		}

		public static string AssemblyVersion
		{
			get
			{
				var attr = GetAssemblyAttribute<AssemblyVersionAttribute>();
				if( attr != null )
					return attr.Version;
				return string.Empty;
			}
		}

		public static string ProductVersion
		{
			get
			{
				var attr = GetAssemblyAttribute<AssemblyProductVersionAttribute>();
				if( attr != null )
					return attr.Version;
				return string.Empty;
			}
		}

		private static T GetAssemblyAttribute<T>() where T : Attribute
		{
			object[] attributes = Assembly.GetExecutingAssembly()
				.GetCustomAttributes(typeof(T), true);
			if (attributes == null || attributes.Length == 0) return null;
			return (T)attributes[0];
		}
	}
}