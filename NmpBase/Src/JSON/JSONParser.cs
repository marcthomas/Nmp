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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;


#pragma warning disable 414


/*

http://tools.ietf.org/html/rfc4627

http://www.json.org/example.html

http://www.bing.com/search?q=JSON+examples&form=QBLH&qs=n&sk=&sc=4-13

http://weblogs.asp.net/bleroy/archive/2008/01/18/dates-and-json.aspx

*/


namespace NmpBase {


	public enum JSONDateFormat {
		At1970,
		Date1970,
		ISODate,
	}


	enum JSON_ParseFlags {
		Strict								= 0,
		AllowDirtyIdentifiers	= 1,
		AllowDateValues				= 2,
	}


	/////////////////////////////////////////////////////////////////////////////

	public class JSONParser {

		/*

			o		we allow leading zeros in numbers

		*/

		// ******
		private enum NUM_PART { 
			Minus = 1, 
			Integer, 
			Fractional, 
			Exponent
		}

		// ******
		private ParseStringReader	reader;
		JSONItemHandler		handler;

		// ******
		//
		// usefull when debugging and want to see the last items successfully
		// parsed
		//
		public bool		ValidationRun			{ get; private set; }
		public string	LastIdentifier		{ get; private set; }
		public object	LastValue					{ get; private set; }


		/////////////////////////////////////////////////////////////////////////////
		/// eats white space but does not eat first non white space char

