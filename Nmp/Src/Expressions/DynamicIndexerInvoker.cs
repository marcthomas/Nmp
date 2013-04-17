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


namespace Nmp.Expressions {


	/////////////////////////////////////////////////////////////////////////////

	class DynamicIndexerInvoker : Invoker {

		object obj;
		string indexerName;


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( object obj, string indexerName, object [] argsIn )
		{
			// ******
			INmpDynamic dyn = obj as INmpDynamic;
			if( null != dyn && dyn.HasIndexer( indexerName ) ) {
				return dyn.GetIndexerValue( indexerName, argsIn );
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( obj, indexerName, argsIn );
		}


		/////////////////////////////////////////////////////////////////////////////

		public DynamicIndexerInvoker( object obj, string indexerName )
		{
			this.obj = obj;
			this.indexerName = indexerName;
		}

	}
}
