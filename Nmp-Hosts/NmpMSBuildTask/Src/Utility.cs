#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
//
// Task.cs
//
using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;


using NmpBase;

namespace NmpMSBuild {


	public static class Utility {
	
	
		/////////////////////////////////////////////////////////////////////////////
		
		public static string RemoveOuterQuotes( string str )
		{
			// ******
			if( string.IsNullOrEmpty(str) ) {
				return string.Empty;
			}
			
			// ******
			if( str.Length > 1 ) {
				if( '"' == str[0] && '"' == str[str.Length - 1] ) {
					str = str.Substring( 1, str.Length - 2 );
				}
			}
			
			// ******
			str = str.Replace( "\\\"", "\"" );
			
			// ******
			return str;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public static bool GetValuePair( string value, out string lhs, out string rhs )
		{
			// ******
			lhs = rhs = string.Empty;
			
			// ******
			string [] parts = null;
			try {
				parts = value.Split( new char [] { '=' } );
			}
			catch {
				return false;
			}
			
			// ******
			if( 0 == parts.Length || string.IsNullOrEmpty(parts[0]) ) {
				return false;
			}
			else if( 1 == parts.Length ) {
				lhs = RemoveOuterQuotes(parts[ 0 ]);
				return true;
			}
			
			// ******
			lhs = RemoveOuterQuotes(parts[ 0 ]);
			rhs = RemoveOuterQuotes(parts[ 1 ]);
			return true;
		}

		/////////////////////////////////////////////////////////////////////////////
		
		public static NmpStringList SplitDefines( string defines )
		{
			// ******
			//
			// macro;macro="something";macro="some,this;and;that";
			//
			
			// no semi-colons in defines, reserved for separating macro defs
			
			string [] sa = defines.Split( ';' );
			return new NmpStringList( sa );
		}


	}
	
	
	
}