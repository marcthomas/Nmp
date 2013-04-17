#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// JSONVisualizer.cs
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

////#pragma warning disable 169


namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	public static class JSONValue {


		//private enum NUM_PART { Minus = 1, Integer, Fractional, Exponent }

		// ******
		//
		// we use this regex form because we've already unescaped the
		// string
		//
		private static string regExDateStr1 = @"^/Date\((-?\d+)\)/$";
		private static Regex dateRegex = null;

	
		/////////////////////////////////////////////////////////////////////////////
	
		public static JSONDateFormat	DateFormat { get; set; }


		/////////////////////////////////////////////////////////////////////////////

		public static DateTime ConvertFromUnixTimestamp( double timestamp )
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return origin.AddSeconds(timestamp / 1024);
		}


		/////////////////////////////////////////////////////////////////////////////

		public static double ConvertToUnixTimestamp( DateTime date )
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan diff = date - origin;
			return Math.Floor(diff.TotalSeconds) * 1024;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static object DecodeDateOrObject( string str )
		{
			/*

				{ currentDate: "@1163531522089@" }

					milliseconds since 1 jan 1970

					http://weblogs.asp.net/bleroy/archive/2008/01/18/dates-and-json.aspx

					"\\/Date\((-?\d+)\)\\/"

					minus for dates BEFORE 1970


			*/

			// ******
			if( string.IsNullOrEmpty(str) || str.Length < 3 ) {
				return str;
			}

			// ******
			char chFirst = str[ 0 ];
			char chLast = str[ str.Length - 1 ];

			//
			// "@xxxxx@"
			//
			if( '@' == chFirst && '@' == chLast ) {
				long ms;
				if( long.TryParse(str.Substring(1, str.Length - 2), out ms) ) {
					return ConvertFromUnixTimestamp( ms );
				}
			}

			//
			// "\/Date(1198908717056)\/",
			//
			// note: the '\' will have been removed when the string was processed,
			// '/' is escaped in the JSON standard so "\/" becomes "/"
			//
			if( '/' == chFirst && '/' == chLast ) {
				if( null == dateRegex ) {
					dateRegex = new Regex( regExDateStr1 );
				}

				Match match = dateRegex.Match( str );
				if( match.Success ) {
					long ms;
					if( long.TryParse(match.Groups[1].Value, out ms) ) {
						return ConvertFromUnixTimestamp( ms );
					}
				}
			}

			//
			// ISO Date format parsing - from Rick Strahl
			//
			if( str.Contains(':') && str.Contains('Z') && str.Contains('T') ) {
				try {
					DateTime date = DateTime.Parse(str, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.RoundtripKind).ToLocalTime(); 
					return date;
				}
				catch {
				}
			}

			DateTime dateTime;
			if( DateTime.TryParse(str, out dateTime) ) {
				return dateTime;
			}

			//Sat Mar 12 11:38:53 +0000 2011
			if( DateTime.TryParseExact(str, "ddd MMM dd HH:mm:ss +0000 yyyy", null, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out dateTime) ) {
				return dateTime;
			}

			// ******
			//
			// not a date
			//
			return str;
		}


		/////////////////////////////////////////////////////////////////////////////

		private enum NUM_PART { 
			Minus = 1, 
			Integer, 
			Fractional, 
			Exponent
		}

		private static string regExNumStr = @"
^([-+]?)
(?:([0-9]+)(?:(\.[0-9]*))?
|
\.[0-9]+)((?:[eE][-+]?[0-9]+)?)$
";
		private static Regex numRegex = null;


		public static double? ConvertToNumber( string valueStr, out string error )
		{
			/*
				[
				0
				-1e5,
				0e5,
				1,
				1.2,
				.3,
				1e3,
				1.2e-12,
				30e6,
				-1,
				-1.2,
				-.3,
				-1e20,
				-1.2e-12,
				]
			*/

			// ******
			if( string.IsNullOrEmpty(valueStr) ) {
				error = string.Format( "JSON convert to number or date: value string is empty" );
				return null;
			}

			// ******
			error = string.Empty;

			// ******
			if( '0' == valueStr[0] ) {
				if( 1 == valueStr.Length ) {
					return (double) 0;
				}
				//else {
				//	handler.Error( "convert JSON string to number, leading '0's not allowed: \"{0}\"", valueStr );
				//	throw new JSONBail();
				//}
			}

			// ******
			if( null == numRegex ) {
				numRegex = new Regex( regExNumStr, RegexOptions.IgnorePatternWhitespace );
			}

			Match match = numRegex.Match( valueStr );
			if( ! match.Success ) {
				error = string.Format( "convert JSON string to number, invalid number string: \"{0}\"", valueStr );
				return null;
			}

			//bool haveMinusPart = match.Groups[ (int) NUM_PART.Minus ].Success;
			//bool haveIntPart = match.Groups[ (int) NUM_PART.Integer ].Success;
			bool haveFracPart = match.Groups[ (int) NUM_PART.Fractional ].Success;
			bool haveExpPart = match.Groups[ (int) NUM_PART.Exponent ].Success;

			if( haveExpPart || haveFracPart ) {
				double doubleValue;
				if( double.TryParse(valueStr, out doubleValue) ) {
					return doubleValue;
				}
			}
			else {
				//
				// integer
				//
				long longValue;
				if( long.TryParse(valueStr, out longValue) ) {
					return (double) longValue;
				}
			}

			// ******
			error = string.Format( "convert JSON string to number, invalid number string: \"{0}\"", valueStr );
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string Encode( object value, JSONDateFormat dateFormat = JSONDateFormat.ISODate )
		{
			// ******
			string valueStr = string.Empty;
			Type valueType = null == value ? null : value.GetType();
			TypeCode typeCode = Type.GetTypeCode( valueType );

			// ******
			if( TypeCode.Empty == typeCode ) {
				//
				// null
				//
				valueStr = "null";
			}
			else if( TypeCode.Boolean == typeCode ) {
				//
				// true or false
				//
				valueStr = ((bool) value) ? "true" : "false";
			}
			else if( typeCode >= TypeCode.SByte && typeCode <= TypeCode.Decimal ) {
				//
				// number, call ToString() - do NOT wrap with quotes
				//
				valueStr = value.ToString();
			}
			else if( TypeCode.DateTime == typeCode ) {
				//
				// date
				//
				long seconds = (long) ConvertToUnixTimestamp( (DateTime) value );

				if( JSONDateFormat.At1970 == dateFormat ) {
					valueStr = string.Format( "\"@{0}@\"", seconds );
				}
				else if( JSONDateFormat.Date1970 == dateFormat ) {
					valueStr = string.Format( "\"\\/Date({0})\\/\"", seconds );
				}
				else {	//if( JSONDateFormat.ISODate == dateFormat ) {
					//
					// ?? "s" or "u" ??
					//
					valueStr = string.Format( "\"{0}\"", ((DateTime) value).ToString("s") );
				}
			}
			else {
				//
				// string object and all other object types, call ToString()
				// and wrap the result in quotes - the result is a string
				//
				valueStr = string.Format( "\"{0}\"", value.ToString() );
			}

			// ******
			return valueStr;
		}


		/////////////////////////////////////////////////////////////////////////////

		static JSONValue()
		{
			DateFormat = JSONDateFormat.ISODate;
		}

		
	}


}
