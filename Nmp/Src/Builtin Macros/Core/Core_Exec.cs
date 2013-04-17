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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NmpBase;
using Nmp;

#pragma warning disable 618

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {

		/////////////////////////////////////////////////////////////////////////////

		protected object HandleExec( string exeName, string cmdLine )
		{
			External ext = new External();
			return ext.Execute( exeName, cmdLine, gc.MaxExecTime );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

			public object exec( string exeName, string cmdLine )
		{
			return HandleExec( exeName, cmdLine );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object shell( string cmdLine )
		{
			// ******
			string shell = Environment.GetEnvironmentVariable( "ComSpec" );
			if( string.IsNullOrEmpty(shell) ) {
				shell = "cmd.exe";
			}

			// ******
			return HandleExec( shell, cmdLine );
		}


	}
}
