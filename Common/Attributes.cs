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
using System.Linq;
using System.Runtime.InteropServices;

using System.Text;



	/////////////////////////////////////////////////////////////////////////////

	[ComVisibleAttribute( true )]
	[AttributeUsageAttribute( AttributeTargets.Assembly, Inherited = false )]

	public class AssemblyProductVersionAttribute : Attribute {

		public string Version = string.Empty;

		/////////////////////////////////////////////////////////////////////////////

		public AssemblyProductVersionAttribute( string version )
		{
			Version = version;
		}
	}



