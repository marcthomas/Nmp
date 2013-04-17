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
	/// Wraps a string that we can index as we move from character to character,
	/// used as input for simple string parsing
	/// </summary>

	[DebuggerDisplay("remainder: {Remainder}")]

	public class StringIndexer {

		// ******
		public const char	EOS = SC.NO_CHAR;	//'\uE000';	// first private unicode char

		// ******
		string theStr = string.Empty;
		int length = 0;
		int index = 0;


		/////////////////////////////////////////////////////////////////////////////

		public string	TheString	{ get { return theStr; } }

		public int	Length		{ get { return length; } }
		public bool	Empty			{ get { return index >= Length; } }
		public int	Index			{ get { return index; } }

		public int		RemainderCount	{ get { return Length - index; } }
		public string	Remainder				{ get { return theStr.Substring(index); } }

		public bool AtEnd			{ get { return Empty; } }

		/////////////////////////////////////////////////////////////////////////////
		
		public char this [ int reqIndex ]
		{
			get {
				if( reqIndex < 0 || reqIndex >= theStr.Length ) {
					throw new ArgumentOutOfRangeException("reqIndex");
				}
				return theStr[ reqIndex ];
			}
		}


//		/////////////////////////////////////////////////////////////////////////////
//
//		public bool StartsWith( string cmpStr )
//		{
//			return Remainder.StartsWith( cmpStr );
//		}
//
//
//		/////////////////////////////////////////////////////////////////////////////
//
//		public bool StartsWith( string cmpStr, StringComparison strComparison )
//		{
//			return Remainder.StartsWith( cmpStr, strComparison );
//		}
//

		/////////////////////////////////////////////////////////////////////////////

		public bool StartsWith( string cmpStr )
		{
			// ******
			int cmpStrLen = cmpStr.Length;
			if( cmpStrLen > RemainderCount ) {
				return false;
			}

			// ******
			for( int i = 0; i < cmpStrLen; i++ ) {
				if( cmpStr[i] != theStr[ index + i ] ) {
					return false;
				}
			}

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool StartsWith( string cmpStr, bool ignoreCase )
		{
			// ******
			if( ! ignoreCase ) {
				return StartsWith( cmpStr );
			}

			// ******
			int cmpStrLen = cmpStr.Length;
			if( cmpStrLen > RemainderCount ) {
				return false;
			}

			// ******
			for( int i = 0; i < cmpStrLen; i++ ) {
				if( char.ToUpperInvariant(cmpStr[i]) != char.ToUpperInvariant(theStr[ index + i ]) ) {
					return false;
				}
			}

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Skip( int nChars )
		{
			// ******
			index += nChars;
			
			if( index >= Length ) {
				index = Length;
				//
				// is empty
				//
				return true;
			}
			else {
				//
				// NOT empty;
				//
				return false;
			}
		}

			
		/////////////////////////////////////////////////////////////////////////////

		public void Skip( Predicate<char> cmp )
		{
			while( cmp(Peek()) ) {
				NextChar();
			}
		}
				

		/////////////////////////////////////////////////////////////////////////////

		public void Reset()
		{
			index = 0;
		}


		/////////////////////////////////////////////////////////////////////////////

		public char Peek()
		{
			return index < 0 || index >= length ? EOS : theStr[ index ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public char NextChar()
		{
			char ch = Peek();
			if( ch != EOS ) {
				++index;
			}
			return ch;
		}


		///////////////////////////////////////////////////////////////////////////

		public string NextChars( int count )
		{
			// ******
			if( 0 == count ) {
				return string.Empty;
			}

			// ******
			var sb = new StringBuilder();
			while( count-- > 0 ) {
				sb.Append( NextChar() );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public StringIndexer( string s )
		{
			theStr = string.IsNullOrEmpty(s) ? string.Empty : s;
			length = theStr.Length;
		}
	}



}
