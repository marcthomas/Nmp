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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Globalization;
using System.IO;
using System.Reflection;


using NmpBase;
using Nmp;


namespace NmpExpressions {

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// 	NMP Object helper for strings.
	/// </summary>
	///
	/// <remarks>
	/// 	Jpm, 3/26/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////

	class StringObjectHelper {

		string theString;


		const int MaxParseItems = 1024;
				

		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string [] SplitHelper( string macroName, string splitOrRegex, bool usingRegex )
		{
			// ******
			//NmpStringList list = null;
			string [] list = null;

			if( usingRegex ) {
				try {
					Regex rx = new Regex( splitOrRegex );
					//list = new NmpStringList( rx.Split(theString, MaxParseItems) );
					list = rx.Split(theString, MaxParseItems );
				}
				catch ( ArgumentException e ) {
					ThreadContext.MacroError( "{0}: {1}", macroName, e.Message );
				}
			}
			else {
				try {
					//list = new NmpStringList( rx.Split(theString, MaxParseItems) );
					list = theString.Split(new string [] { splitOrRegex }, MaxParseItems, StringSplitOptions.None );
				}
				catch ( ArgumentException e ) {
					ThreadContext.MacroError( "{0}: {1}", macroName, e.Message );
				}
			}

			// ******
			return list;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Splits a string with a regular expression </summary>
		///
		/// <remarks>	Jpm, 3/26/2011. </remarks>
		///
		/// <param name="regExStr">	The regular expression string. </param>
		///
		/// <returns>	. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string [] SplitEx( string regExStr )
		{
			return SplitHelper( "SplitEx", regExStr, true );
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Splits a string using another string as the delimiter. </summary>
		///
		/// <remarks>	Jpm, 3/26/2011. </remarks>
		///
		/// <param name="splitStr">	The split string. </param>
		///
		/// <returns>	. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string [] Split( string splitStr )
		{
			return SplitHelper( "Split", splitStr, false );
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Match a portion of the string with a regular expression </summary>
		///
		/// <remarks>	Jpm, 3/26/2011. </remarks>
		///
		/// <param name="regExStr">	The regular expression string. </param>
		///
		/// <returns>	. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string MatchEx( string regExStr )
		{
			//
			// match the first occurance of a regular expression and return it,
			// or return the first subexpression (if there is one)
			//
			
			// ******
			try {
				Regex rx = new Regex( regExStr );
				Match match = rx.Match( theString );
				if( match.Success ) {
					/*
							group[0] represents the overall capture
							
							group[1] .. group[n]	represent sub expresion captures from the outermost to the inner
																		most "()" pair
					*/
				
					if( match.Groups.Count > 1 ) {
						//
						// if there are subexpressions then we return the first one
						//
						return match.Groups[1].Value;
					}
					else {
						return match.Value;
					}
				}
			}
			catch ( ArgumentException ex ) {
				ThreadContext.MacroError( "while executing MatchEx(): {0}", ex.Message );
			}
			
			// ******
			return string.Empty;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Extracts the subexpressions described by regExStr. </summary>
		///
		/// <remarks>	Jpm, 3/26/2011. </remarks>
		///
		/// <param name="regExStr">	The regular expression string. </param>
		///
		/// <returns>	. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		[Macro]
		public NmpArray ExtractSubexpressions( string regExStr )
		{
			return ExtractSubexpressions( regExStr, MaxParseItems );
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Extracts the subexpressions described by regExStr, limiting the
		/// 					number of items returned. </summary>
		///
		/// <remarks>	Jpm, 3/26/2011. </remarks>
		///
		/// <exception cref="ArgumentException">	
		/// 	Thrown when one or more arguments have unsupported or
		/// 	illegal values.
		/// </exception>
		///
		/// <param name="regExStr">	The regular expression string. </param>
		/// <param name="maxItems">	The maximum items. </param>
		///
		/// <returns>
		///		An NmpArray of subexpressions where the key is the string representation
		///		of the index of the item in the array ("0", "1", etc.) and the value of
		///		the item is another NmpArray that holds the matched subexpressions for
		///		the overall match (of item "0", item "1" etc.). Within the array that
		///		holds the matches the first item is alway named "match" and it contains
		///		the text of the overall regular expression match. There is one additional
		///		entry for each subexpression in the regular expression, if the subexpression
		///		has a name associated with it as in &lt;?&lt;name&gt;subExpression&gt; then the name
		///		will be the key to query for the matched value, otherwise the key will will
		///		the string value of the index of the subexpression. These string values
		///		start a "1" for the first subexpression, "2" for the second and so on. If
		///		a subexpression did not match any text then it will be empty.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		[Macro]
		public NmpArray ExtractSubexpressions( string regExStr, int maxItems )
		{
			// ******
			NmpArray array = new NmpArray();

			try {
				Regex rx = new Regex( regExStr );
				MatchCollection mc = rx.Matches( theString );
				
				//
				// find all matches for a regular expression and for each one return an
				// array of all subexpression whether captured or no, subexpressions that
				// are not captured (such as alterations that don't match) will be returned
				// as the empty string
				//
				// so either return a List<string []> or 
				//
				
				string [] groupNames = rx.GetGroupNames();
				int [] groupIndexes = rx.GetGroupNumbers();

				int matchIndex = 0;
				foreach( Match match in mc ) {
					NmpArray subExpressions = new NmpArray();
					
					for( int iGroup = 0; iGroup < match.Groups.Count; iGroup++ ) {
						string key;
						
						if( 0 == iGroup ) {
							key = "match";
						}
						else {
							if( string.IsNullOrEmpty(groupNames[iGroup]) ) {
								continue;
							}
							key = groupNames[ iGroup ];
						}
						
						// ******
						subExpressions.Add( key, match.Groups[ iGroup ].Value );
					}
					
					//
					// match.Value may not be unique
					//
					array.AddArray( (matchIndex++).ToString(), subExpressions );
					
					//
					// note: we are NOT inhibiting Matches from generating as many matches as it
					// can, only limiting the number we return
					//

					if( 0 == --maxItems ) {
						break;
					}
				}
			}
			catch ( ArgumentException ex ) {
				throw ex;
			}
			
			// ******
			return array;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Extracts the given regular expression as many times as it exists
		/// 					int he input string. </summary>
		///
		/// <remarks>	Jpm, 3/26/2011. </remarks>
		///
		/// <param name="regExStr">	The regular expression string. </param>
		///
		/// <returns>	An array of extractes strings </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string [] Extract( string regExStr )
		{
			return Extract( regExStr, MaxParseItems );
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Extracts the given regular expression up to 'maxItem' times. </summary>
		///
		/// <remarks>	Jpm, 3/26/2011. </remarks>
		///
		/// <param name="regExStr">	The regular expression string. </param>
		/// <param name="maxItems">	The maximum items. </param>
		///
		/// <returns>	An array of extracted strings </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string [] Extract( string regExStr, int maxItems )
		{
			// ******
			NmpStringList list = new NmpStringList();

			try {
				Regex rx = new Regex( regExStr );
				MatchCollection mc = rx.Matches( theString );
				
				//
				// find all the matches for a regular expression and return the match or
				// if there is at least one subexpression return the subexpression
				//

				foreach( Match match in mc ) {
					if( match.Groups.Count > 1 ) {
						//
						// if there are subexpressions then we return the first one (outer most)
						//
						list.Add( match.Groups[1].Value );
					}
					else {
						list.Add( match.Value );
					}
					
					//
					// note: we are NOT inhibiting Matches from generating as many matches as it
					// can, only limiting the number we return
					//

					if( 0 == --maxItems ) {
						break;
					}
				}
			}
			catch ( ArgumentException ex ) {
				ThreadContext.MacroError( "while executing Extract(): {0}", ex.Message );
			}
			
			// ******
			return list.ToArray();
		}


		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Replace substrings in the string using a regular expression where each
		/// 	subexpression is replaced by one of the arguments passed in 'args'. All
		/// 	regular matches in the string are replaced.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// 	If you wish to match a subexpression but do not with to replace it
		/// 	then use "/nr" as the replacement value and the subexpression will
		/// 	not be replaced.
		/// 	
		/// 	If only a single replacment value is passed to ReplaceEx() then it 
		/// 	will be used to replace all subexpressions, if you only want to replace
		/// 	the first subexpression then use "/nr" for the remaining matches.
		/// 	
		/// 	Trailing subexpressions that do not have a matching replacement value
		/// 	are not replaced.
		/// </remarks>
		///
		/// <param name="regExStr">
		/// 	The regular expression string.
		/// </param>
		/// <param name="args">
		/// 	The replacement values.
		/// </param>
		///
		/// <returns>
		/// 	The resulting string.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string ReplaceEx( string regExStr, params object [] args )
		{
			const string NO_REPLACE = "/nr";
			// ******
			//
			// replace escapes in replacement string
			//
			string [] multiReplace = null;
			
			if( args.Length > 0 ) {
				multiReplace = new string [ args.Length ];
				for( int i = 0; i < args.Length; i++ ) {
					multiReplace[i] = StringExtensions.TranslateEscapes( args[i].ToString() );
				}
			}
			else {
				multiReplace = new string [ 1 ];
				multiReplace[0] = string.Empty;
			}

			// ******
			/*
					regular expression 	->		Match
						
					Match									represents a regx match
						Groups
							Group							represents the overall match (Group[0]) and all subexpressions
								Capture
						
					foreach match
						
						as we iterate through the groups, if the group has a capture
						then the "index of the group" minus 1 ([index-1]) is the index of the 
						subexpression that has been captured
						
							keep in mind that there may be multiple captures with the lower index
							ones being the more outer captures and the first being the most outer
							capture, the outer most grouping pair of parens
							
								if the entire subexpression is captures (that is, parens around the
								whole regular expression) then the first capture group [1] will be
								the same as the whole capture [0] and the other subexpression will
								start at [2] - unless the outer expression is double parened (( )) ...
								and so on
							
							it is this most outer capture that gets replaced
			*/

			StringBuilder	sb = new StringBuilder();
			Regex rx = new Regex( regExStr );
			int	nextPos = 0;
			
			Match match = rx.Match( theString );
			while( match.Success ) {
				//
				// copy the text that preceeds the match
				//
				sb.Append( theString.Substring(nextPos, match.Index - nextPos) );
				//
				// next position in the string - we need this once matches
				// fail so we can copy the remainder of the string
				//
				nextPos = match.Index + match.Length;

				// ******
				int	nCaptures = match.Groups.Count;

				if( nCaptures > 0 ) {
					if( 1 == multiReplace.Length ) {
						//
						// there is only a single replacment string - the logic is is there is 
						// only one replacment string the user wants the replacement to appy to 
						// all matches, if not, add an empty `' string to the replacement list
						//
						//sb.Append( match.Result( multiReplace[0] ) );
						sb.Append( NO_REPLACE == multiReplace[0] ? match.Value : match.Result(multiReplace[0]) );
					}
					else {
						for( int iGroup = 1; iGroup < nCaptures; iGroup++ ) {
							Group group = match.Groups[ iGroup ];
							
							// ******
							if( group.Captures.Count > 0 ) {
								//
								// first group that had a capture wins
								//
								CaptureCollection cc = group.Captures;
								
								if( cc.Count > 0 ) {

									if( iGroup <= multiReplace.Length ) {
										//sb.Append( match.Result( multiReplace[iGroup - 1] ) );
										sb.Append( NO_REPLACE == multiReplace[iGroup - 1] ? match.Value : match.Result(multiReplace[iGroup - 1]) );
										break;
									}
									
									break;
								}

							}
						}
					}
				}
			
				// ******
				match = match.NextMatch();
			}
			
			// ******
			if( nextPos < theString.Length ) {
				sb.Append( theString.Substring(nextPos) );
			}

			// ******
			return sb.ToString();
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Translates the string by looking up each character in 'lookup' and 
		/// 	replacing it with the character as the same index position in 'replace'.
		/// 	If the character is not found in 'lookup' then the original character
		/// 	is used.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="lookup">
		/// 	The lookup.
		/// </param>
		/// <param name="replace">
		/// 	The replace.
		/// </param>
		///
		/// <returns>
		/// 	The resulting string.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string Translate( string lookup, string replace )
		{
			// ******
			//
			// XPath / XSLT
			//
			//Function: string translate(string, string, string) 
			//
			//The translate function returns the first argument string with
			//occurrences of characters in the second argument string replaced by the
			//character at the corresponding position in the third argument string.
			//For example, translate("bar","abc","ABC") returns the string BAr. If
			//there is a character in the second argument string with no character at
			//a corresponding position in the third argument string (because the
			//second argument string is longer than the third argument string), then
			//occurrences of that character in the first argument string are removed.
			//For example, translate("--aaa--","abc-","ABC") returns "AAA". If a
			//character occurs more than once in the second argument string, then the
			//first occurrence determines the replacement character. If the third
			//argument string is longer than the second argument string, then excess
			//characters are ignored.
			//
			//NOTE: The translate function is not a sufficient solution for case
			//conversion in all languages. A future version of XPath may provide
			//additional functions for case conversion.
			//

			// ******
			StringBuilder sb = new StringBuilder();
			foreach( char ch in theString ) {
				int index = lookup.IndexOf( ch );
				if( index >= 0 ) {
					if( index < replace.Length ) {
						//sb.Append( replace[index] );
						//
						// '\0' gets eliminated
						//
						var replaceChar = replace [ index ];
						if( '\0' != replaceChar ) {
							sb.Append( replace [ index ] );
						}
					}
				}
				else {
					sb.Append( ch );
				}
			}
			
			// ******
			return sb.ToString();
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Reverses the string.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <returns>
		/// 	The resulting string.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string Reverse()
		{
			// ******
			if( 0 == theString.Length ) {
				return theString;
			}
			
			// ******
	    char[] arr = theString.ToCharArray();
	    Array.Reverse(arr);
			return new string(arr);
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// call StringExtension methods
		//
		/////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Translates characters that are escaped in the string into unicode strings.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// 	Escaped characters are:
		/// 			\a \b \t \r \v \f \n \e \xXX \uXXXX \UXXXX
		/// </remarks>
		///
		/// <returns>
		/// 	The resulting string.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string TranslateEscapes()
		{
			return theString.TranslateEscapes();
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Double escapes any escaped characters in the string.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <returns>
		/// 	The resulting string.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string EscapeEscapes()
		{
			return theString.EscapeEscapes();
		}


		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Html encodes the string.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="encodeAngleBrackets">
		/// 	true to encode angle brackets, equal and ampersand characters.
		/// </param>
		///
		/// <returns>
		/// 	.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string HtmlEncode( bool encodeAngleBrackets )
		{
			return theString.HtmlEncode( encodeAngleBrackets );
		}

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Html decodes the string.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <returns>
		/// 	.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string HtmlDecode()
		{
			return theString.HtmlDecode();
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string SetString( char ch, int count )
		{
			if( count <= 0 ) {
				return string.Empty;
			}

			return new string( ch, count );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string SetString( string str, int count )
		{
			if( count <= 0 ) {
				return string.Empty;
			}

			var sb = new StringBuilder();
			for( int i = 0; i < count; i++ ) {
				sb.Append( str );
			}

			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// ctor
		//
		/////////////////////////////////////////////////////////////////////////////

		public StringObjectHelper( object theString )
		{
			// ******
			if( ! typeof(string).Equals(theString.GetType()) ) {
				throw new ArgumentException("expected string argument", "theString");
			}

			// ******
			this.theString = theString as string;
		}


		/////////////////////////////////////////////////////////////////////////////

		// Func<object, object>
		
		public static object Create( object str )
		{
			return new StringObjectHelper( str );
		}




	}


}
