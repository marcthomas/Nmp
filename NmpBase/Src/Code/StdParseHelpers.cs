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
	/// 
	/// </summary>

	public static partial class ParseHelpers {


		/////////////////////////////////////////////////////////////////////////////

		public static bool IsStdTokenStartChar( char ch )
		{
			return char.IsLetter(ch) || '_' == ch;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static int _SkipWhiteSpace( IParseReader input )
		{
			// ******
			int count = 0;
			while( char.IsWhiteSpace(input.Peek()) ) {
				input.Skip( 1 );
				++count;
			}

			// ******
			return count;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool IsValidStdIdentifier( string name, bool allowDots = false )
		{
			// ******
			if( string.IsNullOrEmpty(name) ) {
				return false;
			}
		
			// ******
			for( int i = 0, len = name.Length; i < len; i++ ) {
				char ch = name[ i ];
		
				if( 0 == i && ! char.IsLetter(ch) && '_' != ch ) {
					return false;
				}
				else if( ! char.IsLetter(ch) && !char.IsDigit(ch) && '_' != ch ) {
					if( allowDots && '.' == ch ) {
						//
						// allowed
						//
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
		
		public static string _PeekToken( IParseReader input, int startIndex )
		{
			// ******
			StringBuilder sb = new StringBuilder();
		
			// ******
			for( int i = startIndex; ; i++ ) {
				char ch = input.Peek( i );

				if( char.IsLetterOrDigit(ch) || '_' == ch ) {
					sb.Append( ch );
				}
				else {
					break;
				}
			}

			// ******
			return sb.ToString();
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public static string _GetToken( IParseReader input )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			while( true ) {
				char ch = input.Peek();

				if( char.IsLetterOrDigit(ch) || '_' == ch ) {
					sb.Append( input.Next() );
				}
				else {
					return sb.ToString();
				}
			}
		}


	}
	
}
