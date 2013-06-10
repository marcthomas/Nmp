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
using NmpEvaluators;
using NmpExpressions;

#pragma warning disable 169

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	class ForloopMacros : BlockMacroHandlerBase {

		// ******
		//
		// (#forloop `start', `end', `increment' [,optional0, optional1 ...]) ... (#endforloop)
		//
		IEnumerable<Type> _argTypes = new Type [] { typeof( int ), typeof( int ), typeof( int ), typeof( object [] ) };
		IEnumerable<object> _defArgs = new object[] { 0, 0, 0, new object[0] };

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }


		// ******
		//
		// no hash in name because its alt format only
		//
		const string	FORLOOP									= "#forloop";

		const string	BLOCK_FORLOOP						= "forloop";
		const string	BLOCK_ENDFORLOOP				= "endforloop";

		const string	BLOCK_FORLOOP_START			= "#forloop";
		const string	BLOCK_FORLOOP_END				= "#endforloop)";

		const string	BLOCK_FORLOOP_INJECT		= "(#forloop ";
		const string	BLOCK_ENDFORLOOP_INJECT	= "(#endforloop)";


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
					ThreadContext.MacroError( "defmacro/pushmacro: end of input before (#endforloop) found" );
				}
				
				// ******				
				//
				// (#defmacro ...
				// (#pushmacro ...
				//
				if( SC.OPEN_PAREN == ch && input.StartsWith(BLOCK_FORLOOP_START) ) {
					//
					// get recursive
					//
					input.Skip( BLOCK_FORLOOP_START.Length );

					if( ! char.IsWhiteSpace(input.Peek()) ) {
						ThreadContext.MacroError( "expected white space following \"(#defmacro\"" );
					}
					
					sb.Append( BLOCK_FORLOOP_INJECT );
					ParseMacro( 1 + depth, input, sb );
					sb.Append( BLOCK_ENDFORLOOP_INJECT );
					continue;
				}

				else if( SC.OPEN_PAREN == ch && input.StartsWith(BLOCK_FORLOOP_END) ) {
					input.Skip( BLOCK_FORLOOP_END.Length );
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
		// (#forloop `startValue', `endValue', `increment' [, extraArgs ...] )
		//
		/////////////////////////////////////////////////////////////////////////////

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			var args = GetMacroArgsAsTuples( macroArgs.Expression ) as NmpTuple<int, int, int, object[]>;

			int start = args.Item1;
			int end = args.Item2;
			int increment = args.Item3;
			object [] extraArgs = args.Item4;

			// ******
			return new ForloopHandler(mp).Forloop_TextChunk( start, end, increment, macroArgs.BlockText, extraArgs );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ForloopMacros( IMacroProcessor mp )
			: base(BLOCK_FORLOOP, mp)
		{
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp )
		{
			// ******
			var handler = new ForloopMacros( mp );
			IMacro macro = mp.CreateBuiltinMacro( handler.Name, handler );
			macro.Flags |= MacroFlags.AltTokenFmtOnly | MacroFlags.RequiresArgs;

			// ******
			return macro;
		}

	}
}
