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
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Text;

using NmpBase;
using Nmp;

using ReflectionHelpers;




namespace NmpExpressions {

	/////////////////////////////////////////////////////////////////////////////

	class MethodExtensionInvoker : Invoker {

		object obj;
		Type objType;
		string methodName;
		ExtensionTypeDictionary methodExtensions;


		/////////////////////////////////////////////////////////////////////////////

		//object [] FindMethod( object [] argsIn, out MethodBase miOut )
		//{
		//	// ******
		//	var newArgs = Arguments.PrependArgs( argsIn, obj );
		//	var result = methodExtensions.FindExtensionMethod( obj, methodName, newArgs, MatchArgs2 );

		//	if( null != result && null != result.Item1 ) {
		//		miOut = result.Item1;
		//		return result.Item2;

		//	}

		//	// ******
		//	//
		//	// never returns
		//	//
		//	ThreadContext.MacroError( "unable to locate an implementation of \"{0}\" whose parameters match (or can be converted from) \"{1}\"\nTried matching:\n{2}", methodName, Arguments.ObjectsTypeNames( argsIn ), Arguments.GetMethodSignatures( result.Item3 ) );
		//	miOut = null;
		//	return null;
		//}

		object [] FindMethod( object [] argsIn, out MethodCacheItem cacheItem )
		{
			// ******
			var newArgs = Arguments.PrependArgs( argsIn, obj );

			// ******
			MethodBase mi;
			cacheItem = methodCache.GetCachedHandler( objType, methodName, newArgs );
			if( null != cacheItem ) {
				var args = MatchArgs( new List<MethodBase> { cacheItem.MethodInfo }, newArgs, out mi );
				if( null != args ) {
					return args;
				}
			}

			// ******
			Tuple<MethodBase, object [], List<MethodBase>> result = methodExtensions.FindExtensionMethod( objType, methodName, newArgs );	//, MatchArgs2 );
			if( null != result && null != result.Item1 ) {
				cacheItem = methodCache.Generate( null, objType, methodName, result.Item1 as MethodInfo, newArgs );
				return result.Item2;
			}
			
			// ******
			//
			// never returns
			//
			ThreadContext.MacroError( "unable to locate an implementation of \"{0}\" whose parameters match (or can be converted from) \"{1}\"\nTried matching:\n{2}", methodName, Arguments.ObjectsTypeNames( argsIn ), Arguments.GetMethodSignatures( result.Item3 ) );
			return null;
		}

		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			// ******
			//MethodBase mi = null;
			//var args =  FindMethod( argsIn, out mi );

			//// ******
			//return MethodInvoker.MethodInvoke( obj, mi, args );

			MethodCacheItem cacheItem;
			var args = FindMethod( argsIn, out cacheItem );

			try {
				//return handler.Invoke( obj, args );
				return MethodInvoker.BaseMethodInvoke( cacheItem, obj, args );
			}
			catch( Exception ex ) {
				throw ExceptionHelpers.CreateException( ex, "exception invoking \"{0}\" on \"{1}\"", methodName, obj.GetType().Name );
			}

		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodExtensionInvoker( object obj, string methodname, ExtensionTypeDictionary methodExtensions )
		{
			this.obj = obj;
			this.objType = obj.GetType();
			this.methodName = methodname;
			this.methodExtensions = methodExtensions;
		}

	}

}
