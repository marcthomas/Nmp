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
using System.Reflection;


using NmpBase;
//using NmpExpressions;
using Nmp.Expressions;

#pragma warning disable 169

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Define a text or object macro, which one is determined by the type of
	/// object passed to the Evaluate() as the expressions second argument method
	/// </summary>

	class GenericBlock : BlockMacroHandlerBase {

		const string	GENERIC_BLOCK		= "block";

		// ******
		//
		// (#block `thingToInvoke' [,arg0, arg1 ...]) ... (#endblock)
		//
		// string		-	what needs to be invoked, macro or powershell with text as script
		// object[]	- additional arguments to script
		//
		IEnumerable<Type> _argTypes = new Type[] { typeof(string), typeof(object[]) };
		IEnumerable<object> _defArgs = new object[0];	// { "", "" };

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }

		// ******
		// (#block )
		//
		// ...
		//
		// (#endblock)

		///////////////////////////////////////////////////////////////////////////

		const string	BLOCK_START				= "#block";
		const string	BLOCK_END					= "#endblock)";

		const string	BLOCK_INJECT			= "(#block ";
		const string	BLOCKEND_INJECT		= "(#endblock)";


		private void ParseMacro( int depth, IInput input, StringBuilder sb )
		{
			// ******
			char quoteStartChar = gc.SeqOpenQuote.FirstChar;

			// ******
			while( true ) {
				//
				// first quote check
				//
				if( quoteStartChar == input.Peek() && gc.SeqOpenQuote.Starts(input) ) {
					//
					// GetQuotedText() strips the outer quotes but perserves inner quotes; but
					// first we need to eat the open quote
					//
					gc.SeqOpenQuote.Skip( input );
					sb.Append( gc.GetQuotedText(input, true) );
				}

				// ******				
				char ch = input.Next();

				// ******				
				if( SC.NO_CHAR == ch ) {
					ThreadContext.MacroError( "block error: end of input before (#endblock) found" );
				}
				
				// ******				
				//
				// (#block ...
				//
				if( SC.OPEN_PAREN == ch && input.StartsWith(BLOCK_START) ) {
					//
					// get recursive
					//
					input.Skip( BLOCK_START.Length );

					// ******
					sb.Append( BLOCK_INJECT );
					ParseMacro( 1 + depth, input, sb );
					sb.Append( BLOCKEND_INJECT );
					continue;
				}

				else if( SC.OPEN_PAREN == ch && input.StartsWith(BLOCK_END) ) {
					input.Skip( BLOCK_END.Length );
					return;
				}

				else {
					sb.Append( ch );
				}
			}
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public override string ParseBlock( Expression exp, IInput input )
		{
			// ******
			//
			// remove any newline following the closing paren (#textblock ...)
			//
			if( SC.NEWLINE == input.Peek() ) {
				input.Skip( 1 );
			}

			// ******
			StringBuilder macroText = new StringBuilder();
			ParseMacro( 0, input, macroText );

			// ******
			//
			// remove any newline following the closing paren for the (#endtextblock)
			//
			if( SC.NEWLINE == input.Peek() ) {
				input.Skip( 1 );
			}

			// ******
			return macroText.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		private void Powershell( string blockText, string name )
		{
			// ******
			if( null == mp.Powershell ) {
				ThreadContext.MacroError( "block macro is unable to create Powershell script, Powershell is not loaded" );
			}

			// ******
			if( string.IsNullOrEmpty(name) ) {
				//
				// execute script
				//
				ThreadContext.MacroError( "block macro expected a macro name to save Powershell script under" );
			}
			else {
				//
				// create script macro
				//
				mp.Powershell.CreateScriptMacro( name, blockText );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// where blockOperation is the name of a macro that we are to call,
		// BlockText is the text between (#block `macroName' ..) ... (#endblock)
		// that is to be passed as the first argument; any additional arguments
		// to (#block ...) are passed next
		//

		private object MacroCall( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			//var args = GetMacroArgsAsTuples( macroArgs.Expression, new Type [] { typeof( string ), typeof( object [] ) } ) as NmpTuple<string, object []>;
			var args = GetMacroArgsAsTuples( macroArgs.Expression ) as NmpTuple<string, object []>;
			string macroName = args.Item1;
			object [] argArray = args.Item2;

			// ******
			object [] argsToMacro = new object [ 1 + argArray.Length ];

			argsToMacro[ 0 ] = macroArgs.BlockText;
			Array.Copy( argArray, 0, argsToMacro, 1, argArray.Length );

			// ******
			//
			// macro( blockText [, additiona args ... ] )
			//
			//MacroExpression expression = ETB.CreateMacroCallExpression( macro, argsToMacro );
			return mp.InvokeMacro( macro, argsToMacro, true );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Evaluate( IMacro macro, IMacroArguments argsIn )
		{
			const string POWERSHELL = "psscript";

			// ******
			//var args = GetMacroArgsAsTuples( macroArgs.Expression ) as NmpTuple<string, string, string>;
			//string blockOperation = args.Item1;
			//string name = args.Item2;

			int skipCount;
			ArgumentList argList = GetArguments( macro.Name, argsIn.Expression, out skipCount );

			var macroArgs = new MacroArgs( mp, Name, ExpectedArgTypes, DefaultArgs );
			var result = macroArgs.AsTuples( argList );

			//var args = result as NmpTuple<string, string, string>;

			var args = result as NmpTuple<string, object[]>;
			string blockOperation = args.Item1;
			string name = args.Item1;	//2 [ 0 ].ToString();




			// ******
			IMacro target = null;

			if( POWERSHELL == blockOperation ) {
				Powershell( argsIn.BlockText, name );
			}
			else if( mp.FindMacro(blockOperation, out target) ) {
				return MacroCall( target, argsIn );
			}
			else {
				ThreadContext.MacroError( "invalid block operation, or there is no macro by the name of: \"{0}\"", blockOperation );
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public GenericBlock( IMacroProcessor mp )
			: base(GENERIC_BLOCK, mp)
		{
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp )
		{
			// ******
			var handler = new GenericBlock( mp );
			IMacro macro = mp.CreateBlockMacro( handler.Name, handler );
			macro.Flags |= MacroFlags.AltTokenFmtOnly;

			// ******
			return macro;
		}

	}
}
