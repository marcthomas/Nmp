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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NmpBase {



	///////////////////////////////////////////////////////////////////////////////
	//
	//public class ReverseEnumerator<T> : IEnumerable<T> {
	//
	//	NmpStack<T> items;
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	IEnumerator IEnumerable.GetEnumerator() {
	//			return this.GetEnumerator();
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public IEnumerator<T> GetEnumerator() {
	//		for( int i = 0; i < items.Count; i++ ) {
	//			yield return items[ i ];
	//		}
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public ReverseEnumerator( NmpStack<T> items )
	//	{
	//		this.items = items;
	//	}
	//
	//}
	//


	/////////////////////////////////////////////////////////////////////////////

	public class ReverseEnumerator<T> : IEnumerable<T> {

		IList items;


		/////////////////////////////////////////////////////////////////////////////

		IEnumerator IEnumerable.GetEnumerator() {
				return this.GetEnumerator();
		}


		/////////////////////////////////////////////////////////////////////////////

		public IEnumerator<T> GetEnumerator() {
			for( int i = 0; i < items.Count; i++ ) {
				yield return (T) items[ i ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public ReverseEnumerator( IList items )
		{
			this.items = items;
		}

	}



}