		protected char PeekNotWhiteSpace()
		{
			while( true ) {
				char ch = reader.Peek();
				if( ! char.IsWhiteSpace(ch) ) {
					return ch;
				}
				//
				// eat white space
				//
				reader.Next();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected char NextNotWhiteSpace()
		{
			while( true ) {
				char ch = reader.Next();
				if( ! char.IsWhiteSpace(ch) ) {
					return ch;
				}
			}
		}


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
			if( IsHexString(hexStr) ) {
				try {
					int value = Int32.Parse( hexStr, NumberStyles.HexNumber );

					// ******
					string result = char.ConvertFromUtf32( value );
					return 1 == result.Length ? result[ 0 ] : SC.NO_CHAR;
				}
				catch {
				}
			}

			// ******
			throw new NmpJSONException( this, "while parsing unicode escape in JSON string: expecting 4 hex digits, found: \"{0}\"", hexStr );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string GetQuotedValue( char quoteChar )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			while( true ) {
				char ch = reader.Next();

				if( SC.NO_CHAR == ch ) {
					throw new NmpJSONException( this, "out of input parsing quoted JSON identifier" );
				}
				
				else if( SC.BACKSLASH == ch ) {
					ch = reader.Next();

					switch( ch ) {
						case SC.DOUBLE_QUOTE:
						case SC.BACKSLASH:
							//
							// as is
							//
							break;

						case 'b':
							ch = '\b';
							break;

						case 'f':
							ch = '\f';
							break;

						case 'n':
							ch = '\n';
							break;

						case 'r':
							ch = SC.NO_CHAR;
							break;

						case 't':
							ch = '\t';
							break;

						case 'u':
							ch = EscapedUnicodeValue( reader.Next(4) );
							break;
					}

					// ******
					if( SC.NO_CHAR != ch ) {
						sb.Append( ch );
					}
				}

				else if( quoteChar == ch ) {
					return sb.ToString();
				}

				else {
					sb.Append( ch );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string GetQuotedIdentifier( char quoteChar )
		{
			return GetQuotedValue( quoteChar );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string GetValueString( char firstChar )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			sb.Append( firstChar );

			// ******
			while( true ) {
				char ch = reader.Peek();
				
				switch( ch ) {
					case SC.COLON:
					case SC.COMMA:
					case SC.CLOSE_OBJECT:
					case SC.CLOSE_ARRAY:
						return sb.ToString();

					default:
						if( char.IsWhiteSpace(ch) || SC.NO_CHAR == ch ) {
							return sb.ToString();
						}

						sb.Append( reader.Next() );
						break;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string GetDirtyIdentifier( char firstChar )
		{
			return GetValueString( firstChar );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void Identifier()
		{
			// ******
			string identifier;
			char ch = NextNotWhiteSpace();

			// ******
			switch( ch ) {
				case SC.CLOSE_OBJECT:
				case SC.CLOSE_ARRAY:
				case SC.COMMA:
				case SC.COLON:
					throw new NmpJSONException( this, "unexpected first character while parsing JSON identifier: {0}", ch );

				case SC.NO_CHAR:
					throw new NmpJSONException( this, "out of input parsing JSON identifier" );

				case SC.DOUBLE_QUOTE:
					identifier = GetQuotedIdentifier( ch );
					break;

				default:
					identifier = GetDirtyIdentifier( ch );
					break;
			}

			// ******
			if( string.IsNullOrEmpty(identifier) ) {
				throw new NmpJSONException( this, "zero length JSON identifier" );
			}

			// ******
			handler.Identifier( this, identifier );
			LastIdentifier = identifier;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void Value()
		{
			// ******
			object value = null;

			// ******
			char ch = NextNotWhiteSpace();

			switch( ch ) {
				case SC.NO_CHAR:
					throw new NmpJSONException( this, "out of input parsing JSON value" );

				case SC.DOUBLE_QUOTE:
					//value = CheckForStringObject( GetQuotedValue(ch) );
					//
					// date/object or string
					//
					value = JSONValue.DecodeDateOrObject( GetQuotedValue(ch) );
					break;
			
				case SC.OPEN_OBJECT:
					ParseObject();
					return;
			
				case SC.OPEN_ARRAY:
					ParseArray();
					return;
			
				default:
					if( 't' == ch && "rue" == reader.PeekNext(3) ) {
						reader.Skip( 3 );
						value = true;
					}
					
					else if( 'f' == ch && "alse" == reader.PeekNext(4) ) {
						reader.Skip( 4 );
						value = false;
					}
					
					else if( 'n' == ch && "ull" == reader.PeekNext(3) ) {
						reader.Skip( 3 );
						value = null;
					}
					
					else {
						//
						// get a bunch of chars
						//
						string valueStr = GetValueString( ch );
						//value = ConvertToNumber( valueStr );
						string errStr;
						double? d = JSONValue.ConvertToNumber( valueStr, out errStr );
						if(d.HasValue ) {
							value = d.Value;
						}
						else {
							throw new NmpJSONException( this, errStr );
						}
					}
					break;
			}

			// ******
			handler.Value( this, value );
			LastValue = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void KeyValuePair()
		{
			// ******
			while( true ) {
				char ch = PeekNotWhiteSpace();

				if( SC.CLOSE_OBJECT == ch || SC.COMMA == ch ) {
					return;
				}
				else if( SC.NO_CHAR == ch ) {
					throw new NmpJSONException( this, "out of input parsing JSON Object key/value pair" );
				}
				else if( '"' != ch && ! ParseHelpers.IsStdTokenStartChar(ch) ) {
					throw new NmpJSONException( this, "parsing JSON Object identifier, expecting '\"' or start of identifer" );
				}

				// ******
				Identifier();

				ch = NextNotWhiteSpace();
				if( SC.COLON != ch ) {
					throw new NmpJSONException( this, "unexpected character while parsing JSON key value pair: expected ':', found: {0}", ch );
				}

				Value();

				// ******
				return;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void ParseArray()
		{
			// ******
			handler.EnterArray( this );

			// ******
			while( true ) {
				char ch = PeekNotWhiteSpace();

				if( SC.CLOSE_ARRAY == ch ) {
					reader.Next();
					handler.ExitArray( this );
					return;
				}
				else if( SC.COMMA == ch ) {
					reader.Next();
					handler.Warning( "empty array entry" );
				}
				else if( SC.NO_CHAR == ch ) {
					throw new NmpJSONException( this, "out of input parsing JSON array" );
				}
				else {
					Value();

					// ******
					ch = NextNotWhiteSpace();
					
					switch( ch ) {
						case SC.CLOSE_ARRAY:
							handler.ExitArray( this );
							return;
					
						case SC.COMMA:
							//
							// ok - allows empty, and empty trailing entries
							//
							break;
					
						default:
							throw new NmpJSONException( this, "unexpected character while parsing JSON array, expected ',' or ']', found: {0}", ch );
					}
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void ParseObject()
		{
			// ******
			handler.EnterObject( this );

			// ******
			while( true ) {
				char ch = PeekNotWhiteSpace();

				if( SC.CLOSE_OBJECT == ch ) {
					reader.Next();
					handler.ExitObject( this );
					return;
				}
				else if( SC.COMMA == ch ) {
					reader.Next();
					handler.Warning( "empty object entry" );
				}
				else if( SC.NO_CHAR == ch ) {
					throw new NmpJSONException( this, "out of input parsing JSON object" );
				}
				else {
					KeyValuePair();

					// ******
					ch = NextNotWhiteSpace();

					switch( ch ) {
						case SC.CLOSE_OBJECT:
							handler.ExitObject( this );
							return;
					
						case SC.COMMA:
							//
							// ok - allows empty, and empty trailing entries
							//
							break;

						default:
							throw new NmpJSONException( this, "unexpected character while parsing JSON object, expected ',' or '}}', found: {0}", ch );
					}
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected bool Parse( bool validateOnly )
		{
			// ******
			reader.Reset();
			ValidationRun = validateOnly;

			// ******
			//try {
				
				while( true ) {
					char ch = reader.Next();

					if( SC.NO_CHAR == ch ) {
						throw new NmpJSONException( this, "out of input parsing JSON text, expecting '{{' or '['" );
					}

					if( char.IsWhiteSpace(ch) ) {
						continue;
					}

					// ******
					switch( ch ) {
						case SC.OPEN_OBJECT:
							ParseObject();
							return true;

						case SC.OPEN_ARRAY:
							ParseArray();
							return true;

						default:
							throw new NmpJSONException( this, "expecting '{{' or '[', found {0}", ch );
					}
				}
			
			//}
			//catch ( JSONBail ) {
			//	return false;
			//}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Parse()
		{
			return Parse( false );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Validate()
		{
			return Parse( true );
		}


		/////////////////////////////////////////////////////////////////////////////

		public JSONParser( JSONItemHandler handler, string text )
		{
			// ******
			this.handler = handler;
			reader = new ParseStringReader( text, "json text" );

			// ******
			ValidationRun = false;
			LastIdentifier = string.Empty;
			LastValue = null;
		}


	}




}
