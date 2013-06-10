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
using System.Reflection;

namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	public class ArgumentMatcher {

		//static MethodCache methodCache = new MethodCache { };


		///////////////////////////////////////////////////////////////////////////////
		
		////
		//// abstract method
		////

		//abstract public object Invoke( object [] args );
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		//
		// calls abstract method above
		//

		//public object Invoke()
		//{
		//	return Invoke( new object [ 0 ] );
		//}


		/////////////////////////////////////////////////////////////////////////////

		private static object [] ConvertLastIsArray( Type arrayElementType, IList<object> argsIn, IList<Type> convertToTypes )
		{
			// ******
			//
			// on entry we are "guaranteed" that lastParamType.IsArray
			// and nArgsIn is >= nConvertToTypes - 1 which will allow
			// us (at a minimum) to fill in all params except (maybe)
			// the last
			//
			//Type arrayElementType = lastParamType.GetElementType();
			TypeCode typeCode = Type.GetTypeCode( arrayElementType );

			// ******
			//
			// method( int arg0, params object [] args )
			//
			// method( params string [] strArgs )
			//
			//	called with:
			//
			//		method( a, b, c )
			//		method()
			//
			// ******
			//
			// convert only the first nConvertToTypes - 1 from argsIn, if they can be converted then we'll
			// bundle up the remainder argsIn as an object or string array - note ConvertArguments() will
			// create the newArgs array as nConvertToTypes length
			//
			int nArgsIn = argsIn.Count;	//Length;
			int nConvertToTypes = convertToTypes.Count;	//Length;

			object [] newArgs = Arguments.ConvertArguments( argsIn, nConvertToTypes - 1, convertToTypes, nConvertToTypes );
			if( null == newArgs ) {
				//
				// unable to convert an argument, the following never returns
				//
				return null;
			}

			// ******
			//
			// add one additional for the last parameter which we is the array and was not
			// converted above - diference between the lengths plus the array entry
			//
			int nObjectsToCopy = 1 + nArgsIn - nConvertToTypes;
			object [] array = null;

			if( TypeCode.String == typeCode ) {
				array = new string[ nObjectsToCopy ];
				for( int iArg = nConvertToTypes - 1, index = 0; iArg < argsIn.Count; iArg++, index++ ) {
					array[ index ] = (string) Arguments.ChangeType( argsIn[iArg], typeof(string) );
				}
			}
			else if( arrayElementType.Equals(typeof(System.Linq.Expressions.ParameterExpression)) ) {
				//
				// array type is: System.Linq.Expressions.ParameterExpression
				//
				array = new System.Linq.Expressions.ParameterExpression [ nObjectsToCopy ];
				for( int iSource = nConvertToTypes - 1, iDest = 0; iDest < nObjectsToCopy; iSource++, iDest++ ) {
					array [ iDest ] = argsIn [ iSource ] as System.Linq.Expressions.ParameterExpression;
				}
			}
			else {
				array = new object[ nObjectsToCopy ];
				for( int iSource = nConvertToTypes - 1, iDest = 0; iDest < nObjectsToCopy; iSource++, iDest++ ) {
					array[ iDest ] = argsIn[ iSource ];
				}
			}

			// ******
			newArgs[ nConvertToTypes - 1 ] = array;
			return newArgs;
		}


		/////////////////////////////////////////////////////////////////////////////

		enum MatchArgsReq {
			MatchEqualArgCount,
			MatchLastIsArray,
			MatchBoth,
		}


		/////////////////////////////////////////////////////////////////////////////

		private static int FirstDefaultIndex( IList<ParameterInfo> parameters )
		{
			// ******
			for( int i = 0; i < parameters.Count; i++ ) {
				var attr = parameters[ i ].Attributes;
				if( attr == (ParameterAttributes.Optional | ParameterAttributes.HasDefault) ) {
					return i;
				}
			}

			// ******
			return -1;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static object [] MatchWithDefaultArgs( int firstDefArgToUse, IList<ParameterInfo> paramInfos, IList<Type> convertToTypes, IList<object> argsIn )
		{
			// ******
			var newArgs = new List<object>( argsIn );
			for( int i = firstDefArgToUse; i < paramInfos.Count; i++ ) {
				newArgs.Add( paramInfos[i].DefaultValue );
			}

			// ******
			object [] args = Arguments.ConvertArguments( newArgs, convertToTypes );
			return args;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static object [] MatchArgsToMethod( MatchArgsReq matchReq, IList<ParameterInfo> paramInfos, IList<Type> convertToTypes, IList<object> argsIn )
		{
			// ******
			int argsInLength = argsIn.Count;
			int convertToTypesLength = convertToTypes.Count;

			// ******
			if( MatchArgsReq.MatchEqualArgCount == matchReq || MatchArgsReq.MatchBoth == matchReq ) {
				//
				// first: where number of arguments match, this handles 0 argsIn
				// and 0 parameters just fine
				//
				if( argsInLength == convertToTypesLength ) {
					object [] args = Arguments.ConvertArguments( argsIn, convertToTypes );
					if( null != args ) {
						return args;
					}
				}

				else if( null != paramInfos && argsInLength < convertToTypesLength ) {
					int index = FirstDefaultIndex( paramInfos );

					if( index >= 0 && index <= argsInLength ) {
						//
						// there are default arguments starting at 'index'
						//
						object [] args = MatchWithDefaultArgs( argsInLength, paramInfos, convertToTypes, argsIn );
						if( null != args ) {
							return args;
						}
					}
				}
			}

			// ******
			if( (MatchArgsReq.MatchLastIsArray == matchReq || MatchArgsReq.MatchBoth == matchReq) && convertToTypesLength > 0 ) {
				//
				// where last parameter IsArray (object or string) and argsInLength >= number
				// of method arguments minus on (numMethodArgs - 1)
				//
				var parameterType = convertToTypes[ convertToTypesLength - 1 ];

				if( parameterType.IsArray && argsInLength >= convertToTypesLength - 1 ) {
					var arrayElementType = parameterType.GetElementType();
					var typeCode = Type.GetTypeCode( arrayElementType );

					if( TypeCode.String == typeCode || TypeCode.Object == typeCode ) {
						object [] args = ConvertLastIsArray( arrayElementType, argsIn, convertToTypes );

						if( null != args ) {
							return args;
						}

					}
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		// 
		// returns null on failure
		//

		public static object [] MatchBuiltinMacroArgs( string callee, IList<ParameterInfo> paramInfos, IList<Type> convertToTypes, IList<object> argsIn, bool allowArgumetsTrunction )
		{
			// ******
			object [] args = MatchArgsToMethod( MatchArgsReq.MatchBoth, paramInfos, convertToTypes, argsIn );
			
			if( null == args ) {
				if( allowArgumetsTrunction ) {
					//
					// if there are too many args for the macro, truncate args and see if we 
					// can get a match - ignore the remaining args
					//
					int convertToTypesLength = convertToTypes.Count;	//Length;

					if( argsIn.Count > convertToTypesLength ) {
						
						object [] truncatedArgs = new object[ convertToTypesLength ];
						for( int i = 0; i < convertToTypesLength; i++ ) {
							truncatedArgs[ i ] = argsIn[ i ];
						}
						
						args = Arguments.ConvertArguments( truncatedArgs, convertToTypes );
						if( null != args ) {
							return args;
						}
					}
				}

				// ******
				if( argsIn.Count < convertToTypes.Count ) {
					return null;
				}
				
				// ******
				return null;
			}

			// ******
			return args;
		}


		/////////////////////////////////////////////////////////////////////////////

		// 
		// returns null on failure
		//

		public static object [] MatchArgs( MethodBase mi, object [] argsIn )
		{
			////return MatchArgs( mi.Name, Arguments.GetTypes(mi.GetParameters()), argsIn );

			// ******
			ParameterInfo [] paramInfos = mi.GetParameters();
			Type [] convertToTypes = Arguments.GetTypes( paramInfos );
			object [] args = MatchArgsToMethod( MatchArgsReq.MatchBoth, paramInfos, convertToTypes, argsIn );
			
			// ******
			return args;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Tuple<MethodBase, object[]> MatchArgs2( IList<MethodBase> methods, object [] argsIn )
		{
			MethodBase mb;
			var result = MatchArgs( methods, argsIn, out mb );
			return null == result ? null : new Tuple<MethodBase, object []>( mb, result );
		}
		

		/////////////////////////////////////////////////////////////////////////////

		// 
		// returns null on failure
		//

		public static object [] MatchArgs( IList<MethodBase> methods, object [] argsIn, out MethodBase miOut )
		{
			// ******
			//
			// 0 args ??
			// 
			if( 0 == argsIn.Length ) {
				//
				// cycle through the methods
				//			
				foreach( var mb in methods ) {
					ParameterInfo[] parameters = mb.GetParameters();
					if( 0 == parameters.Length ) {
						//
						// select method that matches with no arguments
						// 
						miOut = mb;
						return new object [0];
					}
				}
			}

//			// ******
//			//
//			// 1 argument ??
//			// 
//			if( 1 == methods.Count ) {
//
//// ?? should we not itterate through all of them ??
//
//				MethodBase mi = methods[0];
//				object [] result = MatchArgs( mi, argsIn );
//
//				if( null != result ) {
//					miOut = mi;
//					return result;
//				}
//			}

			// ******
			int argsInLength = argsIn.Length;
			
			List<ParameterInfo []> paramInfoList = new List<ParameterInfo[]>();
			List<Type []> convertToArgumentsList = new List<Type[]>();

			// ******
			//
			// first: where number of arguments match, this handles 0 argsIn
			// and 0 parameters just fine
			//
			for( int i = 0; i < methods.Count; i++ ) {
				MethodBase mi = methods[ i ];
				//
				// only want to do this once
				//
				ParameterInfo [] paramInfos = mi.GetParameters();
				paramInfoList.Add( paramInfos );

				//Type [] convertToArguments = Arguments.GetTypes( mi.GetParameters() );
				Type [] convertToArguments = Arguments.GetTypes( paramInfos );
				convertToArgumentsList.Add( convertToArguments );

				object [] args = MatchArgsToMethod( MatchArgsReq.MatchEqualArgCount, paramInfos, convertToArguments, argsIn );
				if( null != args ) {
					miOut = mi;
					return args;
				}
			}

			// ******
			//
			// next: where last parameter IsArray (object or string) and argsInLength >= number
			// of method arguments minus on (numMethodArgs - 1)
			//
			for( int i = 0; i < methods.Count; i++ ) {
				object [] args = MatchArgsToMethod( MatchArgsReq.MatchLastIsArray, paramInfoList[i], convertToArgumentsList[i], argsIn );
				if( null != args ) {
					miOut = methods[ i ];
					return args;
				}
			}

			// ******
			miOut = null;
			return null;
		}


	}
}
