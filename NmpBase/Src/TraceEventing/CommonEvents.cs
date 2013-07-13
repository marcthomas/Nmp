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
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;



namespace NmpBase {

	/////////////////////////////////////////////////////////////////////////////

	public struct ConversionError {

		public Type FromType { get; set; }
		public Type ConvertToType { get; set; }

		/////////////////////////////////////////////////////////////////////////////

		public static void Write( Type from, Type to )
		{
			var evt = new ConversionError {
				FromType = from,
				ConvertToType = to
			};
			EventWriter.Error( evt );
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	//public static class CommonEventExtensions {

	//	///////////////////////////////////////////////////////////////////////////////

	//	//public static void ConversionError( this Type fromType, Type convertTo )
	//	//{
	//	//	ConversionError.Write( fromType, convertTo );
	//	//}


	//}
}
