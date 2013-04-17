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
using System.Text;
using Nmp;

//using System.ComponentModel.Composition;
//using System.ComponentModel.Composition.Hosting;

//
// error reporting - dll's
//
// http://www.red-gate.com/supportcenter/Content?p=SmartAssembly&c=SmartAssembly/help/5.5/SA_Errors_DLLs.htm&toc=SmartAssembly/help/5.5/toc1210439.htm


using NmpBase;
using NmpBasicTracer;

namespace NmpHost {


	/////////////////////////////////////////////////////////////////////////////

	public class ErrorHandledException : Exception {
	}


	/////////////////////////////////////////////////////////////////////////////

	class NmpHost : INmpHost {

		const string	NMP_CMDFILE_EXT		= ".rsp";
		const string	LINK_FILE_EXT			= ".lnk";
		const string	RESPONSE_PATH			= "responsepath";

		// ******
		static string 		Codebase;
		static TextWriter	StdErr = Console.Error;

		// ******
		bool	noPathInWarnError = false;

		IMacroTracer macroTracer = null;

		//bool snapShot = false;


		// ******
		public IRecognizer					Recognizer		{ get; set; }
		public IMacroTracer					Tracer				{ get { return macroTracer; } }
		public IMacroProcessorBase MacroHandler { get; set; }
		public string HostName { get { return "commandline"; } }


		///////////////////////////////////////////////////////////////////////////////

