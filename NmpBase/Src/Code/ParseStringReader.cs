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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NmpBase;

namespace NmpBase {

	///////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay( "Peek[64]: {DebugPeek}" )]

	public class ParseStringReader : IBaseReader {

		const int EXTRA_BUFFER_SPACE = 512;

		// ******
		protected readonly string	initialText = string.Empty;        
		//protected StringBuilder		txtBuffer = new StringBuilder();
		protected string		_txtBuffer = string.Empty;

		// ******
		int pos = 0;
		int count = 0;

		int line = 1;
		int column = 0;

		int pushbackCount = 0;

		char lastChar = SC.NO_CHAR;


		/////////////////////////////////////////////////////////////////////////////

		public string	Buffer				{ get { return _txtBuffer; } }

		public string SourceName		{ get; private set; }
		public string DefaultPath		{ get; private set; }

		public int PushedbackCount	{ get { return pushbackCount; } }
		public int Count						{ get { return count; } }
		public int Pos							{ get { return pos; } }
		public int RemainderCount		{ get { return Count - Pos; } }
		public string Remainder			{ get { return _txtBuffer.Substring( Pos ); } }

		public bool AtEnd						{ get { return pos >= Count; } }

		public int Line							{ get { return line; } }
		public int Column						{ get { return column; } }

		public char LastChar				{ get { return lastChar; } }


		/////////////////////////////////////////////////////////////////////////////

