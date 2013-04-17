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
using System.Runtime.InteropServices;

using System.Globalization;
using System.IO;
using System.Reflection;

using System.Diagnostics;

namespace NmpBase {

	// D:\work\projects\common\src\utility\UPreferences.cpp


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Interface for accessing a data store that looks like an INI file
	/// </summary>

	public interface IPrefs {

		// ******
		bool		IsValid			{ get; }

		// ******
		bool		KeyExists( string section, string key );
		//bool		KeyExists( string key );

		string	GetString( string section, string key, string defStr );
		//string	GetString( string key, string defStr );

		void		WriteString( string section, string key, string fmt, params object [] args );
		//void		WriteString( string key, string fmt, params object [] args );

		// ******
		NmpStringList	GetSectionList();
		NmpStringList	GetKeyList( string section );

		// ******
		void		WriteSectionText( string section, string text );
		string	GetSectionText( string section );
		
		//
		// the first just adds keys to the current section, the second method adds the option of allowing
		// all entries in the section to be erased first - but you have to provide a section name
		//
		void		WriteKeysAndValues( string section, IDictionary dict );
		//void		WriteKeysAndValues( IDictionary dict );

		void		WriteKeysAndValues( bool clearSection, string section, IDictionary dict );
		//void		WriteKeysAndValues( bool clearSection, IDictionary dict );

		//
		// uses GetSectionText()
		//
		Dictionary<string, string>	GetKeysAndValues( string section );

	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class that wraps access to an INI file
	/// </summary>

	public partial class FilePrefs : IPrefs {

		// ******
		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString( string section, string key, string val, string filePath );

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString( string section, string key, string def, StringBuilder retVal, int size, string filePath );
		
		//
		// differs from GetPrivateProfileString() above by using an [in, out] char [] so we can get the nulls
		//																														 
		[DllImport("kernel32", EntryPoint = "GetPrivateProfileString" )]
		private static extern int GPPS( string section, string key, string def, [In, Out] char [] retVal, int size, string filePath );


		// ******
		// http://www.codeproject.com/KB/cs/cs_ini.aspx
		// http://www.pinvoke.net/default.aspx/kernel32/GetPrivateProfileString.html
		// http://www.pinvoke.net/default.aspx/kernel32/GetPrivateProfileSection.html

		// ******
		const int STD_INI_READ_SIZE	= 255;
		const string NO_VALUE_COMPARE = "_no_value";

		// ******
		string	iniFileName = string.Empty;
		///string	SECTION = string.Empty;


		/////////////////////////////////////////////////////////////////////////////

