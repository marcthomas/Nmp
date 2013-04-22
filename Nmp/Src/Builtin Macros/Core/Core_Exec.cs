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
		/// <summary>
		/// Executes a program and waits for it to complete or five seconds whichever is shorter
		/// 
		/// Returns an object with the following properties:
		/// 
		/// public string StdOut { get; }
		/// public string StdErr { get; }
		/// public int ExitCode { get; }
		/// public bool Success { get; }
		/// 
		/// </summary>
		/// <param name="exeName">Name of the executable</param>
		/// <param name="cmdLine">Command line</param>
		/// <returns>returns object as described in the summary</returns>
		/// 

		[Macro]
		public object exec( string exeName, string cmdLine )
		{
			return HandleExec( exeName, cmdLine );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Executes the current shell
		/// 
		/// Returns an object with the following properties:
		/// 
		/// public string StdOut { get; }
		/// public string StdErr { get; }
		/// public int ExitCode { get; }
		/// public bool Success { get; }
		/// 
		/// </summary>
		/// <param name="cmdLine">Command line</param>
		/// <returns></returns>

		[Macro]
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
