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
using System.Reflection;

using Microsoft.JScript;
using Microsoft.JScript.Vsa;

using MarkdownSharp;

using NmpBase;
using Nmp;
using Global;


#pragma warning disable 618

namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {

		RegExRecognizer regExRecognizer;


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// There was a bozo the clown
		/// </summary>
		/// <returns></returns>

		[Macro]
		public object bozo()
		{
			return "Yes Virginia, Bozo was a clown!";
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Activate or deactivate the regular expression recognizer
		/// </summary>
		/// <param name="activate">True turns on, false off</param>
		/// <param name="flags">"regexOnly" if you want to use just the regex recognizer, "checkWhiteSpace" to
		/// include white space in regular expression checking</param>
		/// <returns></returns>

		[Macro]
		public object setRegexRecognizer( bool activate, params string [] flags )
		{
			// ******
			bool regexOnly = HasFlag( "regexOnly", flags );
			bool checkWhiteSpace = HasFlag( "checkWhiteSpace", flags );

			// ******
			RegExRecognizer recognizer = gc.Recognizer as RegExRecognizer;

			if( ! activate ) {
				if( null != recognizer ) {
					//
					// if not null then the current recognizer is a RegExRecognizer (which
					// means we replaced the default one) so we need to replace it with the]
					// a default one
					//
					//gc.SetRecognizer( new DefaultRecognizer( gc ) );
					gc.Recognizer = new DefaultRecognizer( gc );
				}
			}
			else {
				if( null == recognizer ) {
					//
					// caller wants regex recognition turned on and
					// its not already on
					//
					if( null == regExRecognizer ) {
						regExRecognizer = new RegExRecognizer(gc);
					}

					gc.Recognizer = regExRecognizer;
					regExRecognizer.CheckBase = ! regexOnly;
					regExRecognizer.CheckWhiteSpace = checkWhiteSpace;
				}
			}

			// ******
			return string.Empty;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Add a regular expression to the regular expression recognizer
		/// 
		/// To remove regular expression call again with 'regExName' the same and
		/// 'regExStr' empty
		/// </summary>
		/// <param name="regExName">Name for the regular expression</param>
		/// <param name="regExStr">Regular expression</param>
		/// <param name="macroToCall">Macro to call when regular expression matches</param>
		/// <returns></returns>

		[Macro]
		public object addRecognizerRegex( string regExName, string regExStr, string macroToCall )
		{
			// ******
			if( null == regExRecognizer ) {
				regExRecognizer = new RegExRecognizer(gc);
			}

			// ******
			regExRecognizer.AddRegEx( regExName, regExStr, macroToCall );

			// ******
			return string.Empty;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Exits command line host
		/// </summary>
		/// <param name="exitCode"></param>
		/// <returns></returns>

		[Macro]
		public object exit( int exitCode )
		{
			throw new ExitException( exitCode, "the Nmp exit macro has been called with the value: {0}", exitCode );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets an Nmp array where the keys are the names of macros
		/// and their value is an IMacro instance
		/// </summary>
		/// <returns></returns>

		[Macro]
		public object getMacros()
		{
			// ******
			//
			// caller needs to initiliaze a macro with a call to us
			//
			NmpArray array = new NmpArray();

			// ******
			foreach( IMacro macro in mp.GetMacros(false) ) {
				array.Add( macro.Name, macro );
			}

			// ******
			return array;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets an Nmp array of macro names
		/// Where the key "unsorted" an unsorted list of all macro names
		/// Where the key "sorted" is a sorted list of all macro names
		/// Where the key "builtin" is a sorted list of all Nmp supplied macros
		/// Where the key "object" is a sorted list of all Object macros ?? including all Nmp supplied macros
		/// And the key "text" is a sorted list of all text macros
		/// </summary>
		/// <returns></returns>

		[Macro]
		public object getMacroNames()
		{
			// ******
			var array = new NmpArray();

			// ******
			var list = mp.GetMacros( false );

			var names = new NmpStringList();
			foreach( IMacro macro in list ) {
				names.Add( macro.Name );
			}

			array.Add( "unsorted", names );
			names.Sort();
			array.Add( "sorted", names );
			
			
			list.Sort( ( a, b ) => string.Compare( a.Name, b.Name ) );


			// ******

//		Builtin,
//		Object,
//		Text,

			names = new NmpStringList();
			foreach( IMacro macro in list ) {
				if( MacroType.Builtin == macro.MacroType ) {
					names.Add( macro.Name );
				}
			}

			array.Add( "builtin", names );

			names = new NmpStringList();
			foreach( IMacro macro in list ) {
				if( MacroType.Object == macro.MacroType ) {
					names.Add( macro.Name );
				}
			}

			array.Add( "object", names );

			names = new NmpStringList();
			foreach( IMacro macro in list ) {
				if( MacroType.Text == macro.MacroType ) {
					names.Add( macro.Name );
				}
			}

			array.Add( "text", names );

			// ******
			return array;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Writes a trace message to the error stream
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		[Macro]
		public object trace( params object [] args )
		{
			// ******
			if( 0 == args.Length ) {
				ThreadContext.WriteMessage( "trace was called without arguments" );
			}
			else if( 1 == args.Length ) {
				ThreadContext.WriteMessage( "trace: {0}", args[0].ToString() );
			}
			else {
				ThreadContext.WriteMessage( "trace was called with {0} arguments:", args.Length );
				foreach( object o in args ) {
					ThreadContext.WriteMessage( ": {0}", o.ToString() );
				}
			}
			
			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// see node_markdown()
		/// </summary>
		/// <param name="str">Required, the first string</param>
		/// <param name="args">Any number of optional strings</param>
		/// <returns></returns>

		//[Macro]
		//public string markdown( string str, params string [] args )
		//{
		//	// ******
		//	try {
		//		var markdown = new Markdown { };
		//		var sb = new StringBuilder();

		//		if( !string.IsNullOrEmpty( str ) ) {
		//			sb.Append( markdown.Transform( str ) );
		//		}

		//		foreach( var arg in args ) {
		//			if( !string.IsNullOrEmpty( arg ) ) {
		//				sb.Append( markdown.Transform( arg ) );
		//			}
		//		}

		//		// ******
		//		return sb.ToString();
		//	}
		//	catch ( Exception ex ){
		//		ThreadContext.MacroError( "while processing markdown text: {0}", ex.Message );
		//	}

		//	return string.Empty;
		//}

		[Macro]
		public string markdown( string str, params string [] args )
		{
			return node_markdown( str, args );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Transforms the supplied text with the 'marked' markdown converter 
		/// package for node
		/// 
		/// node.exe must be in your path and 'marked' must have been installed
		/// 
		/// Note: markdown can also be invoked with (#block `markdown' ...)
		/// </summary>
		/// <param name="str">Required, the first string</param>
		/// <param name="args">Any number of optional strings</param>
		/// <returns></returns>

		[Macro]
		public string node_markdown( string str, params string [] args )
		{
			// ******
			if( string.IsNullOrEmpty( External.FindExe( "node.exe" ) ) ) {
				ThreadContext.MacroError( "could not locate node.exe in the path" );
			}

			// ******
			var sb = new StringBuilder();

			if( !string.IsNullOrEmpty( str ) ) {
				sb.Append( str );
			}

			foreach( var arg in args ) {
				if( !string.IsNullOrEmpty( arg ) ) {
					sb.Append( arg );
				}
			}


			// ******
			string format = @"
var fs = require( 'fs');
var marked = require( 'marked');
console.log( marked.parse(fs.readFileSync('{0}',{{encoding:'utf8'}})));
";

			var mdTempFileName = Path.GetTempFileName();
			File.WriteAllText( mdTempFileName, sb.ToString() );
			var jsSource = string.Format( format, mdTempFileName.Replace( "\\", "\\\\" ) );

			var jsTempFileName = Path.GetTempFileName();
			File.WriteAllText( jsTempFileName, jsSource );

			// ******
			var waitTime = 15 * 1000;
			ExecResult result = External.RunNode( null, '"' + jsTempFileName + '"', waitTime );

			if( 0 != result.ExitCode ) {
				ThreadContext.MacroError( "executing node.exe failed:\n" + result.StdOut );
			}

			// ******
			File.Delete( jsTempFileName );
			File.Delete( mdTempFileName );

			// ******
			return result.StdOut;
		}



	
	}


}
