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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
//using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.JScript;
using Microsoft.JScript.Vsa;


using NmpBase;


#pragma warning disable 414
#pragma warning disable 618


namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////

	partial class TextMacroRunner : IDisposable {

		protected const string	NO_SUBST									= "nosubst";
		protected const string	MACRO_DATA_ONLY						= "data";
		protected const string	STRINGOPS_REPLACE_RESULT	= "replace";

		protected const string	INVOKED_AS		= "InvokedAs";

		//enum MDIndexes : int {
		//	//
		//	// information for post processing
		//	//
		//	ARGSREFED,							// arguments referenced so we've created a list
		//	PRINTBLOCKREFED,				// print blocks detected -> .{ }.
		//	EMBEDEDCHARS,						// special characters have been embeded
		//	
		//	COUNT_DIRECTIVES
		//};

		// ******
		IMacroProcessor	mp;
		IMacro					macro;
		IMacroOptions		options;
		object []				objectArgs;
		NmpStringList 	arguments;
		string 					expressionNodeTypeName;
		NmpArray				localArray;

		// ******
		IMacro	argsMacro;
		IMacro	specials;
		IMacro	localArrayMacro;
		IMacro	objArgsMacro;

		bool argsRefed = false;				// arguments referenced so we've created a list
		bool printBlockRefed = false;	// print blocks detected -> .{ }.
		bool embededChars= false;			// special characters have been embeded
		bool reExpand = false;				// expand (run regex) macro twice

		bool dataOptionFound = false;	// if seen we halt substitution

		//string	diversionName = string.Empty;


		///////////////////////////////////////////////////////////////////////////////
		//
		////private VsaEngine	vsa;
		//
		//public string EvalResult( string result )
		//{
		//	// ******
		//	if( string.IsNullOrEmpty(result) ) {
		//		return string.Empty;
		//	}
		//
		//	// ******
		//	//if( null == vsa ) {
		//		VsaEngine vsa = VsaEngine.CreateEngine();
		//	//}
		//
		//	// ******
		//	if( null == vsa ) {
		//		ThreadContext.MacroWarning( "unable to initialize VsaEngine instance, can not evaluate macro result" );
		//		return result;
		//	}
		//
		//	// ******
		//	try {
		//		//object objResult = Eval.JScriptEvaluate( "(" + result + ")", "unsafe", vsa );
		//
		//		object objResult = Eval.JScriptEvaluate( result, "unsafe", vsa );
		//		return null == objResult ? SC.MACRO_NULL : objResult.ToString();
		//	}
		//	catch ( Exception ex ) {
		//		//
		//		// never returns
		//		//
		//		ThreadContext.WriteMessage( "Evaluate macro result string: {0}", result );
		//		ThreadContext.MacroError( Helpers.RecursiveMessage(ex, "error executing JScriptEvaluate on macro result") );
		//		return null;
		//	}
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		protected string ArgSubstMatch( Match match )
		{
			int index;

			// ******
			string value = match.Value;
			string untouchedValue = value;

			// ******
			if( dataOptionFound ) {
				//
				// once we see .data. we stop processing matches
				//
				return untouchedValue;
			}

			// ******
			//
			// '\\' catches -> \$ 

			// backslash escapes the pattern so that it can be used by .NET regex replacements;
			// this problem happens when a regex is used in one of the parameters string to a
			// macro, it gets seen first by us and we replace it - so escape it
			//

			// TODO: refigure this out, \$ also catches -> somedir\$replacementPathPart


			//if( ('\\' == value[0] || '$' == value[0]) && '$' == value[1] ) {

			if( '$' == value [ 0 ] && '$' == value [ 1 ] ) {
				//
				// \$something or $$something
				//
				return value.Substring( 1 );
			}

			if( '.' == value [ 0 ] && '-' == value [ 1 ] ) {
				//
				// ..something
				//
				return "." + value.Substring( 2 );
			}

			// ******
			//jpm 1 March 13, lower case dot command string

			switch( match.Value.ToLower() ) {
				//
				// number of arguments
				//
				case "$#":
					return arguments.Count.ToString();

				case "$*":
					//
					// all arguments, no quoting
					//
					return arguments.Join( "," );

				case "$@":
					//
					// all arguments, quoted
					//
					return arguments.Join( ",", s => mp.GrandCentral.QuoteWrapString( s ) );

				case "$macroName":
					//
					// macro name
					//
					return macro.Name;

				case "$arguments":
					//
					// argument list is being referenced
					//
					argsRefed = true;
					return argsMacro.Name;

				case "$objArgs":
					//
					// actual object list
					//
					return objArgsMacro.Name;

				case "$local":
					//
					// $local vars
					//
					return localArrayMacro.Name;

				case "$specials":
					//
					// $specials list
					//
					return specials.Name;

				case ".{":
					//
					// argument list is being referenced
					//
					printBlockRefed = true;
					return SC.DEFAULT_BEGINPRINTBLOCK_STR;

				case "}.":
					//
					// argument list is being referenced
					//
					if( !printBlockRefed ) {
						//
						// only allow a close print block if we've seen an open block
						//
						return value;	///string.Empty;
					}
					return SC.DEFAULT_ENDPRINTBLOCK_STR;

				case ".spc.":
					//
					// embed a space
					//
					embededChars = true;
					return SC.EMBED_SPACE_STR;

				case ".nl.":
					//
					// embed a newline
					//
					embededChars = true;
					return SC.EMBED_NEWLINE_STR;

				case ".tab.":
					//
					// embed a tab
					//
					embededChars = true;
					return SC.EMBED_TAB_STR;

				case ".data.":
					//
					// no forward expand, no pushback
					//

					//TODO: look at this

					dataOptionFound = true;
					//options.FwdExpand = false;
					options.Pushback = false;
					break;

				case ".format.":
					options.Format = true;
					break;

				case ".noexp.":
					//
					// do not do a controlled expand of this macro
					//
					options.FwdExpand = false;
					break;

				case ".expand.":
					//
					// force a controled expand of this macro
					// 
					options.FwdExpand = true;
					break;

				case ".nopb.":
					//
					// do not expand the result of this macro - no pushback
					//
					options.Pushback = false;
					break;

				case ".pushback.":
					//
					// force the expansion of the result of this macro - pushback
					//
					options.Pushback = true;
					break;

				case ".noquote.":
					//case ".noquoteresult.":
					options.Quote = false;
					break;

				case ".quote.":
					//case ".quoteresult.":
					options.Quote = true;
					break;

				case ".trim.":
					//
					// strip leading and trailing white space
					//
					options.Trim = true;
					break;

				case ".nlstrip.":
					//
					// strip newline sequences from text
					//
					options.NLStrip = true;
					break;

				case ".ilcompressws.":
				case ".ilcws.":
					//
					// any run of white-space is converted to a single space
					//
					options.ILCompressWhiteSpace = true;
					break;

				case ".normalize.":
				case ".wscompress.":
				case ".cws.":
					//
					// any run of white-space is converted to a single space
					//
					options.CompressAllWhiteSpace = true;
					break;

				case ".tbwrap.":
					options.TextBlockWrap = true;
					break;

				case ".reexpand.":
					reExpand = true;
					break;

				case ".divert.":
					//diversionName = macro.Name;
					options.Divert = true;
					break;

				case ".eval.":
					options.Eval = true;
					break;

				case ".razor.":
					options.Razor = true;
					break;

				case ".razorobject.":
					options.RazorObject = true;
					break;

				case ".empty.":
					options.Empty = true;
					break;

				case ".tabstospaces.":
					options.TabsToSpaces = true;
					break;

				case ".htmlencode.":
					options.HtmlEncode = true;
					break;

				case ".echo.":
					options.Echo = true;
					break;

				default:
					//
					// check for unknown dot directive, a '-' following the first dot
					// indicates a dot directive that's been turned off which means
					// that text with leading ".-" and a trailing '.' will always
					// be stripped
					//
					if( '.' == value [ 0 ] && '.' == value [ value.Length - 1 ] ) {
						return '-' == value [ 1 ] ? string.Empty :
						value;
					}

					// ******			
					if( value.StartsWith( "$[]" ) ) {
						var objArgStr = RefObjectArgument( value.Substring( 3 ) );
						return null == objArgStr ? untouchedValue : objArgStr;
					}
					if( '$' == value [ 0 ] ) {
						value = value.Substring( 1 );

						if( char.IsDigit( value [ 0 ] ) ) {
							if( Int32.TryParse( value, out index ) && index >= 0 && index <= 9 ) {
								if( index < arguments.Count ) {
									return arguments [ index ];
								}
							}
						}
						else {
							//
							// possible named argument
							//
							if( null != macro.ArgumentNames ) {
								index = macro.ArgumentNames.IndexOf( value );
								if( index >= 0 && index < arguments.Count ) {
									return arguments [ index ];
								}
								else {
									//
									// named arg but was not passed in
									//
									// jpm: 13 Dec 2012
									//
									if( macro.ArgumentNames.Count > 1 ) {
										//
										// too few arguments - not just one being empty, a comma would have to be left out
										//
										ThreadContext.MacroWarning( "there were too few arguments passed to macro \"{0}\", the parameter named \"{1}\" could not be set", macro.Name, value );
										//return '$' + value;
										return string.Empty;
									}
									else {
										//
										// the single argument expected was not passed OR it was empty, the parser would have had no way to
										// know ??? really ???
										//
										//ThreadContext.MacroWarning( "the macro \"{0}\" expected a single argument was empty", macro.Name );
										return string.Empty;
									}
								}
							}

							// ******
							//
							// don't know what it is, return it
							//
							return untouchedValue;
						}
					}

					return untouchedValue;	//string.Empty;
				//break;
			}



			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		string RefObjectArgument( string value )
		{
			// ******
			var index = macro.ArgumentNames.IndexOf( value );
			if( index < 0 ) {
				if( 1 == value.Length ) {
					index = "0123456789".IndexOf( value [ 0 ] );
					if( index < 0 ) {
						return null;
					}
				}
				else {
					ThreadContext.MacroError( "the macro \"{0}\" does not have an argument named \"$[]{1}\"", macro.Name, value );
				}
			}

			// ******
			//
			// __macroArgs122Objects[ 4 ]
			//
			// objArgsMacro is a macro whose object is the list of input object which
			// we index to give access
			//
			//    $[]fred  in  (#defmacro `macro', `tina', `fred' ...)
			//
			//       gives us __macroArgs122Objects[ 1 ]  which will be invoked as an indexer
			//
			// if `fred' was the name of a macro we could also access it like this
			//
			//   $fred
			//
			//    which is translated as:  arguments[ 1 ]
			//
			//      where arguments[] is a string list of all the input objects where ToString() was called on each
			//        arguments[1]'s text (the macro name) might be "isDead" so
			//
			//           $fred  ->  isDead      which will invoke that macro

			if( index >= arguments.Count ) {
				//
				// macro caller did not pass enought arguments, return empty string because
				// too few arguments is not an error
				//
				ThreadContext.MacroWarning( "$[]{0} was found but there were too few arguments passed to macro \"{1}\" to give it a value", value, macro.Name );
				return string.Empty;
			}

			var argumentName  = arguments [ index ];
			if( mp.IsGeneratedName( argumentName ) ) {
				//
				// if the object is a macro or an object this allows two things
				//
				// (1) it short circuit's the indexer call
				//
				// (2) it allows $[] to be used on the generated object and macros passed to to the macro, such as foreach target macro
				//
				return arguments [ index ];
			}
			else {
				return string.Format( "{0}[{1}]", objArgsMacro.Name, index );
			}

		}


		/////////////////////////////////////////////////////////////////////////////

		//private static string userMacroSubstStr = @"(?xs)([\\$]?[$][0-9#*@])|([\\$]?[$][a-zA-Z_][a-zA-Z0-9_]*)|([$]args)|(\.{)|(}\.)|(\.[+\-a-zA-Z0-9_]*?\.)";
		//
		// changed to allow the hash character to be part of a $ name
		//
		//private static string userMacroSubstStr = @"(?xs)([\\$]?[$][a-zA-Z_#][a-zA-Z0-9_#]*)|([\\$]?[$][0-9#*@])|([$]args)|(\.{)|(}\.)|(\.[+\-a-zA-Z0-9_]*?\.)";
		//
		// removed for \$  -  [\\$]?  is now [$]?
		//
		//private static string userMacroSubstStr = @"(?xs)([$]?[$][a-zA-Z_#][a-zA-Z0-9_#]*)|([\\$]?[$][0-9#*@])|([$]args)|(\.{)|(}\.)|(\.[+\-a-zA-Z0-9_]*?\.)";

		//private static string userMacroSubstStr = @"(?xs)([$]?[$][a-zA-Z_#][a-zA-Z0-9_#]*)|([$]?\$\[\][a-zA-Z_#][a-zA-Z0-9_#]*)|([\\$]?[$][0-9#*@])|([$]args)|(\.{)|(}\.)|(\.[+\-a-zA-Z0-9_]*?\.)";


		private static string userMacroSubstStr = @"(?xs)([$]?[$][a-zA-Z_#][a-zA-Z0-9_#]*)|([$]?\$\[\][a-zA-Z0-9_#][a-zA-Z0-9_#]*)|([\\$]?[$][0-9#*@])|([$]args)|(\.{)|(}\.)|(\.[+\-a-zA-Z0-9_]*?\.)";

		private static Regex userMacroRegex = new Regex( userMacroSubstStr );

		protected string ArgumentSubstitutions( string text )
		{
			/*	where:
				
							$macroName	is the macro name

							$0-$9				positional macro argument		- args[1] ... args[9]
						
							$#					number of args  ?? include macro name
						
							$*					expands to all args separated by commas

							$@					expands to all args quoted and separated by commas
						

					the  [\\$] allow \$ or $$ for escaping (one level gets removed)
				
				*/

			// ******
			//
			// should compile this into a static member
			//
			string result = userMacroRegex.Replace( text, match => { return ArgSubstMatch( match ); } );
			if( reExpand ) {
				result = userMacroRegex.Replace( result, match => { return ArgSubstMatch( match ); } );
			}

			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string PreProcessText( string input )
		{
			if( options.NoSubst ) {
				return input;
			}

			return ArgumentSubstitutions( input );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string PostProcessText( string input )
		{
			// ******
			if( !printBlockRefed && !embededChars ) {
				return input;
			}

			// ******
			var sb = new StringBuilder();

			char beginBlock = SC.DEFAULT_BEGINPRINTBLOCK_CHAR;
			char endBlock = SC.DEFAULT_ENDPRINTBLOCK_CHAR;

			// ******
			int strLen = input.Length;
			bool copying = printBlockRefed ? false : true;

			// ******
			for( int i = 0; i < strLen; i++ ) {
				char ch = input [ i ];

				// ******
				if( beginBlock == ch ) {
					copying = true;
					continue;
				}
				else if( endBlock == ch ) {
					copying = false;
					continue;
				}

				// ******
				if( copying ) {
					switch( ch ) {
						case SC.EMBED_SPACE_CHAR:
							ch = SC.SPACE;
							break;

						case SC.EMBED_NEWLINE_CHAR:
							ch = SC.NEWLINE;
							break;

						case SC.EMBED_TAB_CHAR:
							ch = SC.TAB;
							break;
					}

					// ******
					sb.Append( ch );
				}
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Run()
		{
			// ******
			string result = string.Empty;

			// ******
			string preProcessedText = PreProcessText( macro.MacroText );

			// ******
			if( options.FwdExpand ) {
				//
				// additional info for macros
				//
				////var locals = localArrayMacro.MacroObject as NmpArray;
				//localArray.Add( INVOKED_AS, expressionNodeTypeName );

				// ******
				//result = mp.ScanHelper.Scanner( preProcessedText, "macro " + macro.Name );
				result = mp.Get<IScanner>().Scanner( preProcessedText, "macro " + macro.Name );
			}
			else {
				result = preProcessedText;
			}

			// ******
			string finalResult = PostProcessText( result );

			// ******
			if( options.Format ) {
				try {
					finalResult = string.Format( finalResult, objectArgs );
				}
				catch( Exception ex ) {
					ThreadContext.MacroError( ExceptionHelpers.RecursiveMessage( ex, "calling string.Format() on Text macro \"{0}\" an exception was thrown: {1}", macro.Name, ex.Message ) );
				}
			}

			// ******
			//
			// check localArray for additional instructions
			//
			if( localArray.Contains( "eval" ) || options.Eval ) {
				finalResult = EvalResult( finalResult );
			}

			if( localArray.Contains( "divert" ) ) {
				options.Divert = Helpers.IsMacroTrue( localArray [ "divert" ] );
			}

			if( options.TabsToSpaces ) {
				finalResult = finalResult.Replace( "\t", "  " );
			}

			// ******
			return finalResult;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			mp.DeleteMacro( objArgsMacro );
			mp.DeleteMacro( localArrayMacro );
			mp.DeleteMacro( specials );
			mp.DeleteMacro( argsMacro );
		}


		/////////////////////////////////////////////////////////////////////////////

		public TextMacroRunner( IMacroProcessor mp, IMacro macro, IMacroArguments args, object [] objArgsIn )
		{
			// ******
			this.mp = mp;
			this.macro = macro;
			this.options = args.Options;
			this.objectArgs = objArgsIn;
			this.arguments = new NmpStringList( objArgsIn, false );

			// ******
			expressionNodeTypeName = args.Expression.NodeTypeName;

			// ******
			///flags = new BitArray( (int)MDIndexes.COUNT_DIRECTIVES, false );

			// ******
			argsMacro = mp.AddObjectMacro( mp.GenerateArgListName( macro.Name ), this.arguments );
			specials = mp.AddObjectMacro( mp.GenerateLocalName( macro.Name + "_specials" ), args.SpecialArgs );
			localArrayMacro = mp.AddObjectMacro( mp.GenerateLocalName( macro.Name ), localArray = new NmpArray() );
			objArgsMacro = mp.AddObjectMacro( mp.GenerateArgListName( macro.Name + "Objects" ), objArgsIn );
		}


	}


}
