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
	/// Helper methods that have not found a home elsewhere
	/// </summary>

	public static partial class Helpers {


		/////////////////////////////////////////////////////////////////////////////
		
		public static bool IsMacroTrue( string strIn )
		{
			// ******
			if( string.IsNullOrEmpty(strIn) ) {
				return false;
			}

			// ******
			string str = strIn.Trim().ToLower();

			switch( str ) {
				case "1":
				case "true":
				case "yes":
				case "on":
					return true;

				//case "0":
				//case "false":
				//case "no":
				//case "off:
				//	return false;
			}

			// ******
			return false;
		}
     

		/////////////////////////////////////////////////////////////////////////////
		
		public static bool IsMacroTrue( object objIn )
		{
			// ******
			TypeCode objTypeCode = Type.GetTypeCode( objIn.GetType() );

			switch( objTypeCode ) {
				case TypeCode.String:
					return IsMacroTrue( objIn as string );

				case TypeCode.Boolean:
					return (bool) objIn;

				case TypeCode.SByte:
					return ((SByte) objIn) != 0;

				case TypeCode.Byte:
					return ((Byte) objIn) != 0;

				case TypeCode.Int16:
					return ((Int16) objIn) != 0;

				case TypeCode.UInt16:
					return ((UInt16) objIn) != 0;

				case TypeCode.Int32:
					return ((Int32) objIn) != 0;

				case TypeCode.UInt32:
					return ((UInt32) objIn) != 0;

				case TypeCode.Int64:
					return ((Int64) objIn) != 0;

				case TypeCode.UInt64:
					return ((UInt64) objIn) != 0;

				case TypeCode.Single:
					return ((Single) objIn) != 0;

				case TypeCode.Double:
					return ((Double) objIn) != 0;

				case TypeCode.Decimal:
					return ((Decimal) objIn) != 0;
			}

			// ******
			return false;
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public static string MacroTruthString( bool isTrue )
		{
			return isTrue ? SC.MACRO_TRUE : SC.MACRO_FALSE;
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public static bool IsNetValidIdentifier( string name, bool allowDots = false )
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

		public static ICollection GetNamespaces( Assembly assembly )
		{
			// ******
			NmpStringList list = new NmpStringList( unique: true );

			foreach( Type t in assembly.GetTypes() ) {
				string ns = t.Namespace;

				if( IsNetValidIdentifier(ns, allowDots : true) ) {
					list.Add( ns );
				}
			}

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Type _FindType( Assembly assembly, string typeName, bool ignoreCase )
		{
			// ******
			Type type = null;

			// ******
			if( null != assembly ) {
				//
				// this will locate the type if if it has a FULL namespace preceeding
				// the name of the type itself, e.g. "Data.TableData" - it should also
				// find the type if it has no namespace
				//
				type = assembly.GetType( typeName, false, ignoreCase );
				if( null != type ) {
					return type;
				}

				// ******
				//
				// this will ONLY find a descrete type name, e.g. TableData (no namespaces),
				// downside is it will find the FIRST type with this name, there can be
				// multiple types with this name if they are in different namespaces or
				// different assemblies
				//
				Type [] types = assembly.GetTypes();
				foreach( Type t in types ) {
					if( 0 == string.Compare( typeName, t.Name, ignoreCase ) ) {
						return t;
					}
				}
			}

			// ******
			//
			// if there is no assembly passed to us or the type was not found then we have
			// two fallbacks: try Type.GetType(), and check for some of the types ourself
			//
			//
			try {
				type = Type.GetType( typeName, false, ignoreCase );
			}
			catch {  }

			if( null == type ) {
				switch( typeName.ToLower() ) {
					case "int16":
						type = typeof( Int16 );
						break;

					case "uint16":
						type = typeof( UInt16 );
						break;

					case "int":
					case "int32":
						type = typeof( Int32 );
						break;

					case "uint":
					case "uint32":
						type = typeof( UInt32 );
						break;

					case "long":
					case "int64":
						type = typeof( Int64 );
						break;

					case "ulong":
					case "uint64":
						type = typeof( UInt64 );
						break;

					case "float":
					case "double":
						type = typeof( double );
						break;

					case "datetime":
						type = typeof( DateTime );
						break;

					case "decimal":
						type = typeof( decimal );
						break;

					case "bool":
						type = typeof( bool );
						break;

					case "string":
						type = typeof( string );
						break;
				}
			}

			// ******
			return type;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string SafeStringFormat( string fmt, params object [] args )
		{
			// ******
			if( null == fmt ) {
				return string.Empty;
			}

			// ******
			if( args.Length > 0 ) {
				try {
					return string.Format( fmt, args );
				}
				catch {
					//return string.Format( "[SafeStringFormat: exception calling string.Format()] format string: {0}", fmt );
					return string.Format( "{0}", fmt );
				}
			}
			//else {
			//	fmt = fmt.Replace( "{", "{{");
			//	fmt = fmt.Replace( "}", "}}" );
			//}

			// ******
			return fmt;
		}


		///////////////////////////////////////////////////////////////////////////////
		
		public static string CompressAllWhiteSpace( string str )
		{
			// ******
			//
			// any run of white-space is converted to a single space
			//
			Regex	rx = new Regex( @"(?xs)\s+" );
			return rx.Replace( str, " " );
		}
		
		
		///////////////////////////////////////////////////////////////////////////////
		
		public static string IntraLineCompressWhiteSpace( string str )
		{
			// ******
			//
			// any run of white-space is converted to a single space
			//
			Regex	rx = new Regex( @"(?xs)[\f\t\v\x85\p{Zs}]+" );
			return rx.Replace( str, " " );
		}
		
		
		///////////////////////////////////////////////////////////////////////////////
		
		public static string StripNewlines( string str )
		{
			// ******
			//
			// any run of white-space is converted to a single space
			//
			Regex	rx = new Regex( @"(?xs)(\r\n)|(\r)|(\n)" );
			return rx.Replace( str, "" );
		}
		


		/////////////////////////////////////////////////////////////////////////////

		static string kvpRegexStr = @"\s*(.*?)\s*?=\s*(.*?)\s*(?:,|$)";
		static Regex kvpRegex = null;

		public static MatchCollection SplitKeyValuePairs( string values )
		{
			// ******
			if( null == kvpRegex ) {
				kvpRegex = new Regex( kvpRegexStr );
			}

			// ******
			return kvpRegex.Matches( values );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Dictionary<string, string> GetKeyValuePairs( string values, bool toLowerKey )
		{
			// ******
			var dict = new Dictionary<string, string>();

			// ******
			foreach( Match match in SplitKeyValuePairs(values) ) {
				//
				// group[0] represents the overall capture
				// group[1] is the key
				// group[2] is the value
				//
				string key = match.Groups[1].Value;
				string value = match.Groups[2].Value;
				
				if( toLowerKey ) {
					key = key.ToLower();
				}

				dict.Add( key, value );
			}

			// ******
			return dict;
		}


	}


	
}
