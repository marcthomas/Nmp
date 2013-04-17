#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// JSONVisualizer.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Security.Permissions;


////#pragma warning disable 169


namespace NmpBase {

/*

"tom" : 1.e4,
"dick" : {
  "harry" : [ 
	  1, 2, 3
	]
}

*/

	/////////////////////////////////////////////////////////////////////////////

	class JSONVisualizer {

		int indent;
		StringBuilder		output;


		/////////////////////////////////////////////////////////////////////////////

		protected void Identifier( string id )
		{
			output.AppendFormat( "\"{0}\":", id );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void Value( object value )
		{
			// ******
			NmpArray array = value as NmpArray;
			if( null != array ) {
				WalkArray( array );
				return;
			}
			
			// ******
			NmpObjectList list = value as NmpObjectList;
			if( null != list ) {
				WalkList( list );
				return;
			}
			
			output.Append( JSONValue.Encode(value) );
			//output.Append( new string(' ', 2 * indent) );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void WalkList( NmpObjectList list )
		{
			// ******
			++indent;
			output.Append( '[' );
			
			// ******
			for( int index = 0; index < list.Count; index++ ) {
				if( index > 0 ) {
					output.Append( ',' );
				}
				Value( list[index] );
			}
			
			// ******
			output.Append( "]" );
			--indent;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void WalkArray( NmpArray array )
		{
			// ******
			++indent;
			output.Append( '{' );

			// ******
			var keyList = array.Keys;
			for( int index = 0; index < keyList.Count; index++ ) {
				if( index > 0 ) {
					output.Append( ',' );
				}

				string key = keyList[ index ];
				Identifier( key );
				Value( array[key] );
			}

			// ******
			output.Append( "}" );
			--indent;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string Array( NmpArray array )
		{
			// ******
			indent = 0;
			output = new StringBuilder();
			WalkArray( array );
			return output.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string List( NmpObjectList list )
		{
			// ******
			indent = 0;
			output = new StringBuilder();
			WalkList( list );
			return output.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public JSONVisualizer( JSONDateFormat dateFormat = JSONDateFormat.ISODate )
		{
			JSONValue.DateFormat = dateFormat;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string VisualizeArray( NmpArray array, JSONDateFormat dateFormat = JSONDateFormat.ISODate )
		{
			var x = new JSONVisualizer( dateFormat );
			return x.Array( array );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string VisualizeList( NmpObjectList list, JSONDateFormat dateFormat = JSONDateFormat.ISODate )
		{
			var x = new JSONVisualizer( dateFormat );
			return x.List( list );
		}


	}


//	/////////////////////////////////////////////////////////////////////////////
//
//
//	class funWithRefs {
//
//
//		public void a( ref string x )
//		{
//			int len = x.Length;
//
//			x = null;
//		}
//
//		public void b()
//		{
//			string s = "hi there";
//
//			a( ref s );
//
//			int len = s.Length;
//
//		}
//
//		public funWithRefs()
//		{
//			b();
//		}
//
//
//
//	}



}
