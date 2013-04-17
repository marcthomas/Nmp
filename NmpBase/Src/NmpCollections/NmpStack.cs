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



	/////////////////////////////////////////////////////////////////////////////

	public class NmpStack<T> : List<T>, IEnumerable<T> where T : class {

		// stack for Ouput - retrofit Input


		/////////////////////////////////////////////////////////////////////////////

		public bool	Empty			{ get { return 0 == Count; } }
		public bool	NotEmpty	{ get { return Count > 0; } }

		/////////////////////////////////////////////////////////////////////////////

		IEnumerator IEnumerable.GetEnumerator()
		{
				return this.GetEnumerator();
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets items from the stack with the top of stack being the first item
		/// in the collection
		/// </summary>
		/// <returns></returns>

		public new IEnumerator<T> GetEnumerator()
		{
			for( int i = Count - 1; i >= 0; i-- ) {
				yield return base[ i ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the items from the stack with the bottom of the stack (oldest item)
		/// being the first item returned
		/// </summary>
		/// <returns></returns>

		public ReverseEnumerator<T> ReverseEnumerator()
		{
			return new ReverseEnumerator<T>( this );
		}


		/////////////////////////////////////////////////////////////////////////////

		private T TOS( out int index )
		{
			// ******
			if( Count < 1 ) {
				throw new Exception( "stack is empty" );
			}

			// ******
			//
			// top of stack is always the last item in the list
			//
			index = Count - 1;

			//
			// we return the index so "Pop" has the index to remove
			//
			return this [ index ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Push( T item )
		{
			this.Add( item );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool TryPeek( int i, out T item )
		{
			// ******
			if( Count < 1 || i >= Count  ) {
				item = null;
				return false;
			}

			// ******
			int index = Count - i - 1;
			item = this[ index ];
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Peek( int i )
		{
			// ******
			if( Count < 1 || i >= Count  ) {
				throw new IndexOutOfRangeException();
			}

			// ******
			int index = Count - i - 1;
			return this [ index ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Peek()
		{
			// ******
			int index;
			return TOS( out index );
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Pop()
		{
			// ******
			int index;
			T item = TOS( out index );

			// ******
			RemoveAt( index );

			// ******
			return item;
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStack()
		{
		}

	}
}
