#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// StringDictionary.cs
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

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;


//using System.Dynamic;

namespace NmpBase {

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// 	NMP's notion of an associative array, based on "Keys" that are strings
	/// 	and "Values" that are objects.
	/// </summary>
	///
	/// <remarks>
	/// 	Jpm, 3/26/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////

	[Serializable()] 
	public partial class NmpArray : NmpDynamicBase, ICollection, IEnumerable<KeyValuePair<string,object>>, ICloneable {

		// ******
		PropertyDictionary arrayProperties = null;
		PropertyDictionary userProperties = null;

		// ******
		OrderedDictionary	array = new OrderedDictionary();


		/////////////////////////////////////////////////////////////////////////////
		//
		// ICollection
		//
		/////////////////////////////////////////////////////////////////////////////

		public void CopyTo( Array arrayIn, int index )
		{
			array.CopyTo( arrayIn, index );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool 	IsSynchronized	{ get { return false; } }	//array.IsSynchronized; } }
		public object SyncRoot				{ get { return null; } }	//array.SyncRoot; } }


		/////////////////////////////////////////////////////////////////////////////
		//
		// IEnumerable
		//
		/////////////////////////////////////////////////////////////////////////////

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public IEnumerator<KeyValuePair<string,object>> GetEnumerator()
		{
			foreach( DictionaryEntry de in array ) {
				yield return new KeyValuePair<string, object>( (string) de.Key, de.Value );
			}
		}

		
		/////////////////////////////////////////////////////////////////////////////
		//
		// ICloneable
		//
		/////////////////////////////////////////////////////////////////////////////
		
		object ICloneable.Clone()
		{
			//return MemberwiseClone();
			return this.Clone();
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public NmpArray Clone()
		{
			// ******
			NmpArray clone = MemberwiseClone() as NmpArray;
			//clone.name += ".cloned";

			// ******
			//if( null != properties ) {
			//	clone.properties = properties.Clone();
			//}

			// ******
			//if( null != userProperties ) {
			//	clone.userProperties = userProperties.Clone();
			//}

			// ******
			return clone;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		//
		// NmpDynamicBase
		//
		/////////////////////////////////////////////////////////////////////////////

		public override NmpDynamicType HasMember( string memberName )
		{
			return HasProperty(memberName) ? NmpDynamicType.Property : NmpDynamicType.None;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override bool HasProperty( string propName )
		{
			return array.Contains( propName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override Type GetPropertyType( string propName )
		{
			// ******
			if( array.Contains(propName) ) {
				object obj = array[ propName ];
				return obj.GetType();
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object GetPropertyValue( string propName )
		{
			// ******
			if( array.Contains(propName) ) {
				return array[ propName ];
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override bool HasIndexer( string indexerName )
		{
			object item = GetPropertyValue( indexerName );
			return item is NmpArray;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object GetIndexerValue( string indexerName, object [] args )
		{
			// ******
			NmpArray item = GetPropertyValue( indexerName ) as NmpArray;
			if( null == item ) {
				return null;
			}

			// ******
			if( 0 == args.Length ) {
			}

			if( args.Length > 1 ) {
			}

			string key = args[0] as string;
			if( null == key ) {
			}

			// ******
			return item[ key ];
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// Methods that dupliate/forward to methods of 'array'
		//
		/////////////////////////////////////////////////////////////////////////////
		
		public int Count
		{
			get {
				return array.Count;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object this [ int index ]
		{
			get {
				if( index < 0 || index >= array.Count ) {
					throw new IndexOutOfRangeException( "index" );
				}

				// ******
				return array[ index ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object this [ string key ]
		{
			get {
				return array[ key ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringList Keys
		{
			get {
				var list = new NmpStringList();
				foreach( DictionaryEntry de in array ) {
					list.Add( (string) de.Key );
				}

				// ******
				return list;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpObjectList Values
		{
			get {
				var list = new NmpObjectList();
				foreach( DictionaryEntry de in array ) {
					list.Add( de.Value );
				}

				// ******
				return list;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Add( string key, object value )
		{
			// ******
			if( string.IsNullOrEmpty(key) ) {
				throw new ArgumentNullException( "key" );
			}

			// ******
			array[ key ] = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Set( string key, object value )
		{
			Add( key, value );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AppendItem( object item )
		{
			array[ array.Count.ToString() ] = item;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AppendArrayItems( params object [] args )
		{
			array[ array.Count.ToString() ] = args;	//new NmpStringList( args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpArray AddNewArray( string key )
		{
			var a = new NmpArray();
			Add( key, a );
			return a;
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public NmpObjectList AddNewObjectList( string key )
		{
			var l = new NmpObjectList();
			Add( key, l );
			return l;
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringList AddNewStringList( string key )
		{
			var l = new NmpStringList();
			Add( key, l );
			return l;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AddArray( string key, NmpArray nmpArray )
		{
			Add( key, nmpArray );
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public void AddObjectList( string key, NmpObjectList list )
		{
			Add( key, list );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AddStringList( string key, NmpStringList list )
		{
			Add( key, list );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Clear()
		{
			array.Clear();
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Contains( string key )
		{
			return array.Contains( key );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Insert( int index, string key, object value )
		{
			// ******
			if( index < 0 ) {
				index = 0;
			}
			else if( index > array.Count ) {
				index = array.Count;
			}

			// ******
			array.Insert( index, key, value );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Remove( string key )
		{
			array.Remove( key );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void RemoveAt( int index )
		{
			// ******
			if( index < 0 || index >= array.Count ) {
				throw new IndexOutOfRangeException( "index" );
			}

			// ******
			array.RemoveAt( index );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// NmpArray native methods
		//
		/////////////////////////////////////////////////////////////////////////////

		public Type GetObjectType( int index )
		{
			// ******
			if( index < 0 || index >= array.Count ) {
				throw new IndexOutOfRangeException( "index" );
			}

			// ******
			return array[ index ].GetType();
		}


		/////////////////////////////////////////////////////////////////////////////

		public Type GetObjectType( string key )
		{
			// ******
			object objType = array[ key ];
			return null == objType ? null : objType.GetType();
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public void AddItems( params object [] args )
		{
			// ******
			int last = args.Length & ~1;

			for( int i = 0; i < last; i += 2 ) {
				string key = args[ i ].ToString();
				object value = args[ 1 + i ];
				Add( key, value );
			}
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public void SetArrayProperty( string key, string value )
		{
			// ******
			if( string.IsNullOrEmpty(key) ) {
				return;
			}

			// ******
			if( null == arrayProperties ) {
				arrayProperties = new PropertyDictionary();
			}
			
			// ******
			arrayProperties[ key.ToLower() ] = string.IsNullOrEmpty(value) ? string.Empty : value;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public string GetArrayProperty( string key )
		{
			// ******
			if( string.IsNullOrEmpty(key) ) {
				return string.Empty;
			}

			// ******
			if( null == arrayProperties ) {
				arrayProperties = new PropertyDictionary();
			}
			
			// ******
			return arrayProperties[ key ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SetUserProperty( string key, string value )
		{
			// ******
			if( string.IsNullOrEmpty(key) ) {
				return;
			}

			// ******
			if( null == userProperties ) {
				userProperties = new PropertyDictionary();
			}
			
			// ******
			userProperties[ key.ToLower() ] = string.IsNullOrEmpty(value) ? string.Empty : value;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public string GetUserProperty( string key )
		{
			// ******
			if( string.IsNullOrEmpty(key) ) {
				return string.Empty;
			}

			// ******
			if( null == userProperties ) {
				userProperties = new PropertyDictionary();
			}
			
			// ******
			return userProperties[ key ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return ToJSONConverter.ConvertArray( this );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Visualize()
		{
			return JSONVisualizer.VisualizeArray( this );
		}


		/////////////////////////////////////////////////////////////////////////////

		public Dictionary<string, string> GetAsStringDictionary()
		{
			// ******
			var dict = new Dictionary<string, string>();

			// ******
			foreach( KeyValuePair<string, object> kvp in this ) {
				dict.Add( kvp.Key, kvp.Value.ToString() );
			}
		
			// ******
			return dict;
		} 


		/////////////////////////////////////////////////////////////////////////////
		
		public void AppendArray( NmpArray value )
		{
			foreach( KeyValuePair<string, object> kvp in value ) {
				try {
					array[ kvp.Key ] = kvp.Value;
				}
				catch {
				}
			}
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public void AppendDictionary( IDictionary dict )
		{
			foreach( string key in dict.Keys ) {
				try {
					array[ key ] = dict[ key ];
				}
				catch {
				}
			}
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public NmpArray( NmpArray array )
		{
			AppendArray( array );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		
		public NmpArray( IDictionary dict )
		{
			AppendDictionary( dict );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		
		public NmpArray()
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public static NmpArray BuildArray( WarningCall warning, string jsonText )
		{
			// ******
			var builder = new JSONArrayBuilder( warning );
			var parser = new JSONParser( builder, jsonText );

			// ******
			parser.Parse();
			return builder.Array;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static NmpArray BuildArray( string jsonText )
		{
			return BuildArray( null, jsonText );
		}


	}




}
