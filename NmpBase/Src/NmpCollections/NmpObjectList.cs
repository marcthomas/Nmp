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
using System.Linq;
using System.Text;


namespace NmpBase {

	/////////////////////////////////////////////////////////////////////////////

	[Serializable()] 
	public class NmpObjectList : List<object> {


		/////////////////////////////////////////////////////////////////////////////

		public void Add( IEnumerable<object> collection )
		{
			// ******
			foreach( object obj in collection ) {
				Add( obj );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public object PeekNextArg()
		{
			// ******
			if( 0 == Count ) {
				return null;
			}
			
			// ******
			return this[ 0 ];
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public object NextArg()
		{
			//
			// treating StringList like a stack and "popping" from the head
			// of the list
			//
			
			// ******
			if( 0 == Count ) {
				return null;
			}
			
			// ******
			object arg = this[ 0 ];
			RemoveAt( 0 );
			
			// ******
			return arg;
		}
		
		
		///////////////////////////////////////////////////////////////////////////////
		
		public object NextArg( object defValue )
		{
			object result = NextArg();
			return null == result ? defValue : result;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		[DebuggerStepThrough]
		public NmpObjectList()
		{
		}
		

		/////////////////////////////////////////////////////////////////////////////

		[DebuggerStepThrough]
		public NmpObjectList( IEnumerable<object> collection )
		{
			Add( collection );
		}


	}

}
