#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// ToJSONConverter.cs
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

	/////////////////////////////////////////////////////////////////////////////

	class ToJSONConverter {

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
			
			// ******
			output.Append( JSONValue.Encode(value) );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void WalkList( NmpObjectList list )
		{
			// ******
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
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void WalkArray( NmpArray array )
		{
			// ******
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
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string Array( NmpArray array )
		{
			// ******
			output = new StringBuilder();
			WalkArray( array );
			return output.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string List( NmpObjectList list )
		{
			// ******
			output = new StringBuilder();
			WalkList( list );
			return output.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string ConvertArray( NmpArray array )
		{
			var x = new ToJSONConverter();
			return x.Array( array );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string ConvertList( NmpObjectList list )
		{
			var x = new ToJSONConverter();
			return x.List( list );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ToJSONConverter( JSONDateFormat dateFormat = JSONDateFormat.ISODate )
		{
			JSONValue.DateFormat = dateFormat;
		}


	}


}
