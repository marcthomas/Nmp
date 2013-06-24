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

		///////////////////////////////////////////////////////////////////////////

		public string EncodeNmpQuotes( string text )
		{
			// ******
			if( 2 != SeqOpenQuote.CountChars + SeqCloseQuote.CountChars ) {
				return text;
			}

			// ******
			var sb = new StringBuilder { };

			foreach( var ch in text ) {
				if( SeqOpenQuote.FirstChar == ch || SeqCloseQuote.FirstChar == ch ) {
					sb.Append( EscapedChars.Add( ch ) );
				}
				else {
					sb.Append( ch );
				}
			}

			// ******
			return sb.ToString();
		}


		///////////////////////////////////////////////////////////////////////////

		public string FixText( string [] strs )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			//
			// we will only escape these guys if they're each only one character long
			//
			bool checkQuotes = 2 == SeqOpenQuote.CountChars + SeqCloseQuote.CountChars;

			foreach( string str in strs ) {
				char lastCh = SC.NO_CHAR;

				// ******
				//foreach( char ch in str ) {
				//	switch( ch ) {
				//		case SC.CR:
				//			sb.Append( SC.NEWLINE );
				//			break;
				//
				//		case SC.NEWLINE:
				//			if( SC.CR != lastCh ) {
				//				sb.Append( SC.NEWLINE );
				//			}
				//			break;
				//
				//		case '\u0085':		// NEL - next line
				//		case '\u2028':		// LS  - line separator
				//		case '\u2029':		// PS  - paragraph separator
				//
				//			// do any of these play together like cr/lf ??
				//
				//			sb.Append( SC.NEWLINE );
				//			break;
				//
				//		case '\0':
				//			//
				//			// dont allow
				//			//
				//			break;
				//
				//		default:
				//			sb.Append( ch );
				//			break;
				//	}
				//
				//	// ******
				//	lastCh = ch;
				//}


				// ******
				int lastIndex = str.Length - 1;
				
				for( int iCh = 0; iCh <= lastIndex; iCh++ ) {
					char ch = str[ iCh ];
					char chNext = iCh < lastIndex ? str[ 1 + iCh ] : SC.NO_CHAR;
	 			
					//
					// check for open/close quote being escaped
					//
					//if( SC.BACKSLASH == ch && checkQuotes ) {
					//	if( SeqOpenQuote.FirstChar == chNext || SeqCloseQuote.FirstChar == chNext ) {
					//		if( SC.BACKSLASH == lastCh ) {
					//			//
					//			// \\` or \\' - escaping the backslash and leaving the quote in place
					//			//
					//			// \` or \'
					//			//
					//			lastCh = SC.BACKSLASH;
					//		}
					//		else {
					//			//
					//			// embed open quote
					//			//
					//			++iCh;
					//			lastCh = GetEscapedChars().Add(chNext);
					//			sb.Append( lastCh );
					//		}
							
					//		continue;
					//	}
					//}

					//if( SC.BACKSLASH == ch ) {
					//	//
					//	// current char is a backslash
					//	//
					//	if( SC.BACKSLASH == lastCh ) {
					//		//
					//		// two backslashes in a row
					//		//
					//		// \\` or \\' - escaping the backslash and leaving the quote in place
					//		//
					//		// \` or \'
					//		//
					//		lastCh = SC.BACKSLASH;
					//	}
					//	else if( checkQuotes && (SeqOpenQuote.FirstChar == chNext || SeqCloseQuote.FirstChar == chNext) ) {
					//		//
					//		// \` or \' - embed open or close quote
					//		//
					//		++iCh;
					//		lastCh = GetEscapedChars().Add( chNext );
					//		sb.Append( lastCh );
					//		continue;
					//	}
					//	else if( SC.COMMA == chNext ) {
					//		//
					//		// \, - embed comma
					//		//
					//		++iCh;
					//		lastCh = GetEscapedChars().Add( chNext );
					//		sb.Append( lastCh );
					//		continue;
					//	}
					//}

					if( SC.BACKSLASH == ch ) {
						var isOpenOrCloseQuote = checkQuotes && (SeqOpenQuote.FirstChar == chNext || SeqCloseQuote.FirstChar == chNext);

						if( SC.BACKSLASH == lastCh ) {	//&& isOpenOrCloseQuote ) {
							//
							// escape backslash - "\\" -> "\"
							//
							// *** i am torn, should this ALWAYS escape or only when followed by an open or close quote ***
							//
							lastCh = SC.BACKSLASH;
							continue;
						}
						else if( isOpenOrCloseQuote ) {
							//
							// \` or \' - embed open or close quote
							//
							++iCh;
							lastCh = EscapedChars.Add( chNext );
							sb.Append( lastCh );
							continue;
						}
						else if( SC.COMMA == chNext ) {
							//
							// \, - embed comma
							//
							++iCh;
							lastCh = EscapedChars.Add( chNext );
							sb.Append( lastCh );
							continue;
						}
					}


			
					// ******
					switch( ch ) {
						case SC.CR:
							sb.Append( SC.NEWLINE );
							break;
				
						case SC.NEWLINE:
							if( SC.CR != lastCh ) {
								sb.Append( SC.NEWLINE );
							}
							break;
				
						case '\u0085':		// NEL - next line
						case '\u2028':		// LS  - line separator
						case '\u2029':		// PS  - paragraph separator
				
							// do any of these play together like cr/lf ??
				
							sb.Append( SC.NEWLINE );
							break;
				
						case '\0':
							//
							// dont allow
							//
							break;
				
						default:
							sb.Append( ch );
							break;
					}
				
					// ******
					lastCh = ch;
				}
			}

			// ******
			string result = sb.ToString();
			return result;
		}


		///////////////////////////////////////////////////////////////////////////

		public string FixText( string str )
		{
			return FixText( new string [] { str } );
		}

	}

}