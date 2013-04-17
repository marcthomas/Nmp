#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// SplitString.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Globalization;
using System.IO;
using System.Reflection;

namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Splits strings for the Arguments class, keeps casts and the values being
	/// cast together, maintains object array items together
	/// </summary>

	public static class SplitString {


		public const char	END_OF_STRING	= StringIndexer.EOS;

		const char	OPEN_BRACKET	= '[';
		const char	CLOSE_BRACKET	= ']';

		const char	OPEN_PAREN		= '(';
		const char	CLOSE_PAREN		= ')';

		const char	ESCAPE_CHAR		= '\\';
		const char	SPLIT_CHAR		= ';';

		const string escapeChars = "\"[]();";


		/////////////////////////////////////////////////////////////////////////////

		public static NmpStringList Split( string s )
		{
			return Split( new StringIndexer(s), END_OF_STRING, END_OF_STRING );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static NmpStringList Split( StringIndexer input, char openChar, char closeChar, bool splitCastMode = false )
		{
			// ******
			NmpStringList subStrings = new NmpStringList();

			// ******
			StringBuilder sb = new StringBuilder();
			char ch;

			while( true ) {
				ch = input.NextChar();

				// ******
				if( ESCAPE_CHAR == ch ) {
					ch = input.NextChar();

					if( END_OF_STRING == ch ) {
						continue;
					}
					else if( escapeChars.IndexOf(ch) >= 0 || openChar == ch || closeChar == ch ) {
						sb.Append( ch );
						continue;
					}

					//
					// not a valid escape char so add escape into output and fall thru to handle
					// ch
					//
					sb.Append( ESCAPE_CHAR );
				}

				// ******
				if( END_OF_STRING == ch || SPLIT_CHAR == ch || closeChar == ch ) {
					subStrings.Add( sb.ToString().Trim() );
					sb.Length = 0;

					if( END_OF_STRING == ch || closeChar == ch ) {
						break;
					}
				}
				else if( OPEN_BRACKET == ch || OPEN_PAREN == ch || openChar == ch ) {
					char closer;

					switch( ch ) {
						case OPEN_BRACKET:
							closer = CLOSE_BRACKET;
							break;

						case OPEN_PAREN:
							closer = CLOSE_PAREN;
							break;

						default:
							closer = closeChar;
							break;
					}
					
					// ******
					NmpStringList s = Split( input, ch, closer );

					sb.Append( ch );
					sb.Append( s.Join(';') );
					sb.Append( closer );

					// ******
					if( OPEN_PAREN == ch && splitCastMode ) {
						//
						// once we've seen a complete parenthesized chunk of text we:
						//
						//		add it to the list
						//		then add the remainder of the text to the next entry
						//		and return
						//
						subStrings.Add( sb.ToString() );
						subStrings.Add( input.Remainder.Trim() );
						return subStrings;
					}
				}
				else {
					sb.Append( ch );
				}
			}

			// ******
			//
			// did not find terminating character
			//
			if( END_OF_STRING != closeChar && END_OF_STRING == ch ) {
				throw ExceptionHelpers.CreateException( "Helpers.SplitString: unbalanced string, could not locate closing character '{0}'", closeChar );
			}

			// ******
			return subStrings;
		}


	}


}
