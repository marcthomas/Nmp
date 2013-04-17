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
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;




namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// String extension methods other than the expand methods
	/// </summary>

	public static partial class StringExtensions {

		const string DefaultExtractPattern = "(?s)///BeginExtract(.*?)///EndExtract";


		/////////////////////////////////////////////////////////////////////////////

		public static string Duplicate( this string str, int count )
		{
			var sb = new StringBuilder();
			for( int i = 0; i < count; i++ ) {
				sb.Append( str );
			}

			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string Extract( this string str )
		{
			return Extract( str, string.Empty );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string Extract( this string str, string pattern )
		{
			// ******
			if( string.IsNullOrEmpty(pattern) ) {
				pattern = DefaultExtractPattern;
			}

			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			try {
				Regex rx = new Regex( pattern );
				MatchCollection matches = rx.Matches( str );				

				foreach( Match match in matches ) {
					/*
							group[0] represents the overall capture
							
							group[1] .. group[n]	represent sub expresion captures from the outermost to the inner
																		most "()" pair
					*/
			
					// ******
					string value = string.Empty;
					if( match.Groups.Count > 1 ) {
						//
						// if there are subexpressions then we return the outermost (or first)
						//
						value = match.Groups[1].Value;
					}
					else {
						value = match.Value;
					}

					// ******
					sb.Append( value );
				}
			}
			catch ( ArgumentException e ) {
				throw new Exception( string.Format("StringHelpers.Extract: {0}", e.Message), e );
			}

			// ******
			return sb.ToString();
		}

		
		///////////////////////////////////////////////////////////////////////////////
		//
		//public static bool IsTrue( this string str, bool defValue )
		//{
		//	// ******
		//	if( 0 == string.Compare(str, "true", StringComparison.OrdinalIgnoreCase) ) {
		//		return true;
		//	}
		//	else if( 0 == string.Compare(str, "false", StringComparison.OrdinalIgnoreCase) ) {
		//		return false;
		//	}
		//
		//	// ******
		//	return defValue;
		//}
		//

		///////////////////////////////////////////////////////////////////////////////
		//
		//public static bool IsTruthful( this string str )
		//{
		//	// ******
		//	if( 0 == string.Compare(str, "true", StringComparison.OrdinalIgnoreCase) ) {
		//		return true;
		//	}
		//	else if( 0 == string.Compare(str, "false", StringComparison.OrdinalIgnoreCase) ) {
		//		return false;
		//	}
		//
		//	// ******
		//	Int64 iValue;
		//	if( Int64.TryParse(str, out iValue) ) {
		//		return 0 != iValue;
		//	}
		//
		//	// ******
		//	return false;
		//}
		//
		
		/////////////////////////////////////////////////////////////////////////////
		
		private static string SES( Match match )
		{
		string result = string.Empty;
		
			// ******
			//
			// \[abtrvfne]
			// \x..
			// \u....
			//
			switch( match.Value[1] ) {
				case 'a':
					result = "\u0007";
					break;
					
				case 'b':
					result = "\u0008";
					break;
					
				case 't':
					result = "\u0009";
					break;
					
				case 'r':
					result = "\u000d";
					break;
					
				case 'v':
					result = "\u000b";
					break;
					
				case 'f':
					result = "\u000c";
					break;
					
				case 'n':
					result = "\u000a";
					break;
					
				case 'e':
					result = "\u001b";
					break;
					
				case 'x':
				case 'u':
				case 'U':
					result = char.ConvertFromUtf32( Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber) );
					break;
					
			}
			
			// ******
			return result;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public static string TranslateEscapes( this string text )
		{
			// ******
			Regex	rx = new Regex( @"\\(([abtrvfne])|(x[0-9A-Fa-f]{2})|([uU][0-9A-Fa-f]{4}))" );

			string result = rx.Replace( text, match => { return SES(match); } );

			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string EscapeEscapes( this string arg )
		{
			// ******
			StringBuilder chars = new StringBuilder();

			foreach( char ch in arg ) {
				switch( ch ) {
					case '\\':
						chars.Append( "\\\\" );
						break;

					case '\"':
						chars.Append( "\\\"" );
						break;

					case '\r':
						chars.Append( "\\r\\n" );
						break;

					case '\n':
						//
						// strip newlines, only CR's are processed (above)
						//
						break;

					default:
						chars.Append( ch );
						break;
				}
			}

			// ******
			return chars.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

//
// common, but NOT all used character entities
//
//&nbsp;		\u00A0		non-breaking space ISO8559-1 
//&copy;		\u00A9		copyright sign ISO8559-1 
//&reg;			\u00AE		registered trade mark sign ISO8559-1 
//&sup2;		\u00B2		superscript 2 (squared) ISO8559-1 
//&sup3;		\u00B3		superscript 3 (cubed) ISO8559-1 
//&quot;		\u0022		quotation mark ISO10646 
//&amp;			\u0026		ampersand sign ISO10646 
//&lt;			\u003C		less than sign ISO10646 
//&gt;			\u003E		greater than sign ISO10646 
//&ndash;		\u2013		en dash ISO10646 
//&mdash;		\u2014		em dash ISO10646 
//&lsquo;		\u2018		left single quote ISO10646 
//&rsquo;		\u2019		right single quote, apostrophe ISO10646 
//&ldquo;		\u201C		left double quotation mark ISO10646 
//&rdquo;		\u201D		right double quotation mark ISO10646 
//&bull;		\u2022		small black circle, bullet ISO10646 
//&dagger;	\u2020		dagger sign ISO10646 
//&Dagger;	\u2021		double dagger sign ISO10646 
//&prime;		\u2032		prime = minutes = feet ISO10646 
//&Prime;		\u2033		double prime = seconds = inches ISO10646 
//&lsaquo;	\u2039		single left pointing angle quote ISO10646 
//&rsaquo;	\u203A		single right pointing angle quote ISO10646 
//&euro;		\u20AC		euro sign ISO10646 
//&trade;		\u2122		Registered Trademark sign ISO10646 
//&tilde;		\u02DC		tilde sign ISO10646 
//&circ;		\u02C6		circumflex (or caret) sign ISO10646 
//&spades;	\u2660		black spade suit ISO10646 
//&clubs;		\u2663		black clubs suit ISO10646 
//&hearts;	\u2665		black heart suit ISO10646 
//&diams;		\u2666		black diamonds suit ISO10646 
//&loz;			\u25CA		lozenge ISO10646 
//&larr;		\u2190		left arrow ISO10646 
//&rarr;		\u2192		right arrow ISO10646 
//&uarr;		\u2191		up arrow ISO10646 
//&darr;		\u2193		down arrow ISO10646 
//&harr;		\u2194		right-left arrow ISO10646 
//&not;			\u00AC		NOT sign ISO8859-1 

		
		
//		private static string HtmlEncodeMatch( Match match, bool encodeAngleBrackets )
//		{
//			/*
//
//				if encodeAngleBrackets is false then '<', '>' and '"' are NOT encoded
//
//			*/
//
//			// ******
//			string value = match.Value;
//			switch( value ) {
//				case "\u00A0":		// non-breaking space ISO8559-1 
//					return "&nbsp;";
//					
//				case "\u00A9":		// copyright sign ISO8559-1 
//					return "&copy;";
//					
//				case "\u00AE":		// registered trade mark sign ISO8559-1 
//					return "&reg;";
//						
//				case "\u00B2":		// superscript 2 (squared) ISO8559-1 
//					return "&sup2;";
//					
//				case "\u00B3":		// superscript 3 (cubed) ISO8559-1 
//					return "&sup3;";
//					
//				//
//				// "
//				//
//				case "\u0022":		// quotation mark ISO10646 
//					//return "&quot;";
//					return encodeAngleBrackets ? "&quot;" : value;
//					
//				//
//				// &
//				//
//				case "\u0026":		// ampersand sign ISO10646 
//					return "&amp;";
//
//				//
//				// '
//				//
//				case "\u0027":
//					return "&apos;";
//
//				//
//				// <
//				//						
//				case "\u003C":		// less than sign ISO10646 
//					return encodeAngleBrackets ? "&lt;" : value;
//
//				//
//				// >
//				//	
//				case "\u003E":		// greater than sign ISO10646 
//					return encodeAngleBrackets ? "&gt;" : value;
//						
//				case "\u2013":		// en dash ISO10646 
//					return "&ndash;";
//					
//				case "\u2014":		// em dash ISO10646 
//					return "&mdash;";
//					
//				case "\u2018":		// left single quote ISO10646 
//					return "&lsquo;";
//					
//				case "\u2019":		// right single quote, apostrophe ISO10646 
//					return "&rsquo;";
//					
//				case "\u201C":		// left double quotation mark ISO10646 
//					return "&ldquo;";
//					
//				case "\u201D":		// right double quotation mark ISO10646 
//					return "&rdquo;";
//					
//				case "\u2022":		// small black circle, bullet ISO10646 
//					return "&bull;";
//					
//				case "\u2020":		// dagger sign ISO10646 
//					return "&dagger;";
//				
//				case "\u2021":		// double dagger sign ISO10646 
//					return "&Dagger;";
//				
//				case "\u2032":		// prime = minutes = feet ISO10646 
//					return "&prime;";
//					
//				case "\u2033":		// double prime = seconds = inches ISO10646 
//					return "&Prime;";
//					
//				case "\u2039":		// single left pointing angle quote ISO10646 
//					return "&lsaquo;";
//				
//				case "\u203A":		// single right pointing angle quote ISO10646 
//					return "&rsaquo;";
//				
//				case "\u20AC":		// euro sign ISO10646 
//					return "&euro;";
//					
//				case "\u2122":		// Registered Trademark sign ISO10646 
//					return "&trade;";
//					
//				case "\u02DC":		// tilde sign ISO10646 
//					return "&tilde;";
//					
//				case "\u02C6":		// circumflex (or caret) sign ISO10646 
//					return "&circ;";
//					
//				case "\u2660":		// black spade suit ISO10646 
//					return "&spades;";
//				
//				case "\u2663":		// black clubs suit ISO10646 
//					return "&clubs;";
//					
//				case "\u2665":		// black heart suit ISO10646 
//					return "&hearts;";
//				
//				case "\u2666":		// black diamonds suit ISO10646 
//					return "&diams;";
//					
//				case "\u25CA":		// lozenge ISO10646 
//					return "&loz;";
//						
//				case "\u2190":		// left arrow ISO10646 
//					return "&larr;";
//					
//				case "\u2192":		// right arrow ISO10646 
//					return "&rarr;";
//					
//				case "\u2191":		// up arrow ISO10646 
//					return "&uarr;";
//					
//				case "\u2193":		// down arrow ISO10646 
//					return "&darr;";
//					
//				case "\u2194":		// right-left arrow ISO10646 
//					return "&harr;";
//					
//				case "\u00AC":		// NOT sign ISO8859-1 
//					return "&not;";
//						
//
//				default:
//					return match.Value;
//			}
//		}
//

		/////////////////////////////////////////////////////////////////////////////

		//private static string htmlEncodeStr = //"<|>|\"|'|&|\u2018|\u2019";
		//	@"\u00A0|\u00A9|\u00AE|\u00B2|\u00B3|\u0022|\u0026|\u003C|\u003E|\u2013|\u2014|\u2018|\u2019|\u201C|\u201D|\u2022|\u2020|\u2021|\u2032|\u2033|\u2039|\u203A|\u20AC|\u2122|\u02DC|\u02C6|\u2660|\u2663|\u2665|\u2666|\u25CA|\u2190|\u2192|\u2191|\u2193|\u2194|\u00AC";
		//	
		//private static Regex htmlEncodeRegex;

		public static string HtmlEncode( this string str, bool encodeAngleBrackets )
		{
			//// ******
			//if( null == htmlEncodeRegex ) {
			//	htmlEncodeRegex = new Regex( htmlEncodeStr );
			//}
			//
			//// ******
			//string result = htmlEncodeRegex.Replace( str, match => { return HtmlEncodeMatch(match, encodeAngleBrackets); } );
			//return result;

			return SC.TryHtmlEncode( str, encodeAngleBrackets );

		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public static string HtmlDecode( this string str )
		{
			return SC.HtmlDecode( str );

		}
	}


}
