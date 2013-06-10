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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


//#pragma warning disable 414


using NmpBase;
using Nmp;
using ReflectionHelpers;




namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	abstract class Invoker : ArgumentMatcher {

		protected static MethodCache methodCache = new MethodCache { };


		/////////////////////////////////////////////////////////////////////////////

		//
		// abstract method
		//

		abstract public object Invoke( object [] args );


		/////////////////////////////////////////////////////////////////////////////

		//
		// calls abstract method above
		//

		public object Invoke()
		{
			return Invoke( new object [ 0 ] );
		}


	}

}
