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
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;

namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	[Serializable()] 
	public class NmpStringArray : Dictionary<string, string> {

		bool lowerCaseKey = false;


		/////////////////////////////////////////////////////////////////////////////

		public string FixKey( string key )
		{
			return (lowerCaseKey ? key.ToLower() : key).Trim();
		}


		/////////////////////////////////////////////////////////////////////////////

		public new void Add( string key, string value )
		{
			//base.Add( FixKey(key), null == value ? string.Empty : value );

			base[ FixKey(key) ] = null == value ? string.Empty : value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Append( NmpStringArray array )
		{
			if( null != array ) {
				foreach( var item in array ) {
					Add( item.Key, item.Value );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public new bool ContainsKey( string key )
		{
			return base.ContainsKey( FixKey(key) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Get( string key )
		{
			// ******
			string value = null;
			if( TryGetValue(FixKey(key), out value) ) {
				return value;
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public new void Remove( string key )
		{
			// ******
			if( base.ContainsKey(key = FixKey(key)) ) {
				base.Remove( key );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public new string this [ string key ]
		{
			get {
				return Get( key );
			}

			set {
				Add( key, value );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string LocateAndRemove( string key, bool ignoreCase )
		{
			// ******
			string value = null;
			
			// ******
			if( ignoreCase ) {
				key = key.ToLower();

				foreach( KeyValuePair<string, string> kvp in this ) {
					if( key == kvp.Key.ToLower() ) {
						Remove( key );
						value = kvp.Value;
					}
				}
			}
			else {
				if( base.TryGetValue(FixKey(key), out value) ) {
					Remove( key );
				}
			}

			// ******
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringArray AddSplitList( IEnumerable<string> list, bool trimValue, char [] splitChars = null )
		{
			// ******
			if( null == splitChars || 0 == splitChars.Length ) {
				splitChars = new char [] { ':', '=' };
			}

			// ******
			foreach( var str in list ) {
				if( string.IsNullOrEmpty(str) ) {
					continue;
				}

				// ******
				int pos = str.IndexOfAny( splitChars );
				
				if( pos < 0 ) {
					//
					// key = str, value = empty
					//
					Add( str, string.Empty );
				}

				else if( 0 == pos ) {
					if( str.Length > 1 ) {
						//
						// key = str, value = empty
						//
						Add( str.Substring(1), string.Empty );
					}
				}

				else {
					//
					// key = str before split char, value = str after split char
					//
					string value = str.Substring(1 + pos);
					if( trimValue ) {
						value = value.Trim();
					}

					Add( str.Substring(0, pos), value );
				}
			}

			// ******
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringArray( NmpStringList list, bool trimValue, char [] splitChars = null, bool lowerCaseKey = false )
		{
			this.lowerCaseKey = lowerCaseKey;
			AddSplitList( list, trimValue, splitChars );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringArray()
		{
			this.lowerCaseKey = false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringArray( bool lowerCaseKey )
		{
			this.lowerCaseKey = lowerCaseKey;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected NmpStringArray( SerializationInfo info, StreamingContext context )
			:	base(info, context)
		{
		}

	}

}
