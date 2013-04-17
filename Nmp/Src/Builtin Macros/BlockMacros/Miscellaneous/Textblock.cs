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

	class Textblock : BlockMacroHandlerBase {

		const string	TEXTBLOCK		= "tb";

		// ******
		//
		// (#tb) ... (#endtb)
		//
		IEnumerable<Type> _argTypes = new Type [ 0 ];
		IEnumerable<object> _defArgs = new object [ 0 ];

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }

		// ******
		// (#tb )
		//
		// ...
		//
		// (#endtb)

		///////////////////////////////////////////////////////////////////////////

		public const 	string TEXTBLOCK_BEGIN	= "(#tb)";
		public const 	string TEXTBLOCK_END		= "(#endtb)";


		const string	TB_START			= "#tb";
		const string	TBEND					= "#endtb)";

		const string	TB_INJECT			= "(#tb ";
		const string	TBEND_INJECT	= "(#endtb)";


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
					ThreadContext.MacroError( "textblock: end of input before (#tbend) found" );
				}
				
				// ******				
				//
				// (#textblock ...
				//
				if( SC.OPEN_PAREN == ch && input.StartsWith(TB_START) ) {
					//
					// get recursive
					//
					input.Skip( TB_START.Length );

					//if( ! char.IsWhiteSpace(input.Peek()) ) {
					//	ThreadContext.MacroError( "expected white space following \"(#Textblock\"" );
					//}
					
					sb.Append( TB_INJECT );
					ParseMacro( 1 + depth, input, sb );
					sb.Append( TBEND_INJECT );
					continue;
				}

				else if( SC.OPEN_PAREN == ch && input.StartsWith(TBEND) ) {
					input.Skip( TBEND.Length );
					return;
				}

				//else if( ThreadContext.SeqOpenQuote.FirstChar == ch && ThreadContext.SeqOpenQuote.Starts(input) ) {
				//	//
				//	// GetQuotedText() strips the outer quotes but perserves inner quotes; but
				//	// first we need to eat the open quote
				//	//
				//	ThreadContext.SeqOpenQuote.Skip( input );
				//	sb.Append( ThreadContext.GetQuotedText(input, true) );
				//}

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

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			string text = string.Empty;

			if( macroArgs.BlockText.Length > 0 ) {
				NamedTextBlocks blocks = gc.GetTextBlocks();
				text = blocks.AddTextBlock( macroArgs.BlockText );
			}

			// ******
			return text;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Textblock( IMacroProcessor mp )
			: base(TEXTBLOCK, mp)
		{
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp )
		{
			// ******
			var handler = new Textblock( mp );
			IMacro macro = mp.CreateBuiltinMacro( handler.Name, handler );
			macro.Flags |= MacroFlags.AltTokenFmtOnly;

			// ******
			return macro;
		}

	}
}
