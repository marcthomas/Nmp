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
using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace NmpBase {



	/////////////////////////////////////////////////////////////////////////////

	public class KVPStringList : KeyValueList<string, KVP> {
	}


	/////////////////////////////////////////////////////////////////////////////

	public class KVPObjectList : KeyValueList<object, KVP> {
	}


	/////////////////////////////////////////////////////////////////////////////

	public class KVP	{ }


	/////////////////////////////////////////////////////////////////////////////

	public class BoolFlag {

		public bool	Flag { get; set; }

	}


	/////////////////////////////////////////////////////////////////////////////

	public class IntProp {

		public int IntValue { get; set; }

	}


	/////////////////////////////////////////////////////////////////////////////

	public class KVPValue<T, K> where K: class, new() {

		public string	Key		{ get; set; }
		public T			Value	{ get; set; }

		public K			Additional { get; set; }


		/////////////////////////////////////////////////////////////////////////////

		public KVPValue( string key, T value, K stuff = null )
		{
			Key = key;
			Value = value;
			Additional = null == stuff ? new K() : stuff;
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	// list of key/value pairs where the key is always a string
	// T is the type of the value
	// K is the type of a class that provides additional context


	public class KeyValueList<T, K> : List<KVPValue<T, K>> where K: class, new()	{


		/////////////////////////////////////////////////////////////////////////////

		public void Add( string key, T value, K stuff = null )
		{
			Add( new KVPValue<T, K>( key, value, stuff ) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public KVPValue<T, K> Find( string key )
		{
			return Find( item => key == item.Key );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Contains( string key )
		{
			return null != Find( key );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		// already implemented by List<>
		//
		//public KVPValue<T> this [ int index ]
		//{
		//	get {
		//		return Value( index );
		//	}
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public KVPValue<T, K> this [ string key ]
		{
			get {
				return Key( key );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public KVPValue<T, K> Key( string key )
		{
			return Find( key );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Key( int index )
		{
			// ******
			if( index < 0 || index >= Count ) {
				throw new ArgumentOutOfRangeException( "index" );
			}

			return base[index].Key;
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Value( string key )
		{
			var kvp = Find( key );
			return null == kvp ? default(T) : kvp.Value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Value( int index )
		{
			// ******
			if( index < 0 || index >= Count ) {
				throw new ArgumentOutOfRangeException( "index" );
			}

			return base[index].Value;
		}


	}

}