		public char this [ int reqIndex ]
		{
			get {
				if( reqIndex < 0 ) {
					throw new ArgumentOutOfRangeException( "reqIndex", "index must be greater or equal to zero" );
				}

				// ******
				if( reqIndex >= count ) {
					throw new ArgumentOutOfRangeException( "reqIndex", "index must be less than buffer length" );
				}

				// ******
				return _txtBuffer [ reqIndex ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

//
// 4 March 11: look here if there are issues, trying to sync pushback
// with backing up
//

		public int BackupOne()
		{
			if( pos > 0 ) {
				--pos;
				++pushbackCount;

				if( pushbackCount < 1 ) {
					if( SC.NEWLINE == _txtBuffer[pos] ) {
						--line;
					}
					else {
						--column;
					}
				}
			}

			return pos;
		}


		/////////////////////////////////////////////////////////////////////////////

		public char Peek( int index )
		{
			// ******
			if( index < 0 ) {
				throw new ArgumentOutOfRangeException( "index", "index must be greater or equal to zero" );
			}

			// ******
			if( index + pos >= count ) {
				return SC.NO_CHAR;
			}

			char ch = this [ pos + index ];
			if( '\0' == ch ) {
				//
				// found embeded \u0000 which we use as SC.NO_CHAR
				//
				ch = SC.SPACE;
			}

			// ******
			return ch;
		}


		/////////////////////////////////////////////////////////////////////////////

		public char Peek()
		{
			return Peek( 0 );
		}


		///////////////////////////////////////////////////////////////////////////

		public string PeekNext( int count )
		{
			// ******
			if( 0 == count ) {
				return string.Empty;
			}

			// ******
			var sb = new StringBuilder();
			for( int i = 0; i < count; i++ ) {
				sb.Append( Peek(i) );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public char Next()
		{
			char ch = Peek();

			if( ch != SC.NO_CHAR ) {
				//
				// advance index
				//
				++pos;

				// ******
				if( pushbackCount < 1 ) {
					//
					// only count lines and columns when there is new text to return,
					// don't count for pushbacks
					//
					if( SC.NEWLINE == ch ) {
						++line;
						column = 1;
					}
					else {
						++column;
					}
				}

				// ******
				//
				// always do this, it's important for BackupOne();
				//
				//else {
				//	--pushbackCount;
				//}
				--pushbackCount;

			}

			// ******
			return ch;
		}


		/////////////////////////////////////////////////////////////////////////////

		public char Next( out bool wasPushedBack )
		{
			wasPushedBack = pushbackCount > 0;
			return Next();
		}


		///////////////////////////////////////////////////////////////////////////

		public string Next( int count )
		{
			// ******
			if( 0 == count ) {
				return string.Empty;
			}

			// ******
			var sb = new StringBuilder();
			while( count-- > 0 ) {
				sb.Append( Next() );
			}

			// ******
			return sb.ToString();
		}


		///////////////////////////////////////////////////////////////////////////

		public void Skip( int count )
		{
			Next( count );
		}


		///////////////////////////////////////////////////////////////////////////

		public string GetText( int start, int end )
		{
			// ******
			int length = 0;

			if( end < start ) {
				end = Count - 1;
			}

			length = 1 + (end - start);
			if( length > Count - start ) {
				length = Count - start;
			}

			// ******
			return _txtBuffer.Substring( start, length );
		}


		///////////////////////////////////////////////////////////////////////////

		public void Pushback( string text )
		{
			/*
					since it's the default, pushing back is going to happen A LOT
			*/

			// ******
			if( 0 == text.Length ) {
				return;
			}

			// ******
			_txtBuffer = text + _txtBuffer.Substring( pos );

			// ******
			pos = 0;
			count = _txtBuffer.Length;

			if( pushbackCount > 0 ) {
				pushbackCount += text.Length;
			}
			else {
				pushbackCount = text.Length;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public string DebugPeek
		{
			get
			{
				int count = RemainderCount;
				if( count > 64 ) {
					count = 64;
				}

				// ******
				return _txtBuffer.Substring( pos, count );
			}
		}


		///////////////////////////////////////////////////////////////////////////

		protected void Initialize()
		{
			// ******
			//txtBuffer = new StringBuilder( initialText, EXTRA_BUFFER_SPACE + initialText.Length );
			_txtBuffer = initialText;

			// ******
			pos = 0;
			count = _txtBuffer.Length;
			pushbackCount = 0;
		}


		///////////////////////////////////////////////////////////////////////////

		public void Reset()
		{
			Initialize();
		}


		///////////////////////////////////////////////////////////////////////////

		[DebuggerStepThrough]
		protected ParseStringReader( string sourceName, string text, int start, int numberOfChars )
		{
			// ******
			if( null == text ) {
				throw new ArgumentNullException( "text" );
			}

			// ******
			DefaultPath = string.Empty;
			if( string.IsNullOrEmpty( sourceName ) ) {
				SourceName = string.Empty;
			}
			else {
				SourceName = sourceName;
				if( Path.IsPathRooted(SourceName) ) {
					DefaultPath = Path.GetDirectoryName( Path.GetFullPath( SourceName ) );
				}
			}

			// ******
			if( 0 == start && 0 == numberOfChars ) {
				//
				// zero length
				//
			}
			else {
				if( start < 0 || start >= text.Length ) {
					throw new ArgumentOutOfRangeException( "start" );
				}

				if( numberOfChars > text.Length - start ) {
					throw new ArgumentOutOfRangeException( "numberOfChars" );
				}
			}

			// ******
			initialText = (0 == start && numberOfChars == text.Length) ? text : text.Substring( start, numberOfChars );
			////initialText = FileReader.FixText( new string [] {initialText} );

			// ******
			Initialize();
		}


		///////////////////////////////////////////////////////////////////////////

		[DebuggerStepThrough]
		public ParseStringReader( string sourceName, string str, int start )
			: this( sourceName, str, start, string.IsNullOrEmpty( str ) ? 0 : str.Length - start )
		{
		}

		///////////////////////////////////////////////////////////////////////////

		[DebuggerStepThrough]
		public ParseStringReader( string str, string sourceName )
			: this( sourceName, str, 0, string.IsNullOrEmpty( str ) ? 0 : str.Length )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		[DebuggerStepThrough]
		public ParseStringReader( string str )
			: this( string.Empty, str, 0, string.IsNullOrEmpty( str ) ? 0 : str.Length )
		{
		}

	}
}
