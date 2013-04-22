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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using NmpBase;
using Nmp;

#pragma warning disable 618

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {

		/////////////////////////////////////////////////////////////////////////////

		class MakeResponse {

			IMacroProcessor mp;

			/////////////////////////////////////////////////////////////////////////////

			private void Update( string rspPath, string dllPath )
			{
				// ******
				string resetDir = Directory.GetCurrentDirectory();

				// ******
				try {
					Directory.SetCurrentDirectory( Path.GetDirectoryName(rspPath) );

					// ******
					DateTime whenStarted = File.GetLastWriteTimeUtc( dllPath );
					
					// ******
					string cmdLine = string.Format( "@\"{0}\"", rspPath );

					External ext = new External();
					ExecResult execResult = ext.Execute( "csc.exe", cmdLine, mp.GrandCentral.MaxExecTime );
					
					if( execResult.StdOut.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 ) {
						ThreadContext.MacroError( "#makersp: error compiling: {0}", execResult.StdOut );
					}

					// ******
					DateTime current = File.GetLastWriteTimeUtc(dllPath);
					if( whenStarted == current ) {
						//
						// did not update, file times the same
						//
						ThreadContext.MacroError( "#makersp: the output file \"{0}\" was not updated\nHINT: make sure csc.exe is in your path\n", dllPath );
					}

					// ******
					ThreadContext.WriteMessage( "successfully compiled \"{0}\" via response file \"{1}\"", dllPath, rspPath );
				}

				finally {
					Directory.SetCurrentDirectory( resetDir );
				}
			}


			/////////////////////////////////////////////////////////////////////////////

			private bool UptoDate( string dllPath, string rspFilePath, string responseDirectory, IList<string> filesToCheck )
			{
				// ******
				if( ! File.Exists(dllPath) ) {
					return false;
				}
				
				// ******
				DateTime lastWriteDll = File.GetLastWriteTimeUtc( dllPath );

				// ******
				DateTime lastWriteRsp = File.GetLastWriteTimeUtc( rspFilePath );
				if( lastWriteRsp.Subtract( lastWriteDll ).TotalMilliseconds > 0 ) {
					//
					// response is newer than dll
					//
					return false;
				}

				// ******
				foreach( string _path in filesToCheck ) {
					string path = _path;

					if( path.Length > 0 ) {
						if( '"' == path[0] ) {
							path = path.Length > 2 ? path = path.Substring(1, path.Length - 2) : string.Empty;
						}

						// ******
						if( ! Path.IsPathRooted(path) ) {
							path = Path.Combine( responseDirectory, path );
						}
					}

					if( ! File.Exists(path) ) {
						ThreadContext.MacroError( "#makersp could not locate the source file \"{0}\"", path );
					}

					// ******
					DateTime lastWriteTime = File.GetLastWriteTimeUtc( path );
					TimeSpan ts = lastWriteTime.Subtract( lastWriteDll );
					
					if( ts.TotalMilliseconds > 0 ) {
						//
						// source is newer than dll
						//
						return false;
					}
				}

				// ******
				return true;
			}


			/////////////////////////////////////////////////////////////////////////////

			public string Make( string rspPath )
			{
				// ******
				try {
					if( string.IsNullOrEmpty(rspPath) ) {
						ThreadContext.MacroError( "the response file path argument to #makersp is empty" );
					}

					// ******
					if( ! Path.IsPathRooted(rspPath) ) {
						rspPath = Path.Combine( mp.GrandCentral.GetDirectoryStack().Peek(), rspPath );
					}

					string responseFileDirectory = Path.GetDirectoryName( rspPath );

					if( ! File.Exists(rspPath) ) {
						ThreadContext.MacroError( "#makersp could not locate the response file \"{0}\"", rspPath );
					}

					// ******
					string dllPath = null;
					var filesToCheck = new NmpStringList();

					// ******
					string [] linesOfText = File.ReadAllLines( rspPath );

					foreach( string _text in linesOfText ) {
						var text = _text.Trim().ToLower();

						// ******
						if( string.IsNullOrEmpty(text) ) {
							continue;
						}

						// ******
						if( text.StartsWith("/out:") ) {
							dllPath = text.Substring( 5 ).Trim();
							continue;
						}

						// ******
						char chFirst = text[0];

						if( '/' == chFirst || '#' == chFirst ) {
							continue;
						}

						// ******
						filesToCheck.Add( text );
					}

					// ******
					if( null == dllPath ) {
						ThreadContext.MacroError( "#makersp was unable to locate the \"/out:\" argument in the response file" );
					}

					if( string.IsNullOrEmpty(dllPath) ) {
						ThreadContext.MacroError( "#makersp: the \"/out:\" argument in the response file is empty" );
					}

					if( ! Path.IsPathRooted(dllPath) ) {
						dllPath = Path.Combine( responseFileDirectory, dllPath );
					}

					// ******
					if( 0 == linesOfText.Count() ) {
						ThreadContext.MacroError( "#makersp was unable to locate any source file paths in the response file" );
					}

					// ******
					if( ! UptoDate(dllPath, rspPath, responseFileDirectory, filesToCheck) ) {
						Update( rspPath, dllPath );
					}

					// ******
					return dllPath;
				}
				catch ( Exception ex ) {
					if( ex is ExitException ) {
						throw ex;
					}
					//
					// never returns
					//
					ThreadContext.MacroError( "exception in macro #makersp: {0}", ex.Message );
					//
					// make the compiler happy
					//
					throw ex;
				}
			}

			/////////////////////////////////////////////////////////////////////////////

			//
			// "make" using a response (.rsp) file and optionally register the output
			// file for macros
			//

			public MakeResponse( IMacroProcessor mp )
			{
				this.mp = mp;
			}

		}

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Does a simple make for a C# project that is defined by a response file
		/// </summary>
		/// <param name="rspPath">Path to a response file for the C# command line compiler</param>
		/// <param name="loadMacros">Load macros as defined by the IMacroContainer interface</param>
		/// <param name="displayFoundMacros">If true, output the names of the macros found</param>
		/// <returns></returns>

		//[Macro]
		//public object makersp( string rspPath, bool loadMacros, bool displayFoundMacros )
		//{
		//	// ******
		//	var mr = new MakeResponse( mp );
		//	string dllPath = mr.Make( rspPath );

		//	// ******
		//	if( loadMacros ) {
		//		AutoRegisterMacros.RegisterMacroContainers( mp, dllPath, displayFoundMacros );
		//	}

		//	// ******
		//	return string.Empty;
		//}

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Does a simple make for a C# project that is defined in a response file
		/// </summary>
		/// <param name="rspPath">Path to the response file</param>
		/// <param name="loadLibrary">If true the library built by the make will be loaded</param>

		[Macro]
		public void makersp( string rspPath, bool loadLibrary )
		{
			// ******
			var mr = new MakeResponse( mp );
			string dllPath = mr.Make( rspPath );

			// ******
			if( loadLibrary ) {
				ObjectMacros.LoadAssembly( dllPath );
			}
		}


	
	}
}
