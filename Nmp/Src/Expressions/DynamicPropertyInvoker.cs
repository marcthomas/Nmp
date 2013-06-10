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

	class DynamicPropertyInvoker : Invoker {

		object obj;
		string propName;


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( object obj, string propName )	//, object [] argsIn )
		{
			// ******
			INmpDynamic dyn = obj as INmpDynamic;
			if( null != dyn && dyn.HasProperty( propName ) ) {
				return dyn.GetPropertyValue( propName );
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( obj, propName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public DynamicPropertyInvoker( object obj, string propName )
		{
			this.obj = obj;
			this.propName = propName;
		}

	}
}
