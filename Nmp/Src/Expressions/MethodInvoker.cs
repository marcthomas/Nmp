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
using System.Reflection;
using System.Text;

using NmpBase;
using Nmp;

using ReflectionHelpers;

namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	class MethodInvoker : Invoker {

		static NullResult nullResult = new NullResult();

		ObjectInfo info;


		/////////////////////////////////////////////////////////////////////////////

		//public static object MethodInvoke( object obj, MethodBase mi, object [] args )
		//{
		//	// ******
		//	object result = null;

		//	try {
		//		result = mi.Invoke( obj, args );
		//	}
		//	catch( ExitException ex ) {
		//		throw ex;
		//	}
		//	catch( Exception ex ) {

		//		Exception exCheck = ex;
		//		while( null != exCheck ) {
		//			if( exCheck is ExitException ) {
		//				//
		//				// this is a test from NmpTests: NewObject.nmp
		//				//
		//				// by capturing and rethrowing it here we:
		//				//
		//				//	1. don't call MacroError() so the test will succeed
		//				//	2. it cleans up capturing the error in the test
		//				//
		//				// this code was introducted once we wrapped a try/catch around invoking
		//				// methods - which, for some unexplicable reason, we hadn't been doing
		//				//
		//				// note: ExitException would never be thrown by .NET code unless we did
		//				// it (as we do) with a Lambda expression in the NewObject.nmp test
		//				//
		//				throw exCheck;
		//			}
		//			exCheck = exCheck.InnerException;
		//		}

		//		// ******
		//		var errorAndMethod = ExceptionHelpers.RecursiveMessage( ex, "error invoking method \"{0}\"", mi.Name );

		//		var sb = new StringBuilder();
		//		sb.AppendFormat( "Arguments as passed to method (may have been coerced): {0}\n", mi.Name );
		//		foreach( var arg in args ) {
		//			sb.AppendLine( null != arg ? arg.ToString() : "null" );
		//		}

		//		//
		//		// never returns
		//		//
		//		ThreadContext.MacroError( "{0}\n{1}", errorAndMethod, sb.ToString() );
		//		return null;
		//	}

		//	// ******
		//	if( typeof( void ) == (mi as MethodInfo).ReturnType ) {
		//		result = nullResult;
		//	}

		//	// ******
		//	return result;
		//}


		/////////////////////////////////////////////////////////////////////////////

		public static object BaseMethodInvoke( MethodCacheItem cacheItem, object obj, object [] args )
		{
			// ******
			var returnsVoid = typeof( void ) == cacheItem.MethodInfo.ReturnType;

			// ******
			var result = cacheItem.Handler.Invoke( obj, args );
			if( returnsVoid ) {
				result = nullResult;
			}

			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object MethodInvoke( MethodCacheItem cacheItem, object obj, object [] args )
		{
			// ******
			var mi = cacheItem.MethodInfo;
			var returnsVoid = typeof( void ) == (mi as MethodInfo).ReturnType;

			try {
				return BaseMethodInvoke( cacheItem, obj, args );
			}
			catch( ExitException ex ) {
				throw ex;
			}
			catch( Exception ex ) {

				Exception exCheck = ex;
				while( null != exCheck ) {
					if( exCheck is ExitException ) {
						//
						// this is a test from NmpTests: NewObject.nmp
						//
						// by capturing and rethrowing it here we:
						//
						//	1. don't call MacroError() so the test will succeed
						//	2. it cleans up capturing the error in the test
						//
						// this code was introducted once we wrapped a try/catch around invoking
						// methods - which, for some unexplicable reason, we hadn't been doing
						//
						// note: ExitException would never be thrown by .NET code unless we did
						// it (as we do) with a Lambda expression in the NewObject.nmp test
						//
						throw exCheck;
					}
					exCheck = exCheck.InnerException;
				}

				// ******
				var errorAndMethod = ExceptionHelpers.RecursiveMessage( ex, "error invoking method \"{0}\"", mi.Name );

				var sb = new StringBuilder();
				sb.AppendFormat( "Arguments as passed to method (may have been coerced): {0}\n", mi.Name );
				foreach( var arg in args ) {
					sb.AppendLine( null != arg ? arg.ToString() : "null" );
				}

				//
				// never returns
				//
				ThreadContext.MacroError( "{0}\n{1}", errorAndMethod, sb.ToString() );
				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		//public static object [] FindMethod( ObjectInfo objInfo, object [] argsIn, out MethodBase miOut )
		//{
		//	// ******
		//	IList<MethodBase> methods = objInfo.MembersAs<MethodBase>();

		//	// ******
		//	MethodBase mi;
		//	object [] args = MatchArgs( methods, argsIn, out mi );
		//	if( null != args ) {
		//		miOut = mi;
		//		return args;
		//	}

		//	// ******
		//	//
		//	// never returns
		//	//
		//	ThreadContext.MacroError( "unable to locate an implementation of \"{0}\" whose parameters match (or can be converted from) \"{1}\"\nTried matching:\n{2}", objInfo.MemberName, Arguments.ObjectsTypeNames( argsIn ), Arguments.GetMethodSignatures( methods ) );
		//	miOut = null;
		//	return null;
		//}


		/////////////////////////////////////////////////////////////////////////////

		public static MethodCacheItem FindPropertyMethod( ObjectInfo objInfo )
		{
			// ******
			object [] emptyObjectArray = null;	//new object [ 0 ];
			//InvokeHandler handler = null;

			var cachedItem = methodCache.GetCachedHandler( objInfo, objInfo.MemberName, emptyObjectArray );
			if( null != cachedItem ) {
				//handler = cachedItem.Handler;
				//return handler;
				return cachedItem;
			}

			// ******
			MethodInfo mi = objInfo.MemberAs<MethodInfo>();
			if( null != mi ) {

				if( objInfo.IsStatic && !mi.IsStatic ) {
					ThreadContext.MacroError( "attempt to call non static property \"{0}\" using a static reference to \"{1}\"", mi.Name, mi.ReflectedType.FullName );
				}

				var cacheItem = methodCache.Generate( objInfo, objInfo.MemberName, mi, emptyObjectArray );
				return cacheItem;
			}

			// ******
			//
			// never returns
			//
			ThreadContext.MacroError( "unable to locate an implementation of property \"{0}\"", objInfo.MemberName );
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] FindMethod( ObjectInfo objInfo, object [] argsIn, out MethodCacheItem cacheItem )
		{
			// ******
			MethodBase mi = null;
			object [] args = null;

			cacheItem = methodCache.GetCachedHandler( objInfo, objInfo.MemberName, argsIn );
			if( null != cacheItem ) {
				args = MatchArgs( new List<MethodBase> { cacheItem.MethodInfo }, argsIn, out mi );
				if( null != args ) {
					return args;
				}
			}

			// ******
			IList<MethodBase> methods = objInfo.MembersAs<MethodBase>();
			args = MatchArgs( methods, argsIn, out mi );
			if( null != args ) {

				if( objInfo.IsStatic && !mi.IsStatic ) {
					ThreadContext.MacroError( "attempt to call non static property \"{0}\" using a static reference to \"{1}\"", mi.Name, mi.ReflectedType.FullName );
				}

				cacheItem = methodCache.Generate( objInfo, objInfo.MemberName, mi as MethodInfo, argsIn );
				return args;
			}

			// ******
			//
			// never returns
			//
			ThreadContext.MacroError( "unable to locate an implementation of \"{0}\" whose parameters match (or can be converted from) \"{1}\"\nTried matching:\n{2}", objInfo.MemberName, Arguments.ObjectsTypeNames( argsIn ), Arguments.GetMethodSignatures( methods ) );
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		//public static object Invoke( ObjectInfo objInfo, object [] argsIn )
		//{
		//	// ******
		//	MethodBase mi;
		//	object [] args = FindMethod( objInfo, argsIn, out mi );
		//	if( objInfo.IsStatic && !mi.IsStatic ) {
		//		ThreadContext.MacroError( "attempt to call non static method \"{0}\" using a statics reference to \"{1}\"", mi.Name, mi.ReflectedType.FullName );
		//	}
		//	return MethodInvoke( objInfo.Object, mi, args );
		//}


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( ObjectInfo objInfo, object [] argsIn )
		{
			// ******
			MethodCacheItem cacheItem;
			var args = FindMethod( objInfo, argsIn, out cacheItem );
			return MethodInvoke( cacheItem, objInfo.Object, args );

			//try {
			//	var result = handler.Invoke( objInfo.Object, args );
			//	if( returnsVoid ) {
			//		result = nullResult;
			//	}

			//	// ******
			//	return result;

			//}
			//catch( Exception ex ) {
			//	throw ExceptionHelpers.CreateException( ex, "exception invoking \"{0}\" on \"{1}\"", objInfo.MemberName, objInfo.ObjectType.Name );
			//}
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( info, argsIn );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodInvoker( ObjectInfo objInfo )
		{
			info = objInfo;
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodInvoker( object obj, string name )
			: this( new ObjectInfo( obj, name ) )
		{
		}

	}


}
