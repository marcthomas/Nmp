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
using System.Text.RegularExpressions;

using System.Globalization;
using System.IO;
using System.Reflection;


using NmpBase;
using Nmp;


namespace Nmp.Expressions {

	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// 	NMP Object helper for string arrays
	/// </summary>
	///
	/// <remarks>
	/// 	Jpm, 3/26/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////

	class StringArrayObjectHelper {

		string [] theArray;


		const int MaxParseItems = 1024;

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Joins all the strings in the array with 'joinString'.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="joinStr">
		/// 	The join string.
		/// </param>
		///
		/// <returns>
		/// 	The string result of the joinn.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public string Join( string joinStr )
		{
			// ******
			int count = 0;
			StringBuilder sb = new StringBuilder();

			// ******
			foreach( string s in theArray ) {
				if( count++ > 0 ) {
					sb.Append( joinStr );
				}
				sb.Append( s );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// ctor
		//
		/////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Constructor.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <exception cref="ArgumentException">
		/// 	Thrown when one or more arguments have unsupported or illegal values.
		/// </exception>
		///
		/// <param name="theArray">
		/// 	Array of thes.
		/// </param>
		////////////////////////////////////////////////////////////////////////////

		public StringArrayObjectHelper( object theArray )
		{
			// ******
			if( ! typeof(string []).Equals(theArray.GetType()) ) {
				throw new ArgumentException("expected string [] argument", "theArray");
			}

			// ******
			this.theArray = theArray as string[];
		}


		/////////////////////////////////////////////////////////////////////////////

		// Func<object, object>

		////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 	Creates this object.
		/// </summary>
		///
		/// <remarks>
		/// 	Jpm, 3/26/2011.
		/// </remarks>
		///
		/// <param name="strArray">
		/// 	The string.
		/// </param>
		///
		/// <returns>
		/// 	.
		/// </returns>
		////////////////////////////////////////////////////////////////////////////

		public static object Create( object strArray )
		{
			return new StringArrayObjectHelper( strArray );
		}




	}


}
