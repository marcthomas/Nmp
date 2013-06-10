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

//#pragma warning disable 169

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	class ExpandoBlock : BlockMacroHandlerBase {

		const string	ExpandoBlockName		= "#&";

		// ******
		//
		// (##& ... ##)
		//
		IEnumerable<Type> _argTypes = new Type[0];
		IEnumerable<object> _defArgs = new object[0];

		// ******
		protected override IEnumerable<Type> 		ExpectedArgTypes		{ get { return _argTypes; } }
		protected override IEnumerable<object>	DefaultArgs { get { return _defArgs; } }

		// ******
		//
		// (## text to expand ##)
		//

		///////////////////////////////////////////////////////////////////////////

		const string	EB_START			= "##";
		const string	EBEND					= "#)";

		const string	EB_INJECT			= "(##";
		const string	EBEND_INJECT	= "##)";


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
					ThreadContext.MacroError( "ExpandoBlock: end of input before \"##)\" found" );
				}
				
				// ******				
				//
				// (## ...
				//
				if( SC.OPEN_PAREN == ch && input.StartsWith(EB_START) ) {
					//
					// get recursive
					//
					input.Skip( EB_START.Length );

					sb.Append( EB_INJECT );
					ParseMacro( 1 + depth, input, sb );
					sb.Append( EBEND_INJECT );
					continue;
				}

				else if( SC.HASH == ch && input.StartsWith(EBEND) ) {
					input.Skip( EBEND.Length );
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
			StringBuilder macroText = new StringBuilder();
			ParseMacro( 0, input, macroText );
			return macroText.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			if( 0 == macroArgs.BlockText.Length ) {
				return string.Empty;
			}

			// ******
			string text = string.Empty;

			//
			// run the contents of the block through the scanner
			//
			text = macroArgs.BlockText;
			int hash = text.GetHashCode();

			var scanner = mp.Get<IScanner>();
			for( int i = 0; i < 128; i++ ) {
				text = scanner.Scanner( text, string.Format( "ExpandoBlock_{0}", i ) );
				int newHash = text.GetHashCode();

				if( hash == newHash ) {
					///ThreadContext.WriteLine( "exit eval block on iteration {0}", i );
					break;
				}
				hash = newHash;
			}

			// ******
			return text;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExpandoBlock( IMacroProcessor mp )
			: base(ExpandoBlockName, mp)
		{
			handlesBlocks = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static IMacro Create( IMacroProcessor mp )
		{
			// ******
			var handler = new ExpandoBlock( mp );
			IMacro macro = mp.CreateBuiltinMacro( handler.Name, handler );
			macro.Flags |= MacroFlags.AltTokenFmtOnly | MacroFlags.NonExpressive;

			// ******
			return macro;
		}

	}
}
