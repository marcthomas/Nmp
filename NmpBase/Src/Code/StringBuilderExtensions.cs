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
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;




namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// String builder extension methods
	/// </summary>

	public static class StringBuilderExtensions {

		/////////////////////////////////////////////////////////////////////////////

		public static StringBuilder PrependText( this StringBuilder sbIn, string text )
		{
			// ******
			var lines = sbIn.ToString().Split( SC.NEWLINE );
			var sb = new StringBuilder();

			// ******
			foreach( var line in lines ) {
				sb.Append( text );
				sb.Append( line );
				sb.Append( '\n' );
			}

			// ******
			return sb;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static StringBuilder PrependTextInPlace( this StringBuilder sb, string text )
		{
			// ******
			var lastIndex = sb.Length - 1;
			for( var index = lastIndex; index >= 0; index-- ) {
				if( index < lastIndex ) {
					if( SC.NEWLINE == sb [ index ] ) {
						sb.Insert( 1 + index, text );
					}
					else if( 0 == index ) {
						sb.Insert( 0, text );
					}
				}
			}

			// ******
			return sb;
		}
	}


}
