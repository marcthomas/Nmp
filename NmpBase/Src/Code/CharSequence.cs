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

namespace NmpBase {

	public delegate bool ConfirmStopSequence( IParseReader input );


//need to dupe routines to handle strings as well

	//
	// should be able to optimize some of the comparison routines by
	// checking for first character matching and checking first char
	// only when the sequence is only one char in length


	/////////////////////////////////////////////////////////////////////////////

	public class CharSequence {
		
		// ******
		public	string	Sequence = string.Empty;
		public	char		FirstChar = SC.NO_CHAR;
		public	int			CountChars = 0;

		// ******
		//public	ConfirmStopSequence	ConfirmProc = null;


		// ******
		public static CharSequence CloseParenStop	= new CharSequence( SC.CLOSE_PAREN_STR );
		public static CharSequence CommaStop			= new CharSequence( SC.COMMA_STR );


		/////////////////////////////////////////////////////////////////////////////

		string _seqRemainder;

		public string SequenceRemainder
		{
			// ******
			get {
				if( null == _seqRemainder ) {
					_seqRemainder = Sequence.Substring( 1 );
				}

				// ******
				return _seqRemainder;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool MatchFirstChar( char ch )
		{
			return FirstChar == ch;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Starts( string str )
		{
			return str.StartsWith( Sequence );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Ends( string str )
		{
			return str.EndsWith( Sequence );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Starts( IParseReader input )
		{
			return input.StartsWith( Sequence );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Starts( StringIndexer input )
		{
			return input.StartsWith( Sequence );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Matches( IParseReader input, int startIndex )
		{
			return input.Matches( startIndex, Sequence );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Skip( string str )
		{
			// ******
			if( FirstChar == str[0] && str.StartsWith(Sequence) ) {
				return str.Substring( Sequence.Length );
			}
			else {
				throw new Exception( string.Format("string buffer does not begin with \"{1}\"", Sequence) );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Skip( IParseReader input )
		{
			// ******
			if( FirstChar == input.Peek() && input.StartsWith(Sequence) ) {
				input.Skip( Sequence.Length );
			}
			else {
				throw new Exception( string.Format("input buffer does not begin with \"{1}\"", Sequence) );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Skip( StringIndexer input )
		{
			// ******
			if( FirstChar == input.Peek() && input.StartsWith(Sequence) ) {
				input.Skip( Sequence.Length );
			}
			else {
				throw new Exception( string.Format("input buffer does not begin with \"{1}\"", Sequence) );
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public string SkipRemainder( string str )
		//{
		//	// ******
		//	if( CountChars < 2 ) {
		//		//
		//		// only one char and we that was already skipped
		//		//
		//		return str;
		//	}
		//
		//	// ******
		//	if( Sequence[1] == str[0] && str.StartsWith(SequenceRemainder) ) {
		//		return str.Substring( Sequence.Length - 1 );
		//	}
		//	else {
		//		throw new Exception( string.Format("input buffer does not begin with \"{1}\"", Sequence) );
		//	}
		//}
		//

		///////////////////////////////////////////////////////////////////////////////
		//
		//public void SkipRemainder( IParseReader input )
		//{
		//	// ******
		//	if( CountChars < 2 ) {
		//		//
		//		// only one char and we that was already skipped
		//		//
		//		return;
		//	}
		//
		//	// ******
		//	if( Sequence[1] == input.Peek() && input.StartsWith(SequenceRemainder) ) {
		//		input.Skip( Sequence.Length - 1 );
		//	}
		//	else {
		//		throw new Exception( string.Format("input buffer does not begin with \"{1}\"", Sequence) );
		//	}
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public CharSequence()
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public CharSequence( char ch )
		{
			// ******
			Sequence = new string( ch, 1 );
			FirstChar = ch;
			CountChars = 1;
		}


		/////////////////////////////////////////////////////////////////////////////

		public CharSequence( string seq )
		{
			// ******
			if( string.IsNullOrEmpty(seq) ) {
				throw new ArgumentNullException( "seq" );
			}
			
			// ******
			Sequence = seq;
			FirstChar = seq[ 0 ];
			CountChars = seq.Length;
		}


	}




}
