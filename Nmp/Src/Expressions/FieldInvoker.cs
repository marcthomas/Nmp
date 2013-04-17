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

	class FieldInvoker : Invoker {

		ObjectInfo objInfo;


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( ObjectInfo objInfo )
		{
			var fieldInfo = objInfo.MemberAs<FieldInfo>();
			return null == fieldInfo ? null : fieldInfo.GetValue( objInfo.Object );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			try {
				return Invoke( objInfo );
			}
			catch ( Exception ex ) {
				//
				// never returns
				//
				ThreadContext.MacroError( ExceptionHelpers.RecursiveMessage( ex, "error invoking indexer \"{0}\"", objInfo.MemberName ) );
				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public FieldInvoker( ObjectInfo objInfo )
		{
			this.objInfo = objInfo;
		}

	}
}
