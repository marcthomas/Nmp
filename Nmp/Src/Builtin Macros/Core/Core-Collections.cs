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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

using Microsoft.JScript;
using Microsoft.JScript.Vsa;


using NmpBase;
using Nmp;
using Global;


#pragma warning disable 618

namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {


		/////////////////////////////////////////////////////////////////////////////
		//
		// #.newStringList
		//
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a new string list
		/// </summary>
		/// <param name="objs"> Any number of objects to be added to the list when it is created</param>
		/// <returns>The list as an object</returns>

		[Macro]
		public object newStringList( params object [] objs )
		{
			return  new NmpStringList( objs );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #.newObjectList
		//
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a new object list
		/// </summary>
		/// <param name="objs"> Any number of objects to be added to the list when it is created</param>
		/// <returns>The list as an object</returns>

		[Macro]
		public object newObjectList( params object [] objs )
		{
			return  new NmpObjectList( objs );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #.newArray
		//
		/////////////////////////////////////////////////////////////////////////////

		private static NmpArray CheckForJSON( string str )
		{
			// ******
			for( int i = 0; i < str.Length; i++ ) {
				char ch = str[ i ];

				if( char.IsWhiteSpace(ch) ) {
					continue;
				}

				if( '{' == ch ) {
					//
					// the Substring() removes the leading white space
					//
					//
					try {
						return NmpArray.BuildArray( str.Substring(i) );
					}
					catch ( NmpJSONException ex ) {
						ThreadContext.MacroError( ex.Message );
					}
				}

				// ******
				//
				// no leading '{'
				//
				break;
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates an Nmp array which is a string object dictionary
		/// 
		/// The strings that can be passed to the method can be in one of three formats.
		/// + key = value (or key : value)
		///  + added as a key/value pair
		/// + key
		///  + added as key with an empty value
		/// + json
		///  + atempts to parse the string as json, on success is appended to the array, this
		///  implies the json look like: "name" : [ ... ] or "name" : { ... }
		/// </summary>
		/// <param name="strs">Any number of strings</param>
		/// <returns>object that is the Nmp array</returns>

		[Macro]
		public object newArray( params string [] strs )
		{
			// ******
			var array = new NmpArray();
			var chars = new char [] { ':', '=' };

			foreach( string str in strs ) {
				string s = null;

				// ******
				var arrayResult = CheckForJSON( str );
				if( null != arrayResult ) {
					array.AppendArray( arrayResult );
				}
				else {
					s = str;

					int index = s.IndexOfAny( chars );
					if( index > 0 && index < s.Length - 1 ) {
						string strValue = s.Substring( 1 + index );

						arrayResult = CheckForJSON( strValue );

						array.Add( s.Substring(0, index), null == arrayResult ? (object) strValue : (object) arrayResult );
					}
					else {
						array.Add( s, string.Empty );
					}
				}
			}

			// ******
			return array;
		}


		
	}


}
