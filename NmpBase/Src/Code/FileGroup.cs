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
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Globalization;
using System.IO;
using System.Reflection;

using System.Security.Permissions;

//using Csx;


namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	public class FileGroup : IEnumerable<string> {

		int relativeDirIndex = 0;
		NmpStringList	filePaths = new NmpStringList();


		/////////////////////////////////////////////////////////////////////////////

		public int 						RelativeDirectoryIndex	{ get { return relativeDirIndex; } }
		public NmpStringList	Files										{ get { return filePaths; } }


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

		public IEnumerator<string> GetEnumerator()
		{
			foreach( string path in filePaths ) {
				yield return path;
			}
		}

		
		/////////////////////////////////////////////////////////////////////////////
		//
		// additional iterators
		//
		/////////////////////////////////////////////////////////////////////////////

		public EnumerationWrapper<string> ByRelativePath
		{
			get {
				return new EnumerationWrapper<string>( filePaths, item => GetRelativePath(item) );
			}
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public EnumerationWrapper<string> ByFileName
		{
			get {
				return new EnumerationWrapper<string>( filePaths, item => Path.GetFileName(item) );
			}
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public string GetFullPath( int index )
		{
			// ******
			if( index < 0 || index >= filePaths.Count ) {
				throw new IndexOutOfRangeException( "index" );
			}

			// ******
			return filePaths[ index ];
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetFileName( int index )
		{
			return Path.GetFileName( GetFullPath(index) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetRelativePath( string path )
		{
			// ******
			if( relativeDirIndex <= 0 ) {
				return Path.GetFileName( path );
			}

			/*

				in the spirit of keeping this simple were just going to split up the
				path and return the parts we need rather than parse the path ourself

			*/

			// ******
			int partsToSkip = relativeDirIndex;
			string [] parts = path.Split( '\\' );

			if( parts.Length <= partsToSkip ) {
				throw new Exception( "too few path componenets in file name!" );
			}

			// ******
			var sb = new StringBuilder();
			for( int i = partsToSkip, trueIndex = 0; i < parts.Length; i++, trueIndex++ ) {
				if( trueIndex > 0 ) {
					sb.Append( '\\' );
				}
				sb.Append( parts[i] );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetRelativePath( int index )
		{
			
			// ******
			return GetRelativePath( GetFullPath(index) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Add( string path )
		{
			filePaths.Add( path );
		}


		/////////////////////////////////////////////////////////////////////////////

		public FileGroup( int relDirIndex )
		{
			relativeDirIndex = relDirIndex;
		}
	}


}
