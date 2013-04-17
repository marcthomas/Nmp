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


namespace Nmp.Expressions {


	/////////////////////////////////////////////////////////////////////////////

	class PropertyInvoker : Invoker {

		ObjectInfo info;


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( ObjectInfo objInfo )
		{
			// ******
			MethodInfo mi = objInfo.MemberAs<MethodInfo>();

			try {
				return mi.Invoke( objInfo.Object, null );
			}
			catch ( Exception ex ) {
				//
				// never returns
				//
				ThreadContext.MacroError( ExceptionHelpers.RecursiveMessage( ex, "error invoking property getter \"{0}\"", objInfo.MemberName ) );
				return null;
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