		public bool ErrorReturns
		{
			get {
				return true;
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Error( Notifier notifier, string fmt, params object [] args )
		{
			// ******
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;

			// ******
			string fileName = ei.FileName;
			if( noPathInWarnError ) {
				fileName = Path.GetFileName( fileName );							
			}

			Console.WriteLine( ei.FullMessage );
			Console.WriteLine();
			Console.WriteLine( "{0} ({1},0): error: {2}", fileName, ei.Line, Helpers.SafeStringFormat( fmt, args ) );

			// ******
			if( Debugger.IsAttached ) {
				Debugger.Break();
			}
			//else {
			//	Environment.Exit( 1 );
			//}

			//Die(string.Empty);
			
			//throw new ErrorHandledException();
			throw new ExitException(1);
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Warning( Notifier notifier, string fmt, params object [] args )
		{
			// ******
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;

			// ******
			string fileName = ei.FileName;
			if( noPathInWarnError ) {
				fileName = Path.GetFileName( fileName );							
			}

			// ******
			//Console.WriteLine( ei.FullMessage );
			Console.WriteLine( "{0} ({1},0): warning: {2}", fileName, ei.Line, string.Format(fmt, args) );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( ExecutionInfo ei, string fmt, params object [] args )
		{
			//if( null != ei ) {
			//	Console.WriteLine( "at Line {0}, in {1}:", ei.Line, ei.FileName );
			//}

			// ******
			Console.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Trace( ExecutionInfo ei, string fmt, params object [] args )
		{
			System.Diagnostics.Trace.WriteLine( string.Format("at Line {0}, in {1}:", ei.Line, ei.FileName) );
			System.Diagnostics.Trace.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}



		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( string fmt, params object [] args )
		{
			WriteMessage( null, fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public void WriteStdError( string fmt, params object [] args )
		{
			StdErr.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public void Die( string fmt, params object [] args )
		{
			StdErr.WriteLine( Helpers.SafeStringFormat(fmt, args) );
			Environment.Exit( 1 );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Goodby( string fmt, params object [] args )
		{
			// ******
			if( ! string.IsNullOrEmpty(fmt) ) {
				WriteMessage( fmt, args );
			}

			// ******
			Environment.Exit( 0 );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		protected void Register( bool addToEnv )
		{
			// ******
			string codeBasePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase.Substring(8) );
			
			if( addToEnv ) {
				WriteMessage( "Adding NMP directory to the users path" );
				NmpEnvironment.AddPathString( codeBasePath, EnvironmentVariableTarget.User );
			}
			else {
				WriteMessage( "Removing NMP directory from the users path" );
				NmpEnvironment.RemovePathString( codeBasePath, EnvironmentVariableTarget.User );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Tuple<string, string, bool, string> EvaluateFile( string fileName, NmpStringArray defines )
		{
			// ******
			if( null != macroTracer ) {
				macroTracer.BeginSession( Path.GetFileName( fileName ), Path.GetDirectoryName( fileName ) );
			}

			// ******
			try {
				if( ! File.Exists(fileName) ) {
					Die( "input file does not exist: {0}", fileName );
				}

				// ******
				using( var mp = new NmpEvaluator(this) ) {

					foreach( var kvp in defines ) {
						var key = kvp.Key;
						if( !string.IsNullOrEmpty( key ) && '-' == key [ 0 ] ) {
							//
							// v3 have not added undef back to evaluator
							//
							continue;
						}

						// ******
						mp.AddTextMacro( key, kvp.Value, null );
					}

					// ******
					string result = mp.Evaluate( mp.GetFileContext(fileName), true );
					return new Tuple<string,string, bool, string>(result, mp.FileExt, mp.NoOutputFile, mp.OutputEncoding);
				}
			}

	//
	// catch error and report here
	//
#if EXREPORTING
			// ref dll
#endif

			finally {
				// ******
				if( null != macroTracer ) {
					macroTracer.EndSession();
				}

			}
		}
		

		/////////////////////////////////////////////////////////////////////////////

		protected void TestCmd( string testType, CmdLineParams cmds )
		{
			// ******
			testType = testType.ToLower();
			
			// ******
			if( "input" == testType || "i" == testType ) {
				var inputTest = new InputTest( this, cmds );
				inputTest.Run();
				Goodby( string.Empty );
			}
			else {
				Die( "unkown test type: \"{0}\"", testType );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		Encoding DetermineEncoding( Encoding defEncoding, string name )
		{
			// ******
			Encoding encoding = defEncoding;

			switch( name ) {
				case "ascii":
					encoding = Encoding.ASCII;
					break;

				case "utf7":
					encoding = Encoding.UTF7;
					break;

				case "utf8":
					encoding = Encoding.UTF8;
					break;

				case "utf32":
					encoding = Encoding.UTF32;
					break;

				case "unicode":
					encoding = Encoding.Unicode;
					break;
			}

			// ******
			return encoding;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool ProcessParams( CmdLineParams cmds )
		{
		
			//// ******
			//for( int iCmd = 0; iCmd < cmds.Count; iCmd++ ) {
			//	string cmd;
			//	string value;
			//
			//	// ******
			//	if( cmds.GetCommand(iCmd, out cmd, out value) ) {
			//		string key = string.Empty;
			//		
			//		// ******
			//		if( null == value ) {
			//			value = string.Empty;
			//		}
			//		
			//		// ******
			//		//WriteLine( "command {0}, value {1}", cmd, value );
			//
			//		// ******
			//		switch( cmd ) {
			//			//case "REG":
			//			//	Register( REG_WHICH.PathOnly, value );
			//			//	break;
			//			//	return true;
			//			//
			//			//case "VSREG":
			//			//	Register( REG_WHICH.VSIntegration, value );
			//			//	return true;
			//			//
			//			//case "REGALL":
			//			//	Register( REG_WHICH.All, value );
			//			//	break;
			//			//	return true;
			//			//
			//			//case "out":
			//			//	outputUTF8 = false;
			//			//	outputUnicode = false;
			//			//	outputFile = Path.IsPathRooted(value) ? value : Path.Combine(Directory.GetCurrentDirectory(), value);
			//			//	break;
			//			//	
			//			//case "out8":
			//			//	outputUTF8 = true;
			//			//	outputUnicode = false;
			//			//	outputFile = Path.IsPathRooted(value) ? value : Path.Combine(Directory.GetCurrentDirectory(), value);
			//			//	break;
			//			//	
			//			//case "out16":
			//			//	outputUnicode = true;
			//			//	outputUTF8 = false;
			//			//	outputFile = Path.IsPathRooted(value) ? value : Path.Combine(Directory.GetCurrentDirectory(), value);
			//			//	break;
			//			//	
			//			//case "define":
			//			//	if( CmdLineParams.GetValuePair(value, out key, out value) ) {
			//			//		tool.AddMacroDefinition( key, value );
			//			//	}
			//			//	else {
			//			//		WriteStdError( "no macro name to define" );
			//			//	}
			//			//	break;
			//			//
			//			//case "undef":
			//			//	if( string.IsNullOrEmpty(value) ) {
			//			//		WriteStdError( "no macro name to undef" );
			//			//	}
			//			//	else {
			//			//		////WriteLine( "  undef: \"{0}\"", value );
			//			//		//defines.Remove( value );
			//			//		tool.RemoveMacroDefinition( value );
			//			//	}
			//			//	break;
			//			//
			//			//case "t":
			//			//	tool.SetParam( "traceon", null == value ? "0" : value );
			//			//	break;
			//			//	
			//			//case "p":
			//			//	noPathInWarnError = true;
			//			//	break;
			//			//	
			//			//	
			//			//case "run":
			//			//	ExecMethod( tool, value );
			//			//	break;
			//
			//			case "test":
			//			case "t":
			//				break;
			//				
			//			default:
			//				isCmdLineError = true;
			//				WriteStdError( "unknown command line switch \"{0}\"", cmd );
			//				break;
			//		}
			//	}
			//	else {
			//		//WriteLine( "value {1}", cmd, value );
			//		
			//		//// ******
			//		//string fileName = value.ToLower().Trim();
			//		//fileName = Path.IsPathRooted(fileName) ? fileName : Path.Combine( Directory.GetCurrentDirectory(), fileName );
			//		//
			//		//if( NMP_CMDFILE_EXT == Path.GetExtension(fileName) ) {
			//		//	//
			//		//	// treat as a response file
			//		//	//
			//		//	if( File.Exists(fileName) ) {
			//		//		CmdLineParams p = new CmdLineParams( fileName, false );
			//		//		cmds.InsertRange( 1 + iCmd, p );
			//		//		
			//		//		Directory.SetCurrentDirectory( Path.GetDirectoryName(fileName) );
			//		//	}
			//		//	else {
			//		//		WriteStdError( "unable to locate response file: {0}", fileName );
			//		//		return false;
			//		//	}
			//		//}
			//		//else if( LINK_FILE_EXT == Path.GetExtension(fileName) ) {
			//		//	//
			//		//	// ignore for now
			//		//	//
			//		//	WriteStdError( ".lnk files are not currently supported" );
			//		//	return false;
			//		//}
			//		//else if( File.Exists(fileName) ) {
			//		//	bool appendToFile = 0 == string.Compare( outputFile, prevOutputFile, true );
			//		//	
			//		//	// ******
			//		//	if( ! ProcessFile(fileName, outputFile, appendToFile) ) {
			//		//		return false;
			//		//	}
			//		//	
			//		//	// ******
			//		//	prevOutputFile = outputFile;
			//		//}
			//		//else {
			//		//	WriteStdError( "file \"{0}\" could not be found", fileName );
			//		//	return false;
			//		//}
			//	}
			//	
			//}
			
		bool		isCmdLineError = false;
		bool		consoleOut = false;
		string	outputFile = string.Empty;
		string	prevOutputFile = string.Empty;
		//string	fileExt = "txt";
		NmpStringArray defines = new NmpStringArray { };

		Encoding encoding = Encoding.UTF8;

			// ******
			while( cmds.Count > 0 ) {
				string cmd;
				string value;
				bool isCmd = cmds.GetCommand( 0, out cmd, out value );
				cmds.Remove( 0 );
			
				// ******
				if( isCmd ) {
					string key = string.Empty;
					
					// ******
					if( null == value ) {
						value = string.Empty;
					}
					
					// ******
					//WriteLine( "command {0}, value {1}", cmd, value );
			
					// ******
					switch( cmd ) {
						//case "ext":
						//	if( ! string.IsNullOrEmpty(value) ) {
						//		fileExt = value.Trim();
						//	}
						//	break;

						//case "mc":
						//	Recognizer = new DefaultRecognizer();
						//	MacroHandler = new MPMacrosCalled( Recognizer );
						//	break;

						case "console":
						case "c":
							consoleOut = true;
							break;

						case "out":
							consoleOut = false;
							if( string.IsNullOrEmpty(value) ) {
								outputFile = string.Empty;
							}
							else {
								outputFile = value.Trim();
								if( ! Path.IsPathRooted(outputFile) ) {
									outputFile = Path.GetFullPath( outputFile );
								}
							}
							break;

						case "define":
						case "d":
							if( CmdLineParams.GetValuePair( value, out key, out value ) ) {
								defines.Add( key, value );
							}
							else {
								WriteStdError( "bad define \"{0}\", usage: -d:macro_name=\"value\"", value );
							}
							break;

						case "undef":
							if( string.IsNullOrEmpty( value ) ) {
								WriteStdError( "no macro name to undef" );
							}
							else {
								////WriteLine( "  undef: \"{0}\"", value );
								//defines.Remove( value );
								//tool.RemoveMacroDefinition( value );
								defines.Add( "-" + value, string.Empty );
							}
							break;
						

						//case "test":
						//case "t":
						//	TestCmd( value, cmds );
						//	break;
						

						case "register":
						case "r":
							Register( true );
							break;

						//case "snampshot":
						//case "s":
						//	snapShot = true;
						//	break;

						case "unregister":
						case "u":
							Register( false );
							break;


						case "ascii":
							encoding = Encoding.ASCII;
							break;

						case "utf7":
							encoding = Encoding.UTF7;
							break;

						case "utf8":
							encoding = Encoding.UTF8;
							break;

						case "utf32":
							encoding = Encoding.UTF32;
							break;

						case "unicode":
							encoding = Encoding.Unicode;
							break;


						case "trace":
						case "t":
							if( null == macroTracer ) {
								macroTracer = new BasicTracer();
							}
							break;
						
							
						default:
							Die( "unknown command line switch \"{0}\"", cmd );
							break;
					}
							
				}
				else {
					
					//WriteLine( "value {1}", cmd, value );
					
					// ******
					//string fileName = value.ToLower().Trim();
					string fileName = value.Trim();
					fileName = Path.IsPathRooted(fileName) ? fileName : Path.GetFullPath( fileName );

					// ******
					if( NMP_CMDFILE_EXT == Path.GetExtension(fileName) ) {
						//
						// .rsp - treat as a response file
						//
						if( File.Exists(fileName) ) {
							CmdLineParams p = new CmdLineParams( fileName, false );
							cmds.InsertRange( 0, p );
							
							Directory.SetCurrentDirectory( Path.GetDirectoryName(fileName) );
						}
						else {
							Die( "unable to locate response file: {0}", fileName );
						}
					}

					//else if( LINK_FILE_EXT == Path.GetExtension(fileName) ) {
					//	//
					//	// ignore for now
					//	//
					//	WriteLine( ".lnk files are not currently supported" );
					//	return false;
					//}
					
					else if( File.Exists(fileName) ) {
						//
						// use outputFile if we have one
						//
						string saveFile = ! string.IsNullOrEmpty(outputFile) ? outputFile : string.Format( @"{0}\{1}", Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) );
						
						// ******
						//
						// where
						//
						//		Item1 is the resulting text output by Nmp
						//
						//		Item2 is the file ext set by by the macro script (usually empty)
						//
						//		Item3 is the "no output" flag where the macro script requested that we not output a file
						//
						//		Item4 is output Encoding (as a string value)
						//
						Tuple<string, string, bool, string> result = EvaluateFile( fileName, defines );

						// ******
						//
						// Item3 is true if there should be no output file 
						//
						if( ! result.Item3 ) {
							if( consoleOut ) {
								Console.Write( result.Item1 );
							}
							else {
								//
								// append ext - may be empty
								//
								string path = saveFile + result.Item2;
								var encodingToUse = DetermineEncoding( encoding, result.Item4 );

								if( 0 == string.Compare( path, prevOutputFile, true ) ) {
									File.AppendAllText( path, result.Item1, encodingToUse );
								}
								else {
									File.WriteAllText( path, result.Item1, encodingToUse );
								}

								// ******
								prevOutputFile = path;
							}
						}
					}
					else {
						WriteMessage( "file \"{0}\" could not be found", fileName );
						return false;
					}


					//if( consoleOut ) {
					//}

					//else {
					//	Die( "unknown non command argument: {0}", cmd );
					//}
				}
			}

			// ******
			return ! isCmdLineError;
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		private int Run( string[] args )
		{
#if TRACER_BUILD
				string debugOrRelease = " (Debug - Tracer),";
#elif DEBUG
				string debugOrRelease = " (Debug),";
#else
			string debugOrRelease = string.Empty;
#endif

			// ******
			CmdLineParams cmds = new CmdLineParams( args, false, false );
			if( 0 == cmds.Count ) {

				WriteStdError( "nmphost [options] filename filename ... " );
				WriteStdError( "" );
				WriteStdError( "        version: {0}{1} Location: {2}", Global.AssemblyInfo.ProductVersion, debugOrRelease, Codebase );
				WriteStdError( "" );
	
				//WriteStdError( "-out:filename          ouput to 'filename' using the default encoding" );
				//WriteStdError( "-out8:filename         ouput to 'filename' using UTF8 encoding" );
				//WriteStdError( "-out16:filename        ouput to 'filename' using Unicode encoding" );
				//WriteStdError( "-define:macro[=value]  define a macro with an optional value" );
				//WriteStdError( "-undef:macro           undefine a macro" );
				//WriteStdError( "-REG:1|0               register the command line tool" );
				//WriteStdError( "-VSREG:1|0             register the Visual Studio extensions" );
				//WriteStdError( "-REGALL:1|0            register all" );
				//WriteStdError( "-t:[level]             turn trace on, level is optional" );
				//WriteStdError( "-p                     do not include path in error file name" );
				//
				//WriteStdError( "-run:\"macroName, path to library, method name, args\"" );
				//
				//// -v  version, exit after print

				WriteStdError( "  -c           output to console" );
				WriteStdError( "  -o:filename  output to 'filename', any specified extension is appended" );
				WriteStdError( "  -d:macro[=value]  define a macro with an optional value" );
				WriteStdError( "  -r           register NmpHost.exe in the users environment path in HKEY_CURRENT_USER" );
				WriteStdError( "  -u           unregister NmpHost.exe in the users environment path in HKEY_CURRENT_USER" );
				WriteStdError( "  -t           a basic trace file is generated (pretty useless in current build)" );
				WriteStdError( "  -ascii       encode output file in 7 bit ASCII)" );
				WriteStdError( "  -utf7        encode output file in UTF7)" );
				WriteStdError( "  -utf8        encode output file in UTF8)" );
				WriteStdError( "  -utf32       encode output file in UTF32)" );
				WriteStdError( "  -unicode     encode output file in Unicode)" );

const string comment = @"

	By default the output file is the same name as the input file with the file extension stripped
	off. You can use the #setOutputExtension(.ext) macro to set an extension to be appended to the
	output file name - you must include the dot in the extension.
";

				WriteStdError( comment );

				return 1;
			}
			else {

				WriteMessage( "nmphost version: {0}{1} Location: {2}", Global.AssemblyInfo.ProductVersion, debugOrRelease, Codebase );
				

			}
	
			WriteStdError( "" );
	
			// ******
			if( ! ProcessParams(cmds) ) {
				//
				// errors
				//
				return 1;
			}
			
			// ******
			return 0;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		
		public string GetLocation()
		{
			// ******
			if( string.IsNullOrEmpty(Codebase) ) {
				Assembly asm = Assembly.GetExecutingAssembly();
				if( null != asm ) {
					Codebase = Path.GetDirectoryName(asm.CodeBase.Substring(8));
				}
			}
			
			// ******
			return Codebase;
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public NmpHost()
		{
			GetLocation();
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

static void Test()
{
	throw new Exception( "hi" );
}

		static int Main( string [] args )
		{
			///var cd = Environment.CurrentDirectory;
			///
			// ConfigurationManager class

			// ******
			try {
				NmpHost program = new NmpHost();
				return program.Run( args );
			}
			catch( ExitException ) {
				return 1;
			}
			catch( ErrorHandledException ) {
				//Environment.Exit( 1 );
				return 1;
			}

		}

	}


}
