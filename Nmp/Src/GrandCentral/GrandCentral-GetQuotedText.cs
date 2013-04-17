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
using System.Linq;
using System.Text;

using NmpBase;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	partial class GrandCentral {


		/////////////////////////////////////////////////////////////////////////////
		
		public string GetQuotedText( IParseReader input, bool keepQuotes, bool processEscapes )
		{
			bool checkEscapesAndSpecialChars = input.CheckEscapesAndSpecialChars;
			input.CheckEscapesAndSpecialChars = processEscapes;

			string result = GetQuotedText( input, keepQuotes );

			input.CheckEscapesAndSpecialChars = checkEscapesAndSpecialChars;
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public string GetQuotedText( IParseReader input, bool keepQuotes )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			if( keepQuotes ) {
				sb.Append( SeqOpenQuote.Sequence );
			}
		
			// ******
			while( true ) {
				char ch = input.Peek();

				if( SeqOpenQuote.FirstChar == ch && SeqOpenQuote.Starts(input) ) {
					SeqOpenQuote.Skip( input );

					// ******
					sb.Append( GetQuotedText(input, true) );
				}

				else if( SeqCloseQuote.FirstChar == ch && SeqCloseQuote.Starts(input) ) {
					SeqCloseQuote.Skip( input );

					// ******
					if( keepQuotes ) {
						sb.Append( SeqCloseQuote.Sequence );
					}
					return sb.ToString();
				}
				else if( SC.NO_CHAR == ch ) {

	// TODO: need file/line/col number for use with MPError

					ThreadContext.MacroError( "end of data: unbalanced quotes" );

				}
				else {
					input.Skip( 1 );
					sb.Append( ch );
				}
			}
		}
			
			
		/////////////////////////////////////////////////////////////////////////////
		
		public string GetQuotedText( StringIndexer input, bool keepQuotes )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			if( keepQuotes ) {
				sb.Append( SeqOpenQuote.Sequence );
			}
		
			// ******
			while( true ) {
				char ch = input.Peek();

				if( SeqOpenQuote.FirstChar == ch && SeqOpenQuote.Starts(input) ) {
					SeqOpenQuote.Skip( input );

					// ******
					sb.Append( GetQuotedText(input, true) );
				}

				else if( SeqCloseQuote.FirstChar == ch && SeqCloseQuote.Starts(input) ) {
					SeqCloseQuote.Skip( input );

					// ******
					if( keepQuotes ) {
						sb.Append( SeqCloseQuote.Sequence );
					}
					return sb.ToString();
				}
				else if( SC.NO_CHAR == ch ) {

	// TODO: need file/line/col number for use with MPError

					ThreadContext.MacroError( "end of data: unbalanced quotes" );

				}
				else {
					input.Skip( 1 );
					sb.Append( ch );
				}
			}
		}

	}

}