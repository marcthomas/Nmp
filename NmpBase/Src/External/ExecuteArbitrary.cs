#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// ExecuteArbitrary.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

//using System.Xml;
//using System.Xml.Schema;
using Microsoft.Win32;

//using MP.Base;


namespace NmpBase {


	public enum ExecError { ExecOk, UnableToExecute = -1000 };//, GenericErr = -5000 }
	
	
	/////////////////////////////////////////////////////////////////////////////

	public class ExecResult {
		public string StdOut = string.Empty;
		public string StdErr = string.Empty;
		public int ExitCode = 1;
		public bool Success = false;
	}


	/////////////////////////////////////////////////////////////////////////////
	
	public class External {

		protected	Process	p = null;
		protected	ProcessStartInfo si = null;

		protected	string	stdInput = string.Empty;
		protected	string	error = string.Empty;	
		
		protected	string	path = string.Empty;

		protected	StringBuilder output = new StringBuilder();
		protected	StringBuilder errOutput = new StringBuilder();
		protected	int	exitCode  = 0;
		
		protected	string	lastCmdLine = string.Empty;

		/////////////////////////////////////////////////////////////////////////////
		
		public string	StdInput		{ set { stdInput = value; } get { return stdInput; } }
		public string	Path				{ set { path = value; } get { return path; } }
		
		public int		ExitCode		{ get { return exitCode; } }
		public string	Error				{ get { return error; } }
		public string	StdOut			{ get { return output.ToString(); } }
		public string	StdErr			{ get { return errOutput.ToString(); } }
		public string	LastCmdLine	{ get { return lastCmdLine; } }
		
		/////////////////////////////////////////////////////////////////////////////
			
		private void Clear()
		{
			p = null;
			si = null;
			
			error = string.Empty;
			
			output.Length = 0;
			errOutput.Length = 0;
			exitCode = 0;

			lastCmdLine = string.Empty;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
			
		private void Update( StringBuilder sb, DataReceivedEventArgs outLine )
		{
			if( null != outLine.Data ) {
				if( outLine.Data.Length > 0 ) {
					sb.Append( outLine.Data + Environment.NewLine );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
			
		private void UpdateError( StringBuilder output, DataReceivedEventArgs outLine )
		{
			Update( output, outLine );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExecResult Execute( string exe, string cmdLine, int waitTime )
		{
			// ******
			Clear();

			// ******
			ExecResult result = new ExecResult() { Success = false };
			StringBuilder sb = new StringBuilder();
		
			// ******
			Process	p = new Process();
			ProcessStartInfo si = p.StartInfo;
			
			try {
				si.UseShellExecute = false;
				si.CreateNoWindow = true;

				// ******
				si.FileName = exe;

				// ******
				si.Arguments = string.IsNullOrEmpty(cmdLine) ? string.Empty : cmdLine;
				lastCmdLine = si.Arguments;

				// ******
				if( ! String.IsNullOrEmpty(path) ) {
					//
					// prepend path
					//
					string curPath = si.EnvironmentVariables[ "path" ];
					sb.Length = 0;
					sb.AppendFormat( "{0};{1}", path, curPath );
					si.EnvironmentVariables[ "path" ] = sb.ToString();
				}

				// ******
				si.RedirectStandardOutput = true;
				p.OutputDataReceived += delegate (object sendingProcess, DataReceivedEventArgs outLine) { Update(output, outLine); };

				// ******
				si.RedirectStandardError = true;
				p.ErrorDataReceived += delegate (object sendingProcess, DataReceivedEventArgs outLine) { UpdateError(output, outLine); };

				// ******
				//si.RedirectStandardInput = true;
			}
			catch {
				error = string.Format( "Error initializing \"{0}\" process", exe );
				return result;
			}
			
			// ******
			try {
				p.Start();
			}
			catch ( Exception e ) {
				error = string.Format( "Exception attempting to execute \"{0}\": {1}", si.FileName, e.Message );
				return result;
			}

			// ******
			try {
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();

				// ******
				if( ! string.IsNullOrEmpty(stdInput) ) { 
					p.StandardInput.Write( stdInput );
					p.StandardInput.Close();
				}
					
				// ******
				//
				// allow time for the program to run 
				//

				if( p.WaitForExit(waitTime) ) {
					p.WaitForExit();
					exitCode = p.ExitCode;
					result.Success = true;
				}
				else {
					p.Kill();

					// p.WaitForExit()	after kill
					// p.HasExited after kill


					error = string.Format( "Timeout executing \"{0}\": {1} milliseconds", si.FileName, waitTime );

					exitCode = -5;
					result.Success = false;
				}

				// ******
				p.Close();
			}
			catch {
				error = string.Format( "error executing \"{0}\"", si.FileName);
				return result;
			}
			finally {
			}
			
			// ******
			result.StdOut = output.ToString();
			result.StdErr = errOutput.ToString();

			result.ExitCode = exitCode;

			return result;
		}
		
		
	}
	
}