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

using NmpBase;
using Nmp;

using ReflectionHelpers;

namespace NmpExpressions {

	/////////////////////////////////////////////////////////////////////////////

	class IndexerInvoker : Invoker {

		ObjectInfo info;


		/////////////////////////////////////////////////////////////////////////////

		//protected static object [] FindMethod( ObjectInfo objInfo, object [] argsIn, out MethodBase miOut )
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

		//	//
		//	// never returns
		//	//
		//	ThreadContext.MacroError( "unable to locate an implementation of \"{0}\" whose parameters match (or can be converted from) \"{1}\"\nTried matching:\n{2}", objInfo.MemberName, Arguments.ObjectsTypeNames(argsIn), Arguments.GetMethodSignatures(methods) );

		//	// ******
		//	miOut = null;
		//	return null;
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static object Invoke( ObjectInfo objInfo, object [] argsIn )
		//{
		//	// ******
		//	MethodBase mi;
		//	object [] args = MethodInvoker.FindMethod( objInfo, argsIn, out mi );

		//	// ******
		//	//
		//	// if objInfo is a static standin then the method must be static, the inverse in
		//	// NOT true, an object instance CAN call a static method of it's class
		//	//
		//	if( objInfo.IsStatic && !mi.IsStatic ) {
		//		ThreadContext.MacroError( "attempt to call non static method \"{0}\" using a static class reference to \"{1}\"", objInfo.MemberName, objInfo.ObjectType.FullName );
		//	}

		//	// ******
		//	try {
		//		return mi.Invoke( objInfo.Object, args );
		//	}
		//	catch ( Exception ex ) {
		//		if( null != ExceptionHelpers.FindException( ex, typeof( IndexOutOfRangeException ) ) || null != ExceptionHelpers.FindException( ex, typeof( ArgumentOutOfRangeException ) ) ) {
		//			//
		//			// for index out of range we warn and return null
		//			//
		//			ThreadContext.MacroWarning( "Indexer out of range: [{0}]", argsIn[0] );
		//		}
		//		else {
		//			//
		//			// never returns
		//			//
		//			ThreadContext.MacroError( "Unable to index object \"{0}\", indexer [{1}]", objInfo.GetType().Name, argsIn[0] );
		//		}

		//		// ******
		//		return null;
		//	}
		//}

		public static object Invoke( ObjectInfo objInfo, object [] argsIn )
		{
			//// ******
			//MethodBase mi;
			//object [] args = MethodInvoker.FindMethod( objInfo, argsIn, out mi );

			//// ******
			////
			//// if objInfo is a static standin then the method must be static, the inverse in
			//// NOT true, an object instance CAN call a static method of it's class
			////
			//if( objInfo.IsStatic && !mi.IsStatic ) {
			//	ThreadContext.MacroError( "attempt to call non static method \"{0}\" using a static class reference to \"{1}\"", objInfo.MemberName, objInfo.ObjectType.FullName );
			//}

			//// ******
			//try {
			//	return mi.Invoke( objInfo.Object, args );
			//}
			//catch( Exception ex ) {
			//	if( null != ExceptionHelpers.FindException( ex, typeof( IndexOutOfRangeException ) ) || null != ExceptionHelpers.FindException( ex, typeof( ArgumentOutOfRangeException ) ) ) {
			//		//
			//		// for index out of range we warn and return null
			//		//
			//		ThreadContext.MacroWarning( "Indexer out of range: [{0}]", argsIn [ 0 ] );
			//	}
			//	else {
			//		//
			//		// never returns
			//		//
			//		ThreadContext.MacroError( "Unable to index object \"{0}\", indexer [{1}]", objInfo.GetType().Name, argsIn [ 0 ] );
			//	}

			//	// ******
			//	return null;
			//}


			MethodCacheItem cacheItem;
			var args = MethodInvoker.FindMethod( objInfo, argsIn, out cacheItem );

			try {
				//var result = handler.Invoke( objInfo.Object, args );
				//return result;

				return MethodInvoker.BaseMethodInvoke( cacheItem, objInfo.Object, args );
			}
			catch( Exception ex ) {
				if( null != ExceptionHelpers.FindException( ex, typeof( IndexOutOfRangeException ) ) || null != ExceptionHelpers.FindException( ex, typeof( ArgumentOutOfRangeException ) ) ) {
					//
					// for index out of range we warn and return null
					//
					ThreadContext.MacroWarning( "Indexer out of range: [{0}]", argsIn [ 0 ] );
				}
				else {
					//
					// never returns
					//
					ThreadContext.MacroError( "Unable to index object \"{0}\", indexer [{1}]", objInfo.GetType().Name, argsIn [ 0 ] );
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( info, argsIn );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IndexerInvoker( ObjectInfo objInfo )
		{
			info = objInfo;
		}

	}
}
