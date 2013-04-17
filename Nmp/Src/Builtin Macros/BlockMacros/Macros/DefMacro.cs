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
using Nmp.Expressions;


#pragma warning disable 169

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Define a text or object macro, which one is determined by the type of
	/// object passed to the Evaluate() as the expressions second argument method
	/// </summary>

	class Defmacro : BlockMacroHandlerBase {

		// ******
		//
		// (#defmacro `macroName' [,arg0, arg1 ...]) ... (#endmacro)
		//
		IEnumerable<Type> _argTypes = new Type [] { typeof( string ), typeof( string [] ) };
		IEnumerable<object> _defArgs = new object[] { string.Empty, new string[0] };

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }

		// ******
		CoreMacros builtins;

		//
		// no hash in name because its alt format only
		//
		const string	DEFMACRO					= "defmacro";

		const string	DEFMACRO_START		= "#defmacro";
		const string	PUSHMACRO_START		= "#pushdef";
		const string	ENDMACRO					= "#endmacro)";

		const string	DEFMACRO_INJECT		= "(#defmacro ";
		const string	PUSHMACRO_INJECT	= "(#pushdef ";
		const string	ENDMACRO_INJECT		= "(#endmacro)";


		int	tempLine = -1;



		///////////////////////////////////////////////////////////////////////////

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

				if( SC.NO_CHAR == ch ) {
					ThreadContext.MacroError( "defmacro/pushmacro: end of input before (#endmacro) found" );
				}
				
				// ******				
				//
				// (#defmacro ...
				// (#pushmacro ...
				//
				if( SC.OPEN_PAREN == ch && input.StartsWith(DEFMACRO_START) ) {
					//
					// get recursive
					//
					input.Skip( DEFMACRO_START.Length );

					if( ! char.IsWhiteSpace(input.Peek()) ) {
						ThreadContext.MacroError( "expected white space following \"(#defmacro\"" );
					}
					
					sb.Append( DEFMACRO_INJECT );
					ParseMacro( 1 + depth, input, sb );
					sb.Append( ENDMACRO_INJECT );
					continue;
				}

		//
		// easier / shorter way to do this - duping code from above
		//
				else if( SC.OPEN_PAREN == ch && input.StartsWith(PUSHMACRO_START) ) {
					//
					// get recursive
					//
					input.Skip( PUSHMACRO_START.Length );

					if( ! char.IsWhiteSpace(input.Peek()) ) {
						ThreadContext.MacroError( "expected white space following \"(#defmacro\"" );
					}
					
					sb.Append( PUSHMACRO_INJECT );
					ParseMacro( 1 + depth, input, sb );
					sb.Append( ENDMACRO_INJECT );
					continue;
				}

					//case SC.EMBED_BEGIN_INBLOCK_EXPAND_CHAR:
					//	if( 0 == depth ) {
					//		//
					//		// only for the outermost macro code
					//		//
					//		sb.Append( HandleEmbedMacroBlock(input) );
					//		continue;
					//	}
					//	break;

				else if( SC.OPEN_PAREN == ch && input.StartsWith(ENDMACRO) ) {
					input.Skip( ENDMACRO.Length );
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
			tempLine = input.Line;

			// ******
			//
			// remove any newline following the closing paren (#macro ...)
			//
			if( SC.NEWLINE == input.Peek() ) {
				input.Skip( 1 );
			}

			// ******
			StringBuilder macroText = new StringBuilder();
			ParseMacro( 0, input, macroText );

			// ******
			//
			// remove any newline following the closing paren for the (#endmacro)
			//
			if( SC.NEWLINE == input.Peek() ) {
				input.Skip( 1 );
			}

			// ******
			return macroText.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// (#defmacro `macroName' [, `positional, arg, names'] )
		//
		/////////////////////////////////////////////////////////////////////////////

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			var args = GetMacroArgsAsTuples( macroArgs.Expression ) as NmpTuple<string, string []>;
			string macroName = args.Item1;
			string [] optArgs = args.Item2;

			// ******
			return builtins.DefineMacro( macroArgs, macroName, macroArgs.BlockText, optArgs, Pushdef.PUSHDEF == macro.Name );
		}


		/////////////////////////////////////////////////////////////////////////////

		public Defmacro( IMacroProcessor mp, CoreMacros builtins )
			: base(DEFMACRO, mp)
		{
			this.builtins = builtins;
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Defmacro( string name, IMacroProcessor mp, CoreMacros builtins )
			: base(name, mp)
		{
			this.builtins = builtins;
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp, CoreMacros builtins )
		{
			// ******
			var handler = new Defmacro( mp, builtins );
			IMacro macro = mp.CreateBlockMacro( handler.Name, handler );
			macro.Flags |= MacroFlags.AltTokenFmtOnly | MacroFlags.RequiresArgs;

			// ******
			return macro;
		}

	}
}
