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

using NmpBase;
using Nmp;


namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	class DynamicMethodInvoker : Invoker {

		object obj;
		string methodName;


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( object obj, string methodName, object [] argsIn )
		{
			// ******
			INmpDynamic dyn = obj as INmpDynamic;
			if( null != dyn && dyn.HasMethod( methodName ) ) {
				return dyn.GetMethodValue( methodName, argsIn );
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( obj, methodName, argsIn );
		}


		/////////////////////////////////////////////////////////////////////////////

		public DynamicMethodInvoker( object obj, string methodName )
		{
			this.obj = obj;
			this.methodName = methodName;
		}

	}
}