		public bool IsValid
		{
			get {
				if( string.IsNullOrEmpty(iniFileName) ) {
					return false;
				}

				// ******
				return File.Exists( iniFileName );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected int ValidLength( char [] buffer )
		{
			//
			// return length of text by locating first non-null character in buffer,
			// note that this allows null chars to be embeded within the string - which
			// we want (to allow)
			//
			for( int index = buffer.Length - 1; ; index-- ) {
				if( index < 0 || '\0' != buffer[index] ) {
					return ++index;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string [] GetSectionOrSectionNames( string section, string key, string defStr, string iniFile )
		{
			// ******
			int bufferSize = STD_INI_READ_SIZE;
			char [] buffer = new char[ bufferSize ];

			while( bufferSize - 1 == GPPS( section, key, defStr, buffer, bufferSize, iniFile ) ) {
				bufferSize *= 2;
				buffer = new char[ bufferSize ];
			}

			// ******
			string result = new string( buffer, 0, ValidLength(buffer) );
			string [] list = result.Split( '\0' );

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool KeyExists( string section, string key )
		{
			// ******
			//Contract.Requires( ! string.IsNullOrEmpty(section) );
			//Contract.Requires( ! string.IsNullOrEmpty(key) );

			// ******
			StringBuilder sb = new StringBuilder( STD_INI_READ_SIZE );
			GetPrivateProfileString( section, key, NO_VALUE_COMPARE, sb, STD_INI_READ_SIZE, iniFileName );

			// ******
			return NO_VALUE_COMPARE != sb.ToString();
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public bool KeyExists( string key )
		//{
		//	return KeyExists( SECTION, key );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public string GetString( string section, string key, string defStr )
		{
			// ******
			//Contract.Requires( ! string.IsNullOrEmpty(section) );
			//Contract.Requires( ! string.IsNullOrEmpty(key) );
			
			// ******
			if( null == defStr ) {
				defStr = string.Empty;
			}

			// ******
			int bufferSize = STD_INI_READ_SIZE;
			StringBuilder sb = new StringBuilder( bufferSize );
			
			while( bufferSize - 1 == GetPrivateProfileString( section, key, defStr, sb, bufferSize, iniFileName) ) {
				bufferSize *= 2;
				sb.EnsureCapacity( bufferSize );
			}

			// ******
			return sb.ToString();
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public string GetString( string key, string defStr )
		//{
		//	return GetString( SECTION, key, defStr );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public void WriteString( string section, string key, string fmt, params object [] args )
		{
			// ******
			//Contract.Requires( ! string.IsNullOrEmpty(section) );
			//Contract.Requires( ! string.IsNullOrEmpty(key) );
			
			// ******
			if( string.IsNullOrEmpty(key) ) {
				//
				// delete whole section (prime key)
				//
				WritePrivateProfileString( section, null, null, iniFileName );
			}
			else {
				if( null == fmt ) {
					//
					// deleting tag
					//
					WritePrivateProfileString( section, key, null, iniFileName );
				}
				else {
					string value = null == fmt ? null : string.Format( fmt, args );
					WritePrivateProfileString( section, key, value, iniFileName );
				}
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public void WriteString( string key, string fmt, params object [] args )
		//{
		//	WriteString( SECTION, key, fmt, args );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public NmpStringList GetSectionList()
		{
			return new NmpStringList( GetSectionOrSectionNames(null, null, string.Empty, iniFileName), false );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringList GetKeyList( string section )
		{
			return new NmpStringList( GetSectionOrSectionNames(section, null, string.Empty, iniFileName), false );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public NmpStringList GetKeyList()
		//{
		//	return new NmpStringList( GetSectionOrSectionNames(SECTION, null, string.Empty, iniFileName), false );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public void WriteSectionText( string section, string text )
		{
			Debug.Assert( false, "FilePrefs: WriteSectionText has not been implemented" );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetSectionText( string section )
		{
			Debug.Assert( false, "FilePrefs: GetSectionText has not been implemented" );
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void WriteKeysAndValues( string section, IDictionary dict )
		{
			foreach( DictionaryEntry de in dict ) {
				WriteString( section, de.Key.ToString(), de.Value.ToString() );
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public void WriteKeysAndValues( IDictionary dict )
		//{
		//	WriteKeysAndValues( SECTION, dict );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public void WriteKeysAndValues( bool clearSection, string section, IDictionary dict )
		{
			// ******
			if( clearSection ) {
				WriteString( section, null, null, iniFileName );
			}

			// ******
			WriteKeysAndValues( section, dict );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public void WriteKeysAndValues( bool clearSection, IDictionary dict )
		//{
		//	WriteKeysAndValues( clearSection, SECTION, dict );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public Dictionary<string, string> GetKeysAndValues( string section )
		{
			// ******
			Dictionary<string, string> dict = new Dictionary<string, string>();
			NmpStringList list = GetKeyList( section );

			// ******
			foreach( string key in list ) {
				if( ! string.IsNullOrEmpty(key) ) {
					string value = GetString(section, key, string.Empty );
					dict.Add( key, value );
				}
			}

			// ******
			return dict;
		}


		/////////////////////////////////////////////////////////////////////////////

		public FilePrefs( string iniFileName )
		{
			// ******
			//Contract.Requires( ! string.IsNullOrEmpty(iniFileName) );
			//Contract.Requires( File.Exists(iniFileName) );

			// ******
			this.iniFileName = iniFileName;
		}
			


	}


}

