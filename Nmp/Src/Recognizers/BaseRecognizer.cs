#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// RegExRecognizer.cs
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NmpBase;


namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	class BaseRecognizer : IRecognizer {

		//
		// punctuation char that is allowed as the last character in a macro
		// name - when seen this terminates the name, can only appear in 
		// IsValidMacroIdentifier as the last character
		//
		const char	PUNCT_TERM_CHAR 			= '&';
		const char	POWERSHELL_START_CHAR	= SC.DOLLAR;

		protected static CharSequence altTokenStart = new CharSequence( "(#" );

		// ******
		GrandCentral gc;


		/////////////////////////////////////////////////////////////////////////////

		public CharSequence OpenQuote
		{
			get {
				return gc.SeqOpenQuote;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public CharSequence CloseQuote
		{
			get {
				return gc.SeqCloseQuote;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public CharSequence AltTokenStart
		{
			get {
				return altTokenStart;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual bool StartsWithAltTokenChars( string str )
		{
			// ******
			if( string.IsNullOrEmpty(str) || str.Length < altTokenStart.CountChars ) {
				return false;
			}

			// ******
			return altTokenStart.FirstChar == str[0] && altTokenStart.Starts(str);
		}


		/////////////////////////////////////////////////////////////////////////////

		private bool IsValidInnerTokenChar( char ch )
		{
			//
			// inner (non start) chars can also have digits but NOT $ chars
			//
			return char.IsLetterOrDigit(ch) || '_' == ch || SC.HASH == ch;
		}


		/////////////////////////////////////////////////////////////////////////////

		private bool IsStartChar( char ch )
		{
			return char.IsLetter(ch) || '_' == ch || SC.HASH == ch || SC.DOLLAR == ch;
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual bool IsMacroIdentifierStartChar( char ch )
		{
			return IsStartChar( ch );
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual bool IsValidMacroIdentifier( string name, bool allowDots = false )
		{
			// ******
			if( string.IsNullOrEmpty(name) ) {
				return false;
			}
		
			// ******
			for( int i = 0, len = name.Length; i < len; i++ ) {
				char ch = name[ i ];
		
				if( 0 == i ) {
					if( ! IsStartChar(ch) ) {
						return false;
					}
				}
				else if( ! char.IsLetter(ch) && ! char.IsDigit(ch) && '_' != ch && SC.HASH != ch ) {
					//
					// any char OTHER than letter, digit, underscore or hash
					//
					if( allowDots && '.' == ch ) {
						//
						// allowed
						//
					}
					else if( PUNCT_TERM_CHAR == ch && i == len - 1 ) {
						//
						// & can be the last character
						//
						return true;
					}
					else {
						return false;
					}
				}
			}

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		private bool IsPowershellStartChar( char ch )
		{
			return POWERSHELL_START_CHAR == ch;
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//private string PeekPwrShellToken( IInput input, int startIndex, char startChar )
		//{
		//	// ******
		//	StringBuilder sb = new StringBuilder();
		//	sb.Append( startChar );
		//
		//	// ******
		//	//
		//	// skip '$'
		//	//
		//	++startIndex;
		//
		//	for( int i = startIndex; ; i++ ) {
		//		char ch = input.Peek( i );
		//
		//		if( char.IsLetterOrDigit(ch) || '_' == ch ) {
		//			sb.Append( ch );
		//		}
		//		else {
		//			break;
		//		}
		//	}
		//
		//	// ******
		//	return sb.ToString();
		//}
		//
		
		/////////////////////////////////////////////////////////////////////////////
		
		private string PeekToken( IInput input, int startIndex )
		{
			// ******
			char ch = input.Peek( startIndex );

			//
			// if we're allowing $ as a start char then we can't differentiate
			// for powershell - user will just have to use legal powershell names
			// - perhaps can show some error in macro lookup
			//
			////
			//// powershell names are different
			////
			//if( IsPowershellStartChar(ch) ) {
			//	return PeekPwrShellToken( input, startIndex, ch );
			//}

			// ******
			StringBuilder sb = new StringBuilder();
		
			// ******
			//for( int i = startIndex; ; i++ ) {
			//	ch = input.Peek( i );
			//
			//	//
			//	// first char has already been validated so we're just checking for
			//	// any character that is valid within a macro name since that includes
			//	// all characters that can start a macro name
			//	//
			//	if( char.IsLetterOrDigit(ch) || '_' == ch || SC.HASH == ch ) {
			//		sb.Append( ch );
			//	}
			//	else if( PUNCT_TERM_CHAR == ch ) {
			//		//
			//		// last char can be '&' - added so we
			//		// can do things like this (##& and #macro& followed
			//		// immedatly by text
			//		//
			//		sb.Append( ch );
			//		break;
			//	}
			//	else {
			//		break;
			//	}
			//}
		
			//
			// first char already validated
			//
			sb.Append( input.Peek(startIndex) );

			for( int i = 1 + startIndex; ; i++ ) {
				ch = input.Peek( i );
		
				//
				// check for valid char OTHER than the start char
				//
				if( char.IsLetterOrDigit(ch) || '_' == ch || SC.HASH == ch ) {
					sb.Append( ch );
				}
				else if( PUNCT_TERM_CHAR == ch ) {
					//
					// last char can be '&' - added so we
					// can do things like this (##& and #macro& followed
					// immedatly by text
					//
					sb.Append( ch );
					break;
				}
				else {
					break;
				}
			}
		
			// ******
			return sb.ToString();
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public void Skip( IInput input, TokenMap tm )
		{
			// ******
			int skipCount = tm.MatchLength;
			input.Skip( skipCount );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetText( IInput input, TokenMap tm )
		{
			// ******
			//
			// offset index + length spans the entire match
			//
			string text = input.Next( tm.MatchLength );
			return text;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected RecognizedCharType OtherCharTypes( char ch )
		{
			// ******
			if( OpenQuote.FirstChar == ch ) {
				return RecognizedCharType.QuoteStartChar;
			}

			//else if( CloseQuote.FirstChar == ch ) {
			//	return RecognizedCharType.CloseQuoteStartChar;
			//}

			// ******
			switch( ch ) {
				case '(':
					return RecognizedCharType.OpenParenChar;

				case ')':
					return RecognizedCharType.CloseParenChar;

				case '[':
					return RecognizedCharType.OpenBracketChar;

				case ']':
					return RecognizedCharType.CloseBracketChar;

				case ',':
					return RecognizedCharType.ArgSepChar;

				default:
					return RecognizedCharType.JustAChar;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		private RecognizedCharType NonAltTokenTestCharType( char ch )
		{
			// ******
			//if( char.IsLetter(ch) || '_' == ch || SC.HASH == ch || POWERSHELL_START_CHAR == ch ) {
			//if( char.IsLetter(ch) || '_' == ch || SC.HASH == ch || SC.DOLLAR == ch  ) {
			
			//
			// calling IsMacroIdentifierStartChar() FAILS when using RegExRecognizer because
			// it is virtual and reg ex recognizer has a different notion of start chars ! so
			// we needed to create the local method IsStartChar()
			//
			
			if( IsStartChar(ch) ) {
				return RecognizedCharType.TokenStartChar;
			}

			// ******
			return OtherCharTypes( ch );
		}


		/////////////////////////////////////////////////////////////////////////////

		private RecognizedCharType TestCharType( IInput input )
		{
			// ******
			char ch = input.Peek();
			
			// ******
			if( input.RemainderCount > 2 ) {
				if( altTokenStart.FirstChar == ch && altTokenStart.Matches(input, 0) ) {
					
					char char3 = input.Peek( altTokenStart.CountChars );
					if( IsStartChar(char3) ) {
						return RecognizedCharType.AltTokenStartChar;
					}
				}
			}

			// ******
			return NonAltTokenTestCharType( ch );
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual RecognizedCharType Next( IInput input, out TokenMap tm )
		{
/*

	powershell support for a leaing $ presumes that Next() is only called by
	NmpScanner and that IsMacroIdentifierStartChar() and IsValidMacroIdentifier()
	are never called to validate a powershell name - because they will not be validated

	THIS IS PROBABLY INVALID

*/


			// ******
			RecognizedCharType rct = TestCharType( input );
			
			//
			// must tread tokens that start with '$' special - should do this check for
			// alt token starts as well but since we're going to disallow alt token starts
			// for macros as we rebuild this thing we'll ignore that
			//
			if(	RecognizedCharType.TokenStartChar == rct
					&& (SC.DOLLAR == input.Peek() || SC.HASH == input.Peek()) 
					&& ! IsValidInnerTokenChar(input.Peek(1)) ) {
				//
				// a token that starts with a $ can ONLY be followed by a '.' or a valid token
				//
				if( SC.DOT != input.Peek(1) ) {
					tm = null;
					return RecognizedCharType.JustAChar;
				}
			}

			// ******
			if( RecognizedCharType.TokenStartChar == rct || RecognizedCharType.AltTokenStartChar == rct ) {
				tm = new TokenMap();

				// ******
				if( RecognizedCharType.AltTokenStartChar == rct ) {
					//
					// skip "(#" to find token
					//
					tm.Token = PeekToken( input, altTokenStart.CountChars );
					tm.MatchLength = altTokenStart.CountChars + tm.Token.Length;
					tm.IsAltTokenFormat = true;
				}
				else {
					tm.Token = PeekToken( input, 0 );
					tm.MatchLength = tm.Token.Length;
					tm.IsAltTokenFormat = false;
				}

				// ******
				return RecognizedCharType.TokenStart;
			}

			// ******
			tm = null;
			return rct;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetMacroName( IInput input, out TokenMap tm )
		{
			// ******
			RecognizedCharType charType = Next( input, out tm );

			// ******
			if( RecognizedCharType.TokenStart == charType ) {
				Skip( input, tm );
				return tm.Token;
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetMacroName( IInput input )
		{
			// ******
			TokenMap tm;
			return GetMacroName( input, out tm );
		}


		/////////////////////////////////////////////////////////////////////////////

		public BaseRecognizer( GrandCentral gc )
		{
			this.gc = gc;
		}

	}



}
