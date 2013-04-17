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

namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	[Serializable()] 
	public class PropertyDictionary : ICloneable {

		Dictionary<string, string> properties = new Dictionary<string, string>();

		/////////////////////////////////////////////////////////////////////////////

		object ICloneable.Clone()
		{
			return this.Clone();
		}


		/////////////////////////////////////////////////////////////////////////////

		public PropertyDictionary Clone()
		{
			return MemberwiseClone() as PropertyDictionary;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string this [ string key ]
		{
			get
			{
				return properties [ key.ToLower() ];
			}

			set
			{
				properties [ key.ToLower() ] = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Add( string key, string value )
		{
			if( !string.IsNullOrEmpty( key ) ) {
				properties [ key.ToLower() ] = value;
			}
		}

	}
}
