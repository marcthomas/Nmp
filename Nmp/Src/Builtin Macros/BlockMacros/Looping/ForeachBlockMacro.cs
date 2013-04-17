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
using NmpExpressions;
using Nmp.Expressions;

#pragma warning disable 169

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	class ForeachBlockMacro : BlockMacroHandlerBase {

		// ******
		//
		// (#foreach `trueOrFalseExpression') ... (#endforeach)
		//
		IEnumerable<Type> _argTypes = new Type [] { typeof( object ), typeof( object [] ) };
		IEnumerable<object> _defArgs = new object[] { null, new object[0] };

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }

		// ******

		//
		// no hash in name because its alt format only
		//
		//const string	FOREACH									= "#foreach";

		const string	BLOCK_FOREACH						= "foreach";
		const string	BLOCK_ENDFOREACH				= "foreach";

		const string	BLOCK_FOREACH_START			= "#foreach";
		const string	BLOCK_FOREACH_END				= "#endforeach)";

		const string	BLOCK_FOREACH_INJECT			= "(#foreach ";
		const string	BLOCK_ENDFOREACH_INJECT		= "(#endforeach)";


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
				if( SC.OPEN_PAREN == ch && input.StartsWith(BLOCK_FOREACH_START) ) {
					//
					// get recursive
					//
					input.Skip( BLOCK_FOREACH_START.Length );

					if( ! char.IsWhiteSpace(input.Peek()) ) {
						ThreadContext.MacroError( "expected white space following \"(#defmacro\"" );
					}
					
					sb.Append( BLOCK_FOREACH_INJECT );
					ParseMacro( 1 + depth, input, sb );
					sb.Append( BLOCK_ENDFOREACH_INJECT );
					continue;
				}

				else if( SC.OPEN_PAREN == ch && input.StartsWith(BLOCK_FOREACH_END) ) {
					input.Skip( BLOCK_FOREACH_END.Length );
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
		// (#foreach objToEnumerate [, `extraArgs'] )
		//
		/////////////////////////////////////////////////////////////////////////////

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			var args = GetMacroArgsAsTuples( macroArgs.Expression ) as NmpTuple<object, object[]>;
			object objToEnumerate = args.Item1;
			object [] extraArgs = args.Item2;

			// ******
			if( null == objToEnumerate ) {
				ThreadContext.MacroError( "(#foreach ...) requires an object to iterate over as its first argument" );
			}

			// ******
			var handler = new ForeachHandler( mp );
			return handler.Foreach_TextChunk( objToEnumerate, macroArgs.BlockText, extraArgs );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ForeachBlockMacro( IMacroProcessor mp )
			: base(BLOCK_FOREACH, mp)
		{
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp )
		{
			// ******
			var handler = new ForeachBlockMacro( mp );
			IMacro macro = mp.CreateBlockMacro( handler.Name, handler );
			macro.Flags |= MacroFlags.AltTokenFmtOnly | MacroFlags.RequiresArgs;

			// ******
			return macro;
		}

	}
}
