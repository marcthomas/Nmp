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
using System.Diagnostics.Eventing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Globalization;
using System.IO;
using System.Reflection;


//#pragma warning disable 1591

namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Methods to create argument lists from simple types described by
	/// casting string values
	/// </summary>
	/// 
	public static class Arguments {

		static Type typeofObject = typeof( object );
		static Type typeofString = typeof( string );
		static Type typeofType = typeof( Type );
		static Guid runtimeTypeGuid = new Guid( "97e69551-7329-39c5-ba61-2ca4b573e0e5" );


		/////////////////////////////////////////////////////////////////////////////

		static object CastEnum( object value, Type enumType )
		{
			// ******
			if( null == enumType || !enumType.IsEnum ) {
				throw new ArgumentException( "unexpected enum type" );
			}

			// ******
			try {
				if( value is string ) {
					return Enum.Parse( enumType, (string) value );
				}
				else {
					var i = Convert.ToInt32( value );
					return Enum.ToObject( enumType, i );
				}

			}
			catch {
			}

			// ******
			return null;
		}



		/////////////////////////////////////////////////////////////////////////////

		public static object ChangeType( object objIn, Type type )	//TypeCode typeCode )
		{

			//TODO: add checks for objIn type is derived from type

			// ******
			if( typeofObject.Equals( type ) ) {
				//
				// caller requesting an object - which we simply need to return
				//
				return objIn;
			}

			else if( typeofString.Equals( type ) ) {
				//
				// caller requesting an object - which we simply need to return
				//
				return null == objIn ? string.Empty : objIn.ToString();
			}

			// ******
			Type objInType = objIn.GetType();

			//else if( type.IsInstanceOfType(objIn) ) {
			//	return objIn;
			//}
			if( type == objInType || type.IsAssignableFrom( objInType ) ) {
				return objIn;
			}

			if( runtimeTypeGuid.Equals( objInType.GUID ) && ((Type) objIn) == type ) {
				//
				// requesting a Type object which 'objIn' is, and they are equal
				//
				return objIn;
			}

			if( objInType.Equals( type ) ) {
				//
				// Convert.ChangeType() uses IConvertable to change the type, if the
				// objIn does not support IConvertable then it will always fail - lots
				// of stuff does not support IConvertable
				//
				// so, we check to see if objIn directly matches 'type' and if so we
				// just need to return it
				//
				return objIn;
			}
			else if( typeofType.Equals( type ) ) {
				//
				// unfortunately if the 'type' being requested is a Type
				// object the check above will fail because if you do
				// a GetType() on a Type object the result  will be RuntimeType
				// object which we can not directly check for
				//
				// however, the GUID's are stable between 3.5 and 4.0 for the
				// RuntimeType and we CAN check that
				//
				if( runtimeTypeGuid.Equals( objInType.GUID ) ) {
					//
					// objIn is a Type object
					//
					return objIn;
				}
			}


			// ******
			TypeCode typeCode = Type.GetTypeCode( type );

			if( TypeCode.Boolean == typeCode ) {
				return Helpers.IsMacroTrue( objIn );
			}

			else if( type.IsEnum ) {
				return CastEnum( objIn, type );
			}

			// ******
			try {
				object changed = Convert.ChangeType( objIn, type );
				return changed;
			}
			catch( Exception ex ) {

//#if DEBUG
//				if( Debugger.IsAttached ) {
//					Debug.WriteLine( "could not convert \"{0}\" to \"{1}\"", objInType.Name, type.Name );
//				}
//#endif
				//objInType.EventConversionError( type );

				ConversionError.Write( objInType, type );
				throw ex;
			}
		}



		/////////////////////////////////////////////////////////////////////////////

		public static Type [] GetTypes( object [] args )
		{
			return Type.GetTypeArray( args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] ConvertArguments( IList<object> argsIn, int nArgsToConvert, ParameterInfo [] pi, int sizeArgsOutArray )
		{
			// ******
			if( argsIn.Count < nArgsToConvert ) {
				throw ExceptionHelpers.CreateArgumentException( "nArgsToConvert is greater than argument array" );
			}

			// ******
			if( sizeArgsOutArray < nArgsToConvert ) {
				throw ExceptionHelpers.CreateArgumentException( "sizeArgsOutArray is less than nArgsToConvert" );
			}

			// ******
			object [] argsOut = new object [ sizeArgsOutArray ];

			for( int i = 0; i < nArgsToConvert; i++ ) {
				try {
					argsOut [ i ] = ChangeType( argsIn [ i ], pi [ i ].ParameterType );
				}
				catch {
					return null;
				}
			}

			// ******
			return argsOut;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] ConvertArguments( IList<object> argsIn, int nArgsToConvert, IList<Type> types, int sizeArgsOutArray )
		{
			// ******
			if( argsIn.Count < nArgsToConvert ) {
				throw ExceptionHelpers.CreateArgumentException( "nArgsToConvert is greater than argument array" );
			}

			// ******
			if( sizeArgsOutArray < nArgsToConvert ) {
				throw ExceptionHelpers.CreateArgumentException( "sizeArgsOutArray is less than nArgsToConvert" );
			}

			// ******
			object [] argsOut = new object [ sizeArgsOutArray ];

			for( int i = 0; i < nArgsToConvert; i++ ) {
				try {
					argsOut [ i ] = ChangeType( argsIn [ i ], types [ i ] );
				}
				catch {
					return null;
				}
			}

			// ******
			return argsOut;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] ConvertArguments( IList<object> argsIn, ParameterInfo [] pi )
		{
			// ******
			object [] argsOut = new object [ argsIn.Count ];

			for( int i = 0; i < argsIn.Count; i++ ) {
				try {
					argsOut [ i ] = ChangeType( argsIn [ i ], pi [ i ].ParameterType );
				}
				catch {
					return null;
				}
			}

			// ******
			return argsOut;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] ConvertArguments( IList<object> argsIn, IList<Type> convertToTypes )
		{
			// ******
			object [] argsOut = new object [ argsIn.Count ];

			for( int i = 0; i < argsIn.Count; i++ ) {
				try {
					argsOut [ i ] = ChangeType( argsIn [ i ], convertToTypes [ i ] );
				}
				catch {
					return null;
				}
			}

			// ******
			return argsOut;
		}


		/////////////////////////////////////////////////////////////////////////////

		//public static object [] ConvertArguments( ArgumentList argsIn, Type [] convertToTypes )
		//{
		//	// ******
		//	object [] argsOut = new object [ argsIn.Count ];

		//	for( int i = 0; i < argsIn.Count; i++ ) {
		//		try {
		//			argsOut[ i ] = ChangeType( argsIn[i], convertToTypes[i] );
		//		}
		//		catch {
		//			return null;
		//		}
		//	}

		//	// ******
		//	return argsOut;
		//}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] PrependArgs( object [] argsIn, params object [] moreArgs )
		{
			// ******
			var moreArgsLength = moreArgs.Length;
			if( 0 == moreArgsLength ) {
				return argsIn;
			}

			// ******
			var argsInLength = argsIn.Length;

			var newArgs = new object [ moreArgsLength + argsInLength ];
			Array.Copy( argsIn, 0, newArgs, moreArgsLength, argsInLength );

			Array.Copy( moreArgs, 0, newArgs, 0, moreArgsLength );

			return newArgs;
		}

		/////////////////////////////////////////////////////////////////////////////

		public static Type [] GetTypes( ParameterInfo [] piArray )
		{
			// ******
			Type [] types = new Type [ piArray.Length ];

			int i = 0;
			foreach( ParameterInfo pi in piArray ) {
				types [ i++ ] = pi.ParameterType;
			}

			// ******
			return types;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Type [] GetTypes( IEnumerable<object> args )
		{
			// ******
			List<Type> types = new List<Type>();

			// ******
			foreach( object obj in args ) {
				types.Add( obj.GetType() );
			}

			// ******
			return types.ToArray();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string GetMethodSignatures( IEnumerable<MethodBase> methods )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			foreach( MethodBase mi in methods ) {
				if( sb.Length > 0 ) {
					sb.AppendLine();
				}

				sb.AppendFormat( "{0}.{1}", mi.DeclaringType.Name, mi.Name );
				sb.Append( ParametersTypeNames( mi.GetParameters(), "( ", " )" ) );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string ArgumentsTypeNames( IList<Type> argTypes, string openStr, string closeStr )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			if( null != openStr ) {
				sb.Append( openStr );
			}

			for( int i = 0; i < argTypes.Count; i++ ) {
				if( i > 0 ) {
					sb.Append( ", " );
				}
				sb.Append( argTypes [ i ].Name );
			}

			if( null != closeStr ) {
				sb.Append( closeStr );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string ArgumentsTypeNames( IList<Type> argTypes )
		{
			return ArgumentsTypeNames( argTypes, "", "" );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string ObjectsTypeNames( IList<object> args )
		{
			// ******
			Type [] types = new Type [ args.Count ];

			for( int i = 0; i < args.Count; i++ ) {
				types [ i ] = args [ i ].GetType();
			}

			// ******
			return ArgumentsTypeNames( types );

		}


		/////////////////////////////////////////////////////////////////////////////

		//public static string _ArgumentsTypeNames( object [] args )
		//{
		//	return ArgumentsTypeNames( Type.GetTypeArray(args) );
		//}


		/////////////////////////////////////////////////////////////////////////////

		public static string ParametersTypeNames( ParameterInfo [] propParams, string openStr, string closeStr )
		{
			// ******
			Type [] types = new Type [ propParams.Length ];

			for( int i = 0; i < propParams.Length; i++ ) {
				types [ i ] = propParams [ i ].ParameterType;
			}

			// ******
			return ArgumentsTypeNames( types, openStr, closeStr );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string ParametersTypeNames( ParameterInfo [] propParams )
		{
			// ******
			Type [] types = new Type [ propParams.Length ];

			for( int i = 0; i < propParams.Length; i++ ) {
				types [ i ] = propParams [ i ].ParameterType;
			}

			// ******
			return ArgumentsTypeNames( types );
		}


		/////////////////////////////////////////////////////////////////////////////

		//public static string ArgumentsTypeNames( MethodInfo mi )
		//{
		//	// ******
		//	ParameterInfo [] propParams = mi.GetParameters();

		//	//Type [] types = new Type [ propParams.Length ];
		//	//
		//	//for( int i = 0; i < propParams.Length; i++ ) {
		//	//	types[ i ] = propParams[i].ParameterType;
		//	//}
		//	//
		//	//// ******
		//	//return ArgumentsTypeNames( types );

		//	return ParametersTypeNames( propParams );
		//}


		///////////////////////////////////////////////////////////////////////////////
		//
		//private static object TryOneLastTime( string castType, string castValue )
		//{
		//	// ******
		//	//
		//	// this checks in the current assembly (LinFuHelpers.dll) and mscorelib.dll
		//	//
		//	Type type = Type.GetType( castType, false, /*ignoreCase*/ true );
		//	if( null != type ) {
		//		//
		//		// now we have to create it - cast value being the arguments
		//		//
		//	}
		//
		//	// ******
		//	return null;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		private static object CastExpression( string castType, string castValue )
		{
			// ******
			//			if( castType.IndexOf(';') >= 0 ) {
			//				//
			//				// not a cast but a string list
			//				//
			////SDebugger.Break();
			//			}

			// ******
			object obj = null;

			switch( castType.ToLower() ) {

				case "int16":
					Int16 i16Value;
					if( !Int16.TryParse( castValue, out i16Value ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to Int16: \"({0}) {1}\"", castType, castValue );
					}
					obj = i16Value;
					break;

				case "uint16":
					UInt16 u16Value;
					if( !UInt16.TryParse( castValue, out u16Value ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to UInt16: \"({0}) {1}\"", castType, castValue );
					}
					obj = u16Value;
					break;



				case "int":
				case "int32":
					Int32 i32Value;
					if( !Int32.TryParse( castValue, out i32Value ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to Int32: \"({0}) {1}\"", castType, castValue );
					}
					obj = i32Value;
					break;

				case "uint":
				case "uint32":
					UInt32 u32Value;
					if( !UInt32.TryParse( castValue, out u32Value ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to UInt32: \"({0}) {1}\"", castType, castValue );
					}
					obj = u32Value;
					break;



				case "long":
				case "longlong":
				case "int64":
					Int64 i64Value;
					if( !Int64.TryParse( castValue, out i64Value ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to Int64: \"({0}) {1}\"", castType, castValue );
					}
					obj = i64Value;
					break;

				case "ulong":
				case "uint64":
					UInt64 u64Value;
					if( !UInt64.TryParse( castValue, out u64Value ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to Int64: \"({0}) {1}\"", castType, castValue );
					}
					obj = u64Value;
					break;



				case "float":
				case "double":
					double doubleValue;
					if( !Double.TryParse( castValue, out doubleValue ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to double: \"({0}) {1}\"", castType, castValue );
					}
					obj = doubleValue;
					break;



				case "datetime":
					DateTime dtValue;
					if( !DateTime.TryParse( castValue, out dtValue ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to DateTime: \"({0}) {1}\"", castType, castValue );
					}
					obj = dtValue;
					break;



				case "decimal":
					decimal decValue;
					if( !decimal.TryParse( castValue, out decValue ) ) {
						throw new ArgumentCastException( castType, castValue, "unable to convert string to decimal: \"({0}) {1}\"", castType, castValue );
					}
					obj = decValue;
					break;



				case "bool":
					//obj = castValue.IsTruthful();
					obj = Helpers.IsMacroTrue( castValue );
					break;



				case "string":
					obj = castValue;
					break;



				case "null":
					obj = null;
					break;



				default:
					//obj = TryOneLastTime( castType, castValue );
					//if( null == obj ) {
					//	throw new ArgumentCastException( "invalid cast identifier \"{0}\"", castType );
					//}
					//break;
					throw new ArgumentCastException( castType, castValue, "invalid cast expression \"({0})\"", castType );
			}

			// ******
			return obj;
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//private static string regExStr = @"\(\s*?(.*?)\s*?\)\s*(.*?)\s*$";
		//private static Regex regex;
		//
		//private static object _CastArg( string s )
		//{
		//	// ******
		//	if( null == regex ) {
		//		regex = new Regex( regExStr );
		//	}
		//
		//	// ******
		//	Match match = regex.Match( s );
		//
		//	if( match.Groups.Count < 3 ) {
		//		return s;
		//	}
		//
		//	// ******
		//	string castType = match.Groups[1].ToString().ToLower();
		//	string castValue = match.Groups[2].ToString();
		//
		//	object value = SubstMatch( castType, castValue );
		//	if( null == value ) {
		//		value = s;
		//	}
		//	
		//	// ******
		//	return value;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public static object CastArg( string s )
		{
			// ******
			Debug.Assert( '(' == s [ 0 ], "Helpers.CastArg: cast or list string does not begin with a '('" );

			// ******
			//
			// calling SplitString() with 'splitCastMode' set to true, see the comments below where
			// we check the number of items returned
			//
			StringIndexer si = new StringIndexer( s );
			NmpStringList list = SplitString.Split( si, SplitString.END_OF_STRING, SplitString.END_OF_STRING, true );

			return CastArg( list, s );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object CastArg( NmpStringList list, string s )
		{

			//TODO: buggy, does not handle errors in list length

			// ******
			if( 0 == list.Count ) {
				//
				// "()" 
				//
				return string.Empty;
			}
			else if( 1 == list.Count ) {
				//
				// the opening parenthesis was NOT ballanced by a closer !
				//
			}
			else {
				//
				// if Count is greater than 2
				//
				Trace.Assert( 2 == list.Count, string.Format( "Helpers.CastArg: error parsing cast expression \"{0}\"", s ) );
			}

			// ******
			string castType = list [ 0 ];
			string castValue = list [ 1 ];

			object value = CastExpression( castType.Substring( 1, castType.Length - 2 ), castValue );
			if( null == value ) {
				value = s;
			}

			// ******
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static object [] CollectionArg( string s )
		{
			// ******
			Debug.Assert( '[' == s [ 0 ], "Helpers.CollectionArg: argument string does not begin with a '['" );
			Debug.Assert( ']' == s [ s.Length - 1 ], "Helpers.CollectionArg: argument string does end with a ']'" );

			// ******
			NmpStringList list = SplitString.Split( new StringIndexer( s.Substring( 1 ) ), '[', ']' );
			object [] args = Create( list );

			// ******
			return args;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] Create( NmpStringList argList )
		{
			// ******
			if( 0 == argList.Count ) {
				return new object [ 0 ];
			}

			// ******
			var args = new NmpObjectList();

			foreach( string arg in argList ) {
				string s = arg.Trim();

				// ******
				if( string.IsNullOrEmpty( s ) ) {
					args.Add( string.Empty );
				}
				else if( 1 == s.Length ) {
					args.Add( s );
				}
				else if( '(' == s [ 0 ] && '(' != s [ 1 ] ) {
					args.Add( CastArg( s ) );
				}
				else if( '[' == s [ 0 ] && '[' != s [ 1 ] ) {
					args.Add( CollectionArg( s ) );
				}
				else {
					//
					// as a string
					//
					args.Add( s );
				}
			}

			// ******
			return args.ToArray();
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Cast an argument in the form "(type) value", the string must be
		/// correctly formatted
		/// </summary>
		/// <param name="arg">string in the form "(cast type) value" </param>
		/// <returns></returns>

		public static object Create( string arg )
		{
			object [] array = Create( new NmpStringList() { arg } );
			return null == array || 0 == array.Length ? arg : array [ 0 ];
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Generates the string "(typename) value" to pass along to the single
		/// argument version of Creat()
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static object Create( string typeName, string arg )
		{
			return Create( string.Format( "({0}) {1}", typeName, arg ) );
		}
	}


}
