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
using System.Text;

using Xunit;

using NmpBase;
using Nmp;


#pragma warning disable 169

namespace NmpTests {


	/////////////////////////////////////////////////////////////////////////////

	class DieException : Exception {
		public DieException( string msg )
			: base(msg)
		{
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	public class Evaluator<T> : NmpEvaluator where T: INmpHost, new() {

		/////////////////////////////////////////////////////////////////////////////

		public Evaluator()
			: base( new T { } )
		{
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	// NmpHostHelper
	//
	/////////////////////////////////////////////////////////////////////////////

	public class NmpHostHelper : INmpHost {

		StringBuilder warningsErrorsAndText = new StringBuilder();


		// ******
		public IRecognizer					Recognizer	{ get { return null; } }
		public IMacroTracer					Tracer			{ get { return null; } }
		public IMacroProcessorBase MacroHandler	{ get { return null; } }
		public string HostName { get { return "hosthelper"; } }


		///////////////////////////////////////////////////////////////////////////////

		public bool ErrorReturns
		{
			get {
				return false;
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Error( Notifier notifier, string fmt, params object [] args )
		{
			// ******
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;
			string errStr = Helpers.SafeStringFormat( "{0} ({1},0): error: {2}", ei.FileName, ei.Line, string.Format(fmt, args) );
			
			if( null != notifier.ExceptionToThrow ) {
				throw notifier.ExceptionToThrow;
			}
			throw new DieException( errStr );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Warning( Notifier notifier, string fmt, params object [] args )
		{
			// ******
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;
			Console.WriteLine( "{0} ({1},0): warning: {2}", ei.FileName, ei.Line, string.Format(fmt, args) );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( ExecutionInfo ei, string fmt, params object [] args )
		{
			Console.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Trace( ExecutionInfo ei, string fmt, params object [] args )
		{
			System.Diagnostics.Trace.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public void Die( string fmt, params object [] args )
		{
			throw new DieException( Helpers.SafeStringFormat(fmt, args) );
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	// TestHelpers
	//
	/////////////////////////////////////////////////////////////////////////////

	public static class TestHelpers {

		// ******
		//
		// all test files must put text to be tested in a macro named "_Text",
		// other things need to happend as well - see the files for what
		//
		const string MacroName = "_Test";

		// ******
		public static string SourceTestTextPath						{ get; private set; }
		public static string SourceTestSuccessTextPath		{ get; private set; }


		///////////////////////////////////////////////////////////////////////////////

		private static string SourcePath( string fileName )
		{
			return SourceTestTextPath + fileName;
		}


		///////////////////////////////////////////////////////////////////////////////

		private static string SourceSuccessPath( string fileName )
		{
			return SourceTestSuccessTextPath + fileName + ".result";
		}


		/////////////////////////////////////////////////////////////////////////////

		//public static Tuple<string, string> GetFilesContents( string name, out string source, out string success )

		public static void GetFilesContents( string name, out string source, out string success )
		{
			source = File.ReadAllText( SourcePath(name) );
			success = File.ReadAllText( SourceSuccessPath(name) );
		}


		///////////////////////////////////////////////////////////////////////////////

		public static Tuple<string, string> RunMacro( string fileName)
		{
			// ******
			string sourceText;
			string successText;
			GetFilesContents( fileName, out sourceText, out successText );
			
			// ******
			using ( var mp = NewNmpEvaluator() ) {
				//
				//
				//
				mp.ChangeRootDirectory( SourceTestTextPath );
				
				//
				// define `isTest' true
				//
				mp.AddTextMacro( "isTest", "true", null );

				// ******
				//
				// sourceText contains the macro definition that we want to
				// invoke - this is important: we are NOT testing the result
				// of running the macro file, we are testing the results of
				// a specific macro (which we do in the following section)
				//
				// here we run the macro text so that a macro that it contains
				// is added to the macro table so we can execute it in the
				// next step
				//
				mp.Evaluate( mp.GetStringContext(sourceText), true );

				// ******
				//
				// invoke macro named 'MacroName' ("_Test")
				//
				string result = mp.InvokeMacro( MacroName, null, true );
				return new Tuple<string, string>( result, successText );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static NmpEvaluator NewNmpEvaluator()
		{
			return new NmpEvaluator( new NmpHostHelper() );
		}



		/////////////////////////////////////////////////////////////////////////////
	
		public static string InterpretFile( string filePath )
		{
			// ******
			using ( var mp = NewNmpEvaluator() ) {
				return mp.Evaluate( mp.GetFileContext(filePath), true );
			}

		}
		
		
		/////////////////////////////////////////////////////////////////////////////
	
		public static string InterpretString( string str )
		{
			// ******
			using ( var mp = NewNmpEvaluator() ) {
				return mp.Evaluate( mp.GetStringContext(str), true );
			}

		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		static TestHelpers()
		{
			SourceTestTextPath = string.Format( @"{0}\..\..\Src\FileTestSource\", Directory.GetCurrentDirectory() );
			SourceTestSuccessTextPath = SourceTestTextPath + @"Success\";
		}

	}


}


