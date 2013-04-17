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
using System.Text;
using NmpBase;

namespace Nmp {
	/////////////////////////////////////////////////////////////////////////////

	class RazorRunnerException : Exception {

		public enum RunHow { Run, RunOnly, RunBefore, RunAfter };

		// ******		
		public RunHow					RunContext;
		public string					StackDump = string.Empty;
		public ExecutionInfo	ExecutionInfo;

		public string					RazorError;
		public string					Code;

		// ******		
		public string Error { get { return base.Message; } }


		/////////////////////////////////////////////////////////////////////////////

		public RazorRunnerException( RunHow runHow, string error, string razorError, string code )
			: base( error )
		{
			// ******
			RunContext = runHow;
			RazorError = razorError;
			Code = code;

			// ******
			var stackDump = new StringBuilder();
			this.ExecutionInfo = ThreadContext.GetExecutionInfo( stackDump, error );
			StackDump = stackDump.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string AddLineNumbers( string code )
		{
			// ******
			code = code.Replace( "\r", "" );
			string [] lines = code.Split( new string [] { "\n" }, StringSplitOptions.None );

			// ******
			int lineNo = 1;
			var sb = new StringBuilder();

			foreach( var line in lines ) {
				sb.AppendFormat( "{0,-4} ", lineNo++ );
				sb.AppendLine( line );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string HandleError( RazorRunnerException ex )
		{
			// ******
			string preMsg = string.Empty;

			switch( ex.RunContext ) {
				case RunHow.Run:
					preMsg = "While running a razor instance:";
					break;
				case RunHow.RunOnly:
					preMsg = "While running razor on only the input text (no Nmp):";
					break;
				case RunHow.RunBefore:
					preMsg = "While running razor on the input text before invoking Nmp:";
					break;
				case RunHow.RunAfter:
					preMsg = "While running razor on the output produced by Nmp:";
					break;
			}

			/// stack dump

			// ******
			//ThreadContext.WriteMessage( preMsg );
			//ThreadContext.WriteMessage( ex.Message + " Session output is the Razor source\n" );

			ThreadContext.WriteMessage( "{0}\n{1} Session output is the Razor source", preMsg, ex.Message );

			//ThreadContext.WriteMessage( ex.RazorError );
			string [] errs = ex.RazorError.Split( new string [] { "\r\n" }, StringSplitOptions.None );
			foreach( string err in errs ) {
				string text = err.Trim();
				if( !string.IsNullOrEmpty( text ) ) {
					ThreadContext.WriteMessage( text );
				}
			}

			// ******
			return AddLineNumbers( ex.Code );
		}
	}
}
