#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// Reader.cs
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using NmpBase;


namespace Nmp.Input {


	///////////////////////////////////////////////////////////////////////////

	class Reader : IReader {

		GrandCentral gc;

		public bool checkEscapesAndSpecialChars = true;

		protected	IBaseReader	reader =	null;


		///////////////////////////////////////////////////////////////////////////

		public string	Buffer
		{
			get {
				reader.Peek();
				return reader.Buffer;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public IBaseReader Source
		{
			get {
				return reader;
			}
		}


		///////////////////////////////////////////////////////////////////////////
//
// not going to be accurate because we can take many chars and reduce to one
//
		public int Index
		{
			get {
				//return reader.Pos - peekBuffer.Count;
				return reader.Pos;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public int RemainderCount
		{
			get {
				//return reader.RemainderCount + peekBuffer.Count;
				return reader.RemainderCount;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public string Remainder
		{
			get {
				//return peekBuffer.PeekAll() + reader.Remainder;
				return reader.Remainder;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public string SourceName
		{
			get {
				return reader.SourceName;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public int Line
		{
			get {
				NmpTuple<int, int> lineAndColumn = LineAndColumn();
				return lineAndColumn.Item1;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public int Column
		{
			get {
				NmpTuple<int, int> lineAndColumn = LineAndColumn();
				return lineAndColumn.Item2;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public string GetText( int start, int end )
		{
			return reader.GetText( start, end );
		}


		///////////////////////////////////////////////////////////////////////////

		public bool CheckEscapesAndSpecialChars
		{
			get {
				return checkEscapesAndSpecialChars;
			}
			
			set {
				checkEscapesAndSpecialChars = value;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public void Pushback( string text )
		{
			// ******
			//
			// first the pushback text since we want to get it back first and then
			// the current contents of the peekBuffer which need to be retrieved
			// AFTER the pushback text
			//
			//reader.Pushback( new string [] {text, peekBuffer.GetAll()} );
			reader.Pushback( text );	//new string [] { text } );
		}


		///////////////////////////////////////////////////////////////////////////

		public NmpTuple<int, int> LineAndColumn()
		{
			// ******
			////
			//// get the number of lines and columns currently in peekBuffer the
			//// subtract that from the line and column count we've extracted from 
			//// reader buffer to get the actual line and column we're at
			////
			//NmpTuple<int, int> pbLinesAndColumns = peekBuffer.LinesAndColumns();
			//return new NmpTuple<int, int>( reader.Line - pbLinesAndColumns.Item1, reader.Column - pbLinesAndColumns.Item2 );

			return new NmpTuple<int, int>( reader.Line, reader.Column );
		}


		///////////////////////////////////////////////////////////////////////////

		private void RemoveMultiLineComment()
		{
			while( true ) {
				char ch = reader.Next();
			
				if( SC.NO_CHAR == ch ) {
					return;
				}

				if( SC.STAR == ch && SC.POUND == reader.Peek() && SC.CLOSE_PAREN == reader.Peek(1) ) {
					reader.Next();	// eat '#'
					reader.Next();	// eat ')'
					return;
				}
			}
		}


//		///////////////////////////////////////////////////////////////////////////
//
//		//
//		// returns number of chars spaned by the comment - not including
//		// the first 3
//		//
////
//// need to use with NextChar() as well
////
//
//		private int PeekMultiLineComment( int index )
//		{
//			// ******
//			int nextIndex = index;
//
//			// ******
//			while( true ) {
//				char ch = reader.Peek( nextIndex++ );
//			
//				if( SC.NO_CHAR == ch ) {
//					return nextIndex - index - 1;
//				}
//
//				if( SC.STAR == ch && SC.POUND == reader.Peek(nextIndex) && SC.CLOSE_PAREN == reader.Peek(1 + nextIndex) ) {
//					return nextIndex + 2;
//				}
//			}
//		}
//
//
//		///////////////////////////////////////////////////////////////////////////
//
//		//
//		// assumes there are no reads between various peeks since the in/out 'index'
//		// is used to position relative to the reader.Pos
//		//
//
////
//// NextChar() needs to reset 'index' for peeking
////
//		char _lastNextChar;
//
//		private char NextValidChar( ref int nextIndex )
//		{
//			// ******
//			//int readerPos = reader.Pos;
//			//int nextIndex = readerPos + index;
//
//			//int nextIndex = index;
//
//			// ******
//			while( true ) {
//				//
//				// if we overrun the end of the buffer 'reader' will just return
//				// SC.NO_CHAR
//				//
//				char ch = reader.Peek( nextIndex++ );
//
//			char nextCh = reader.Peek( nextIndex );
//
//				//
//				// single line comment
//				//
//				if( SC.SEMI_COLON == ch && SC.SEMI_COLON == nextCh ) {
//					//
//					// skips the second comma
//					//
//					++nextIndex;
//
//					while( true ) {
//						ch = reader.Peek( nextIndex++ );
//
//						if( SC.NO_CHAR == ch ) {
//							//
//							// out of data
//							//
//							return SC.NO_CHAR;
//						}
//						else if( SC.NEWLINE == ch ) {
//							//
//							// newline terminates comment
//							//
//							if( SC.NEWLINE == _lastNextChar ) {
//								//
//								// if the character previous to the comment start char was a
//								// newline then we remove all of the line including the
//								// terminating newline at the end of the comment - we break
//								// to get the character that follows the newline
//								// 
//								break;
//							}
//
//							//
//							// otherwise we leave the newline in place
//							//
//							return _lastNextChar = SC.NEWLINE;
//						}
//					}
//				}
//
//				//
//				// multi line comment
//				//
//				else if( SC.OPEN_PAREN == ch && SC.POUND == nextCh && SC.STAR == reader.Peek(1 + nextIndex) ) {
//					nextIndex += 2;
//					nextIndex += PeekMultiLineComment( nextIndex );
//				}
//
//				//
//				// begining of line: starts with a dash, is preceeded by a newline and is followed
//				// by white space
//				//
//				else if( SC.DASH == ch && SC.NEWLINE == _lastNextChar && char.IsWhiteSpace(nextCh) && checkEscapesAndSpecialChars ) {
//					//
//					// eat whitespace, process next char
//					//
//					while( char.IsWhiteSpace(reader.Peek(nextIndex)) ) {
//						++nextIndex;
//					}
//				}
//
//				else if( SC.DASH == ch && SC.NEWLINE == nextCh && checkEscapesAndSpecialChars ) {
//					//
//					// eat newline, process next char
//					//
//					++nextIndex;
//				}
//
//
//				else {
//					return _lastNextChar = ch;
//				}
//
//			}
//		}
//
//
//		int peekNextPos = 0;

		///////////////////////////////////////////////////////////////////////////

		void TextBlockLine()
		{

			//
			// optimize: remove consecutive lines
			//

			// ******
			var sb = new StringBuilder();

			while( true ) {
				var ch = reader.Next();

				if( SC.NO_CHAR == ch ) {
					//
					// out of data
					//
					break;
				}
				else if( SC.NEWLINE == ch ) {
					//
					// newline terminates
					//
					sb.Append( ch );
					break;
				}
				sb.Append( ch );
			}

			if( sb.Length > 0 ) {
				NamedTextBlocks blocks = gc.GetTextBlocks();
				var text = blocks.AddTextBlock( sb.ToString() );
				reader.Pushback( text );

			}
		}


		///////////////////////////////////////////////////////////////////////////

		//
		// instead of NO_CHAR set this to NEWLINE so that comments at the top of the 
		// file are cleanly removed
		//
		//char lastNextChar = SC.NO_CHAR;
		char lastNextChar = SC.NEWLINE;

		private char NextChar()
		{
			// ******
			while( true ) {
				char ch = reader.Next();

				//
				// single line comment
				//
				if( SC.SEMI_COLON == ch && SC.SEMI_COLON == reader.Peek() ) {
					while( true ) {
						ch = reader.Next();

						if( SC.NO_CHAR == ch ) {
							//
							// out of data
							//
							return SC.NO_CHAR;
						}
						else if( SC.NEWLINE == ch ) {
							//
							// newline terminates comment
							//
							if( SC.NEWLINE == lastNextChar ) {
								//
								// if the character previous to the comment start char was a
								// newline then we remove all of the line including the
								// terminating newline at the end of the comment - we break
								// to get the character that follows the newline
								// 
								break;
							}

							//
							// otherwise we leave the newline in place
							//
							return lastNextChar = SC.NEWLINE;
						}
					}
				}

				else if( SC.SEMI_COLON == ch && SC.COLON == reader.Peek() ) {
					reader.Next();
					TextBlockLine();
				}

				//
				// multi line comment
				//
				else if( SC.OPEN_PAREN == ch && SC.POUND == reader.Peek() && SC.STAR == reader.Peek( 1 ) ) {
					RemoveMultiLineComment();
				}

				//
				// begining of line: starts with a dash, is preceeded by a newline and is followed
				// by white space
				//
				else if( SC.DASH == ch && SC.NEWLINE == lastNextChar && char.IsWhiteSpace( reader.Peek() ) && checkEscapesAndSpecialChars ) {
					//
					// eat whitespace, process next char
					//
					while( char.IsWhiteSpace( reader.Peek() ) ) {
						reader.Next();
					}
				}

				else if( SC.DASH == ch && SC.NEWLINE == reader.Peek() && checkEscapesAndSpecialChars ) {
					//
					// eat newline, process next char
					//
					reader.Next();
					//
					// 15 march 11 - added this to fix issue where "popdivert()-" followed by ";;" left a newline in the output
					//
					//lastNextChar = SC.NEWLINE;
				}

				else {
					return lastNextChar = ch;
				}

				// loop top

			}
		}


		///////////////////////////////////////////////////////////////////////////

		//
		// needs to NOT match reader.Pos on first character looked at in case it's
		// a comment - which is how we found it to be a problem; -1, not 0
		//
		int _lastPeekPos = -1;

		public char PeekChar( int index )
		{
			// ******
			//return reader.Peek( index );

			if( 0 == index ) {
				//
				// when peeking the next char we need to make sure that
				// it's valid - so we use NextChar() and then backup reader.Pos
				// "pushback"
				//

				if( reader.Pos == _lastPeekPos ) {
					return reader.Peek();
				}
				
				// ******
				char ch = NextChar();
				if( SC.NO_CHAR != ch ) {
					_lastPeekPos = reader.BackupOne();
				}

				// ******
				return ch;
			}

			// ******
			return reader.Peek( index );
		}


		///////////////////////////////////////////////////////////////////////////

		public char GetChar()
		{
			return NextChar();
		}


		///////////////////////////////////////////////////////////////////////////

		public void SkipChars( int count )
		{
			//
			// in theory we'd never skip into comments or such because 'skip' should
			// only be used on characters we've already looked at
			//
			while( count > 0 ) {
				GetChar();
				--count;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Matches( int startIndex, string cmpStr )
		{
			//
			// where 'startIndex' is some number of characters into the input, in 
			// practice this is one or two characters
			//

			// ******
			//
			// num chars to compare and were we start in the buffer
			//
			int cmpStrLen = cmpStr.Length;
			int pos = startIndex + reader.Pos;

			//if( pos >= reader.Count || reader.Count - pos < cmpStrLen ) {
			if( reader.Count - pos < cmpStrLen ) {
				//
				// out of bounds or not enough chars to compare
				//
				return false;
			}

			// ******
			for( int i = 0; i < cmpStrLen; i++ ) {
				if( cmpStr[i] != reader[ pos + i ] ) {
					return false;
				}
			}

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool StartsWith( string cmpStr )
		{
			// ******
			int cmpStrLen = cmpStr.Length;
			if( cmpStrLen > reader.RemainderCount ) {
				return false;
			}

			// ******
			int pos = reader.Pos;
			for( int i = 0; i < cmpStrLen; i++ ) {
				if( cmpStr[i] != reader[ pos + i ] ) {
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
			if( cmpStrLen > reader.RemainderCount ) {
				return false;
			}

			// ******
			int pos = reader.Pos;
			for( int i = 0; i < cmpStrLen; i++ ) {
				if( char.ToUpperInvariant(cmpStr[i]) != char.ToUpperInvariant(reader[ pos + i ]) ) {
					return false;
				}
			}

			// ******
			return true;
		}


		///////////////////////////////////////////////////////////////////////////

		public void RegexMatch( IList<Regex> rx )
		{
/*
	
	? make sure peek buffer has been pushed back onto





*/
		}


		///////////////////////////////////////////////////////////////////////////

		//public Reader()
		//{
		//}


		///////////////////////////////////////////////////////////////////////////

		public Reader( GrandCentral gc, IBaseReader reader )
		{
			this.gc = gc;
			this.reader = reader;
		}




	}

}
