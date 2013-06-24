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
using System.Globalization;
using System.Linq;
using System.Text;


using NmpBase;
//using NmpOutput;


namespace Nmp.Output {


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay("{Contents}")]
	partial class NmpOutput : IOutput {

		protected StringBuilder	sbOutput;
		protected bool					writeOn;
	
		/////////////////////////////////////////////////////////////////////////////
	
		public	int						Count						{ get { return sbOutput.Length; } }
		public	StringBuilder	Contents				{ get { return sbOutput; } }
		public	string				StringContents	{ get { return sbOutput.ToString(); } }
		public	bool					WriteOn					{ get { return writeOn; } set { writeOn = value; } }


		///////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return sbOutput.ToString();
		}
		
	
		/////////////////////////////////////////////////////////////////////////////

		public virtual IOutput WriteChar( char ch )
		{
			// ******
			if( ! WriteOn ) {
				return this;
			}
			
			// ******
			//
			// must convert token back to chars in case we encounter this text again
			// and the quote characters have changed
			//
			//if( SC.EMBED_OPEN_QUOTE == ch ) {
			//	sbOutput.Append( shared.OPEN_QUOTE_STR );
			//}
			//else if( SC.EMBED_CLOSE_QUOTE == ch ) {
			//	sbOutput.Append( shared.CLOSE_QUOTE_STR );
			//}
			//else {
			//	sbOutput.Append( ch );
			//}

			//if( SC.NO_CHAR == ch ) {
			//	Debugger.Break();
			//}

			sbOutput.Append( ch );
			return this;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public virtual IOutput Write( string str )
		{
			// ******
			if( ! WriteOn || string.IsNullOrEmpty(str) ) {
				return this;
			}
			
			// ******
			//
			// must convert token back to chars in case we encounter this text again
			// and the quote characters have changed
			//
			//foreach( char ch in str ) {
			//	if( SC.EMBED_OPEN_QUOTE == ch ) {
			//		sbOutput.Append( shared.OPEN_QUOTE_STR );
			//	}
			//	else if( SC.EMBED_CLOSE_QUOTE == ch ) {
			//		sbOutput.Append( shared.CLOSE_QUOTE_STR );
			//	}
			//	else {
			//		sbOutput.Append( ch );
			//	}
			//}

			//if( SC.NO_CHAR == str[ str.Length - 1] ) {
			//	Debugger.Break();
			//}

			sbOutput.Append( str );
			return this;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public virtual IOutput Append( IOutput rhs )
		{
			sbOutput.Append( rhs.Contents.ToString() );
			return this;
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public void Zero()
		{
			sbOutput.Length = 0;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public NmpOutput()
		{
			sbOutput = new StringBuilder();
			writeOn = true;
		}

	}


	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	partial class MasterOutput : NmpOutput {

		Diversion				current;
		Diversions			diversions;
		DiversionStack	diversionsStack;

		GrandCentral gc;

		// ******
		//NamedTextBlocks blocks;	// = ThreadContext.TextBlocks;
		//EscapedCharList chars;	// = ThreadContext.EscapedChars;


		///////////////////////////////////////////////////////////////////////////////
		//
		// an issue with this is what div replaces the current one if
		// divName references the current one
		//
		//		? top of buffer stack
		//
		//		? default
		//
		//public void Remove( string divName )
		//{
		//	// ******
		//	if( string.IsNullOrEmpty(divName) ) {
		//		//
		//		// empty is the default buffer - can't remove it
		//		//
		//		return;
		//	}
		//
		//	// ******
		//	diversions.Remove( divName );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public void ClearDivert( IEnumerable<string> list )
		{
			// ******
			foreach( string name in list ) {
				Diversion div = diversions.GetExistingDiversion( name );
				if( null != div ) {
					//div.Value.Clear();
					div.Value.Length = 0;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void PushDivert( string divName )
		{
			// ******
			diversionsStack.Push( current );
			current = diversions.GetDiversion( divName );
			sbOutput = current.Value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void PopDivert()
		{
			// ******
			if( diversionsStack.NotEmpty ) {
				current = diversionsStack.Pop();
				sbOutput = current.Value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Divert( string divName )
		{
			// ******
			current = diversions.GetDiversion( divName );
			sbOutput = current.Value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Undivert( IEnumerable<string> list, bool remove )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			foreach( string name in list ) {
				Diversion div = diversions.GetExistingDiversion( name );
				if( null != div ) {
					sb.Append( div.Value );
					if( remove ) {
						if( name == current.Name ) {
							ThreadContext.MacroWarning( "can't remove active diversion: \"{0}\"", name );
						}
						else {
							diversions.Remove( name );
						}
					}
				}
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		string PrependString( StringBuilder sb, int count, string str )
		{
			// ******
			string prependStr = string.Empty;

			switch( str.Trim() ) {
				case "tab":
				case ".tab.":
					prependStr = new string( '\t', count );
					break;

				case "space":
				case "spc":
				case ".spc.":
					prependStr = new string( ' ', count );
					break;

				default:
					prependStr = new StringBuilder( str, count ).ToString();
					break;
			}

			// ******
			return sb.PrependTextInPlace( prependStr ).ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string FetchDivert( string divName, bool remove, int prependCount, string prependStr, bool suppressWarnings = false )
		{
			// ******
			Diversion div = diversions.GetExistingDiversion( divName );
			if( null == div ) {
				if( !suppressWarnings ) {
					ThreadContext.MacroWarning( "diversion \"{0}\" does not exist", divName );
				}
				return string.Empty;
			}

			// ******
			if( remove ) {
				if( divName == current.Name ) {
					if( !suppressWarnings ) {
						ThreadContext.MacroWarning( "can't remove active diversion: \"{0}\"", divName );
					}
				}
				else {
					diversions.Remove( divName );
				}
			}

			// ******
			if( prependCount > 0 && ! string.IsNullOrEmpty(prependStr) ) {
				return PrependString( div.Value, prependCount, prependStr );
			}
			else {
				return div.Value.ToString();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string FetchDivert( string divName, bool remove )
		{
			return FetchDivert( divName, remove, 0, string.Empty );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AddToDivert( string divName, string textToAdd )
		{
			// ******
			Diversion div = diversions.GetDiversion( divName );
			div.Value.Append( textToAdd );
		}









//
//		THIS STUFF IS HERE TO IMPLEMENT ESCAPING CHARACTERS AND SUCH ON THE
//		OUTPUT, NOT THE INPUT WHERE WE HAD IT
//
//		IN FACT SINCE WE'RE GENERATING TEXT FOR OTHERS TO USE IM NOT SURE WE
//		SHOULD BE CONVERTING THE ESCAPED CHARS
//
//		WE SHOULD MAKE THIS OPTIONAL AND NOT ON BY DEFAULT
//

/// !!!!!!!!!!!!!!!!!!!  checkEscapesAndSpecialChars that we used on Reader/ParseReader
///                      HOW DOES/SHOULD THIS APPLY WHEN WE DO IT HERE


//		///////////////////////////////////////////////////////////////////////////
//
//		protected char EscapedCharValue( char ch )
//		{
//			// ******
//			char result = SC.NO_CHAR;
//
//			switch( ch ) {
//				case 'a':
//					result = '\u0007';
//					break;
//					
//				case 'b':
//					result = '\u0008';
//					break;
//					
//				case 't':
//					result = '\u0009';
//					break;
//					
//				case 'r':
//					result = '\u000d';
//					break;
//					
//				case 'v':
//					result = '\u000b';
//					break;
//					
//				case 'f':
//					result = '\u000c';
//					break;
//					
//				case 'n':
//					result = '\u000a';
//					break;
//					
//				case 'e':
//					result = '\u001b';
//					break;
//					
//				case '-':
//					result = '-';
//					break;
//			}
//
//			return result;
//		}
//
//
//		///////////////////////////////////////////////////////////////////////////
//
//		private bool IsHexString( string str )
//		{
//			// ******
//			foreach( char ch in str.ToLower() ) {
//				if( (ch >= 'a' && ch <= 'f') || (ch >= '0' && ch <= '9') ) {
//					//
//					// good
//					//
//				}
//				else {
//					//
//					// bad
//					//
//					return false;
//				}
//			}
//
//			// ******
//			return true;
//		}
//
//
//		///////////////////////////////////////////////////////////////////////////
//
//		protected char EscapedUnicodeValue( string hexStr )
//		{
//			// ******
//			if( IsHexString(hexStr) ) {
//				try {
//					int value = Int32.Parse( hexStr, NumberStyles.HexNumber );
//
//					// ******
//					string result = char.ConvertFromUtf32( value );
//					return 1 == result.Length ? result[ 0 ] : SC.NO_CHAR;
//				}
//				catch {
//				}
//			}
//
//			ThreadContext.MacroError( "expecting 4 hex digits representing an escaped unicode character, found: \"{0}\"", hexStr );
//
//			// ******
//			return SC.NO_CHAR;
//		}
//
//
//		///////////////////////////////////////////////////////////////////////////
//
//		private char HandleBackslash()
//		{
//			// ******
//			char ch = reader.Peek();
//
//			if( 'u' == ch || 'U' == ch ) {
//				reader.Next();
//				string hexStr = reader.Next( 4 );
//				return EscapedUnicodeValue( hexStr );
//			}
//
//			// ******
//			char peekChar = reader.Peek();
//
//			//
//			// if single char open quoting and if it is being escaped
//			// then push it through as an embeded escape
//			//
//
//			if( peekChar == SC.BACKSLASH ) {
//				return reader.Next();
//			}
//			//
//			// moved to FixText() in FileReader.cs
//			//
//			//else if( peekChar == ThreadContext.SeqOpenQuote.FirstChar && 1 == ThreadContext.SeqOpenQuote.CountChars ) {
//			//	//
//			//	// embed open quote
//			//	//
//			//	return ThreadContext.EscapedChars.Add( reader.Next() );
//			//}
//			//else if( peekChar == ThreadContext.SeqCloseQuote.FirstChar && 1 == ThreadContext.SeqCloseQuote.CountChars ) {
//			//	//
//			//	// embed close quote
//			//	//
//			//	return ThreadContext.EscapedChars.Add( reader.Next() );
//			//}
//
//
//
//			// ******
//			ch = EscapedCharValue( peekChar );
//			
//			if( SC.NO_CHAR == ch ) {
//				//
//				// use backslash if we don't recogninize the escape
//				//
//				return SC.BACKSLASH;
//			}
//			else {
//				reader.Next();
//				return ch;
//			}
//		}
//









		///////////////////////////////////////////////////////////////////////////

		private bool IsHexString( string str )
		{
			// ******
			foreach( char ch in str.ToLower() ) {
				if( (ch >= 'a' && ch <= 'f') || (ch >= '0' && ch <= '9') ) {
					//
					// good
					//
				}
				else {
					//
					// bad
					//
					return false;
				}
			}

			// ******
			return true;
		}


		///////////////////////////////////////////////////////////////////////////

		protected char EscapedUnicodeValue( string hexStr )
		{
			// ******
			if( ! IsHexString(hexStr) ) {
				return SC.NO_CHAR;
			}

			// ******
			try {
				int value = Int32.Parse( hexStr, NumberStyles.HexNumber );

				// ******
				string result = char.ConvertFromUtf32( value );
				return 1 == result.Length ? result[ 0 ] : SC.NO_CHAR;
			}
			catch {
				return SC.NO_CHAR;
			}
		}







		/////////////////////////////////////////////////////////////////////////////

		private static void AppendChar( StringBuilder sb, char ch, bool htmlEncode )
		{
			// ******
			if( SC.NEWLINE == ch ) {
				//
				// prepend
				//
				sb.Append( SC.CR );
				//
				// fall through to append NEWLINE
				//
			}
			
			// ******
			if( htmlEncode ) {
				string enc = SC.TryHtmlEncode( ch, true );
				if( string.IsNullOrEmpty(enc) ) {
					sb.Append( ch );
				}
				else {
					sb.Append( enc );
				}
			}
			else {
				sb.Append( ch );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected StringBuilder ProcessText( StringBuilder sbIn, bool isTextBlock )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			int index = 0;

//			bool inTextBlock = false;
			bool htmlEncode = false;

			while( index < sbIn.Length ) {
				int countRemaining = sbIn.Length - index - 1;
				char ch = sbIn[ index ];

				/////////
				//
				// named (text) blocks
				//
				if( NamedTextBlocks.TextBlockStartChar == ch ) {
					string key = sbIn.ToString( index, NamedTextBlocks.NameLength );
					index += NamedTextBlocks.NameLength;

					string text = gc.GetTextBlocks().GetTextBlock( key );
					if( null == text ) {
						throw ExceptionHelpers.CreateException( "could not locate text block key \"{0}\" while final processing text", key );
					}

					// ******
					//
					// for a text block we ONLY prepend SC.CR - we don't make all these other substitions,
					// or should we ?
					//
					// text may have stuff inserted into be before it got wrapped
					//
					//foreach( char c in text ) {
					//	if( SC.NEWLINE == c ) {
					//		sb.Append( SC.CR );
					//	}
					//	sb.Append( c );
					//}
					
					sb.Append( ProcessText(new StringBuilder(text), true) );

					// ******
					continue;
				}

				/////////
				//
				// escape unicode character
				//
				if( SC.OPEN_BRACKET == ch && countRemaining >= 8 ) { // && ! isTextBlock ) {
					//
					// [\uXXXX]
					//
					if( sbIn[1 + index] == '\\'  && sbIn[2 + index] == 'u' && sbIn[7 + index] == SC.CLOSE_BRACKET ) {
						string hex = sbIn.ToString( 3 + index, 4 );
						char tempCh = EscapedUnicodeValue( hex );
						if( SC.NO_CHAR != tempCh ) {
							index += 8;
							
							//sb.Append( tempCh  );
							AppendChar( sb, tempCh, htmlEncode );
							
							continue;
						}
					}
				}




				/////////
				//
				// HtmlEncode
				//
				if( SC.START_HTML_ENCODE_CHAR == ch ) {
					++index;
					htmlEncode = true;
					continue;
				}
				else if( SC.END_HTML_ENCODE_CHAR == ch ) {
					++index;
					htmlEncode = false;
					continue;
				}

				/////////
				//
				// escaped characters
				//
				if( ch >= EscapedCharList.FirstEmbedEscape && ch <= EscapedCharList.LastEmbedEscape ) {
					sb.Append( gc.EscapedChars.Get( ch ) );
					++index;
				}

				//
				// just a character
				//
				else {
					//if( SC.NEWLINE == ch ) {
					//	sb.Append( SC.CR );
					//}
					//
					//// ******
					//++index;
					//
					//if( htmlEncode ) {
					//	string enc = SC.TryHtmlEncode( ch, true );
					//	if( string.IsNullOrEmpty(enc) ) {
					//		sb.Append( ch );
					//	}
					//	else {
					//		sb.Append( enc );
					//	}
					//}
					//else {
					//	sb.Append( ch );
					//}
					++index;
					AppendChar( sb, ch, htmlEncode );
				}
			}

			// ******
			return sb;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// also called by InvokeMacro() with its results
		//

		public StringBuilder FinalProcessText( StringBuilder sbIn, bool removeBlocksAndChars )
		{
			// ******
			StringBuilder sb = ProcessText( sbIn, false );

			// ******
			if( removeBlocksAndChars ) {
				gc.GetTextBlocks().Clear();
				//chars.Clear();
			}

			// ******
			return sb;
		}


		/////////////////////////////////////////////////////////////////////////////

		public StringBuilder AllText
		{
			get {
				StringBuilder text = sbOutput;
				sbOutput = new StringBuilder();

				// ******
				return FinalProcessText( text, true );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public MasterOutput( GrandCentral gc )
		{
			// ******
			this.gc = gc;
			//blocks = gc.GetTextBlocks();
			//chars = gc.GetEscapedChars();

			// ******
			diversions = new Diversions();
			diversionsStack = new DiversionStack();

			// ******
			current = diversions.GetDiversion( Diversion.DEFAULT_DIV_NAME );
			sbOutput = current.Value;
		}

	}

}
