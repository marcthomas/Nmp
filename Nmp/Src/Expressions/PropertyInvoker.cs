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
using System.Reflection;

using NmpBase;
using Nmp;

using ReflectionHelpers;

namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	class PropertyInvoker : Invoker {

		ObjectInfo info;

		/////////////////////////////////////////////////////////////////////////////

		//public static object Invoke( ObjectInfo objInfo )
		//{
		//	// ******
		//	MethodInfo mi = objInfo.MemberAs<MethodInfo>();
		//	if( objInfo.IsStatic && !mi.IsStatic ) {
		//		ThreadContext.MacroError( "attempt to call non static property \"{0}\" using a static reference to \"{1}\"", mi.Name, mi.ReflectedType.FullName );
		//	}
		//	return MethodInvoker.MethodInvoke( objInfo.Object, mi, new object [ 0 ] );
		//}

		public static object Invoke( ObjectInfo objInfo )
		{
			// ******
			var cacheItem = MethodInvoker.FindPropertyMethod( objInfo );
			try {
				//var result = handler.Invoke( objInfo.Object, null );
				//return result;

				return MethodInvoker.BaseMethodInvoke( cacheItem, objInfo.Object, new object [0] );
			}
			catch( Exception ex ) {
				throw ExceptionHelpers.CreateException( ex, "exception invoking property \"{0}\" on \"{1}\"", objInfo.MemberName, objInfo.ObjectType.Name );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( info );
		}


		/////////////////////////////////////////////////////////////////////////////

		public PropertyInvoker( ObjectInfo objInfo )
		{
			info = objInfo;
		}

		/////////////////////////////////////////////////////////////////////////////

		public PropertyInvoker( object obj, string name )
			: this( new ObjectInfo(obj, name) )
		{
		}


	}
}
