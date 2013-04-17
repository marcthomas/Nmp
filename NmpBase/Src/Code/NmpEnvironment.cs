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
using System.IO;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using System.Reflection;
using Microsoft.Win32;

//using NmpBase;


namespace NmpBase {

	
	/////////////////////////////////////////////////////////////////////////////

	public class NmpEnvironment {

		//const string	USER_PATH_REG_LOCATION		= @"HKEY_CURRENT_USER\Environment";
		//const string	MACHINE_PATH_REG_LOCATION	= @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
		
		const string	PATH_VALUE_NAME						= @"Path";


		/////////////////////////////////////////////////////////////////////////////

		private static bool ItemInString( string paths, string path )
		{
			// ******
			if( null == paths ) {
				throw new ArgumentNullException( "paths" );
			}

			if( null == path ) {
				throw new ArgumentNullException( "path" );
			}

			// ******
			if( 0 == paths.Length ) {
				return false;
			}

			// ******
			paths = paths.ToLower();
			path = path.ToLower();
			int indexOf = paths.IndexOf( path );

			// ******
			if( indexOf < 0 ) {
				//
				// nope
				//
				return false;
			}

			// ******
			int indexNextChar = indexOf + path.Length;
			//
			// True if last substring in string OR followed by a semicolon
			//
			return paths.Length == indexNextChar || ';' == paths[indexNextChar ];
		}


		/////////////////////////////////////////////////////////////////////////////

		private static void AddString( string varName, EnvironmentVariableTarget target, string strIn )
		{
			// ******
			string oldValue = Environment.GetEnvironmentVariable( varName, target );
			if( null == oldValue ) {
				oldValue = string.Empty;
			}
			
			// ******
			if( ItemInString(oldValue, strIn) ) {
				//
				// already there
				//
				return;
			}

			// ******
			string newValue = string.Format( "{0}{1}{2}", oldValue, oldValue.EndsWith(";") ? string.Empty : ";", strIn );
			Environment.SetEnvironmentVariable( varName, newValue, target );
		}


		/////////////////////////////////////////////////////////////////////////////

		private static void RemoveString( string varName, EnvironmentVariableTarget target, string strIn )
		{
			// ******
			string oldValue = Environment.GetEnvironmentVariable( varName, target );
			if( null == oldValue ) {
				oldValue = string.Empty;
			}
			
			// ******
			if( ! ItemInString(oldValue, strIn) ) {
				//
				// not there
				//
				return;
			}

			// ******
			string [] strs = oldValue.Split( ';' );
			var sb = new StringBuilder();

			foreach( string str in strs ) {
				if( string.IsNullOrEmpty(str) || 0 == string.Compare(strIn, str, true) ) {
					//
					// ignore it
					//
				}
				else {
					sb.Append( str );
					sb.Append( ';' );
				}
			}

			// ******
			Environment.SetEnvironmentVariable( varName, sb.ToString(), target );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void AddStringToValue( string valueName, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process )
		{
			AddString( valueName, target, value );

			if( EnvironmentVariableTarget.Process != target ) {
				//
				// update process environment
				//
				AddString( valueName, EnvironmentVariableTarget.Process, value );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void RemoveStringFromValue( string valueName, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process )
		{
			RemoveString( valueName, target, value );

			if( EnvironmentVariableTarget.Process != target ) {
				//
				// update process environment
				//
				RemoveString( valueName, EnvironmentVariableTarget.Process, value );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void AddPathString( string pathItem, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process )
		{
			AddString( PATH_VALUE_NAME, target, pathItem );

			if( EnvironmentVariableTarget.Process != target ) {
				//
				// update process environment
				//
				AddString( PATH_VALUE_NAME, EnvironmentVariableTarget.Process, pathItem );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void RemovePathString( string pathItem, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process )
		{
			RemoveString( PATH_VALUE_NAME, target, pathItem );

			if( EnvironmentVariableTarget.Process != target ) {
				//
				// update process environment
				//
				RemoveString( PATH_VALUE_NAME, EnvironmentVariableTarget.Process, pathItem );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void SetValue( string valueName, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process )
		{
			Environment.SetEnvironmentVariable( valueName, value, target );

			if( EnvironmentVariableTarget.Process != target ) {
				//
				// update process environment
				//
				Environment.SetEnvironmentVariable( valueName, value, EnvironmentVariableTarget.Process );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string GetValue( string valueName, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process )
		{
			return Environment.GetEnvironmentVariable( valueName, target );
		}


	}

}

	
