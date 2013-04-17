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

		public object bozo()
		{
			return "Yes Virginia, Bozo was a clown!";
		}


		/////////////////////////////////////////////////////////////////////////////
		
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

		public object exit( int exitCode )
		{
			throw new ExitException( exitCode, "the Nmp exit macro has been called with the value: {0}", exitCode );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

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

		public string markdown( string str, params string [] args )
		{
			// ******
			try {
				var markdown = new Markdown { };
				var sb = new StringBuilder();

				if( !string.IsNullOrEmpty( str ) ) {
					sb.Append( markdown.Transform( str ) );
				}

				foreach( var arg in args ) {
					if( !string.IsNullOrEmpty( arg ) ) {
						sb.Append( markdown.Transform( arg ) );
					}
				}

				// ******
				return sb.ToString();
			}
			catch ( Exception ex ){
				ThreadContext.MacroError( "while processing markdown text: {0}", ex.Message );
			}

			return string.Empty;
		}
	
	
	
	}


}
