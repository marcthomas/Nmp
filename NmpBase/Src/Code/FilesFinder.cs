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


	///////////////////////////////////////////////////////////////////////////////
	//
	//public class FileGroup : IEnumerable<string> {
	//
	//	int relativeDirIndex = 0;
	//	NmpStringList	filePaths = new NmpStringList();
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public int 						RelativeDirectoryIndex	{ get { return relativeDirIndex; } }
	//	public NmpStringList	Files										{ get { return filePaths; } }
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//	//
	//	// IEnumerable
	//	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	IEnumerator IEnumerable.GetEnumerator()
	//	{
	//		return this.GetEnumerator();
	//	}
	//
	//	
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public IEnumerator<string> GetEnumerator()
	//	{
	//		foreach( string path in filePaths ) {
	//			yield return path;
	//		}
	//	}
	//
	//	
	//	/////////////////////////////////////////////////////////////////////////////
	//	//
	//	// additional iterators
	//	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public EnumerationWrapper<string> ByRelativePath
	//	{
	//		get {
	//			return new EnumerationWrapper<string>( filePaths, item => GetRelativePath(item) );
	//		}
	//	}
	//
	//	
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public EnumerationWrapper<string> ByFileName
	//	{
	//		get {
	//			return new EnumerationWrapper<string>( filePaths, item => Path.GetFileName(item) );
	//		}
	//	}
	//
	//	
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public string GetFullPath( int index )
	//	{
	//		// ******
	//		if( index < 0 || index >= filePaths.Count ) {
	//			throw new IndexOutOfRangeException( "index" );
	//		}
	//
	//		// ******
	//		return filePaths[ index ];
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public string GetFileName( int index )
	//	{
	//		return Path.GetFileName( GetFullPath(index) );
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public string GetRelativePath( string path )
	//	{
	//		// ******
	//		if( relativeDirIndex <= 0 ) {
	//			return Path.GetFileName( path );
	//		}
	//
	//		/*
	//
	//			in the spirit of keeping this simple were just going to split up the
	//			path and return the parts we need rather than parse the path ourself
	//
	//		*/
	//
	//		// ******
	//		int partsToSkip = relativeDirIndex;
	//		string [] parts = path.Split( '\\' );
	//
	//		if( parts.Length <= partsToSkip ) {
	//			throw new Exception( "too few path componenets in file name!" );
	//		}
	//
	//		// ******
	//		var sb = new StringBuilder();
	//		for( int i = partsToSkip, trueIndex = 0; i < parts.Length; i++, trueIndex++ ) {
	//			if( trueIndex > 0 ) {
	//				sb.Append( '\\' );
	//			}
	//			sb.Append( parts[i] );
	//		}
	//
	//		// ******
	//		return sb.ToString();
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public string GetRelativePath( int index )
	//	{
	//		
	//		// ******
	//		return GetRelativePath( GetFullPath(index) );
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public void Add( string path )
	//	{
	//		filePaths.Add( path );
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public FileGroup( int relDirIndex )
	//	{
	//		relativeDirIndex = relDirIndex;
	//	}
	//}
	//

	/////////////////////////////////////////////////////////////////////////////

	public class FilesFinder {

		const char DirectorySeparatorChar			= '\\';
		const char AltDirectorySeparatorChar	= '/';
		const char VolumeSeparatorChar				= ':';

		const char VERT_BAR			= '|'; 
		const char REGEX_TOGGLE	= '#';


		/////////////////////////////////////////////////////////////////////////////

		protected class PathPart {

			public PathPart	Next;
			public string		Name;
			public bool			IsDirectoryName;

			public List<string>	FilePatterns = new List<string>();

		}

		/////////////////////////////////////////////////////////////////////////////
		
		protected class PathParts : List<PathPart> {

			
			public int relDirIndex = 0;

		
			/////////////////////////////////////////////////////////////////////////////

			public new void Add( PathPart part )
			{
				if( Count > 0 ) {
					this[ Count - 1 ].Next = part;
				}
				base.Add( part );
			}

		}


		/////////////////////////////////////////////////////////////////////////////

		protected string EndWithSeparator( string str )
		{
			// ******
			if( string.IsNullOrEmpty(str) ) {
				return @"\";
			}

			// ******
			char ch = str[ str.Length - 1 ];
			if( DirectorySeparatorChar == ch || AltDirectorySeparatorChar == ch ) {
				return str;
			}

			// ******
			return str + "\\";
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string GetFullPath( string pathIn, string primeDir, string altDir )
		{
const char EXCLAMATION_POINT = '!';

			// ******
			if( string.IsNullOrEmpty(pathIn) ) {
				return null;
			}

			// ******
			string baseDir;

			if( EXCLAMATION_POINT == pathIn[0] ) {
				pathIn = pathIn.Substring( 1 );
				baseDir = altDir;
			}
			else {
				baseDir = primeDir;
			}

			Trace.Assert( ! string.IsNullOrEmpty(baseDir), "base directory is empty!" );

			//if( string.IsNullOrEmpty(baseDir) ) {
			//	//
			//	// if not passed a directory then use the current one
			//	//
			//	// ?? should we allow this ??
			//	//
			//	baseDir = Directory.GetCurrentDirectory();
			//	CSX.WriteWarning( "FilesFinder.GetFullPath called with an empty base directory, using default directory: \"{0}\"", baseDir );
			//}

			// ******
			baseDir = EndWithSeparator( baseDir );
			
			// ******
			char ch1 = pathIn[ 0 ];
			char ch2 = pathIn.Length > 1 ? pathIn[ 1 ] : SC.NO_CHAR;

			if( VolumeSeparatorChar == ch2 ) {
				//
				// "x:" full path including drive
				//
				if( 2 == pathIn.Length ) {
					//
					// need "x:\"
					//
					return pathIn + "\\";
				}

				// ******
				char ch3 = pathIn[ 2 ];
				if( DirectorySeparatorChar == ch3 || AltDirectorySeparatorChar == ch3 ) {
					//
					// "x:\" or "x:/"
					//
					return pathIn;
				}

				// ******
				//
				// add a backslash after volume separator so we get "x:\ ..."
				//
				return pathIn.Substring(0, 2) + "\\" + pathIn.Substring( 2 );
			}
			else if( DirectorySeparatorChar == ch1 || AltDirectorySeparatorChar == ch1 ) {
				//
				// "\" or "/" - root of current directory
				//
				return Directory.GetDirectoryRoot(baseDir) + pathIn.Substring( 1 );
			}

			// ******
			//
			// okay, we have a relative directory
			//
			if( '.' != pathIn[0] || (pathIn.Length > 1 && char.IsLetterOrDigit(pathIn[1])) ) {
				//
				// does not start with a dot OR starts with a dot followed by a letter or number
				//
				return baseDir + pathIn;
			}

			// ******
			//
			// okay, we've got at least on dot !
			//
			var sb = new StringBuilder();
			var input = new StringIndexer( pathIn );

			//
			// get all the dots, fwd slashes, and back slashes
			//
			while( ! input.Empty ) {
				char ch = input.Peek();

				if( SC.DOT == ch || DirectorySeparatorChar == ch || AltDirectorySeparatorChar == ch ) {
					sb.Append( input.NextChar() );
				}
				else {
					break;
				}
			}

			// ******
			//
			// use GetFullPath() to "fix" the path
			//
			string dir = Path.GetFullPath( baseDir + sb.ToString() );

			// ******
			return dir + input.Remainder;
		}


		/////////////////////////////////////////////////////////////////////////////

		/*
			regex:	#...#
							#...
							#...#\
							\#...#\
							\#...
		*/

		/////////////////////////////////////////////////////////////////////////////

		protected void ParsePatterns( List<string> list, string text )
		{
			list.AddRange( text.Split(';') );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected PathParts ParseName( PathParts parts, StringIndexer input )
		{
			// ******
			var part = new PathPart();
			int partIndex = parts.Count;
			parts.Add( part );

			// ******
			var sb = new StringBuilder();

			for( int index = 0; ! input.Empty; index++ ) {
				char ch = input.NextChar();

				// ******
				if( 0 == index ) {
					//
					// regex check
					//
				}

				// ******
				switch( ch ) {
					case DirectorySeparatorChar:
					case AltDirectorySeparatorChar:
						ParseName( parts, input );
						break;
						
					case VERT_BAR:
						//
						// vertical bar indicates that this directory entry
						// should be the start of a relative path - if more
						// than one vertical bar is used then it's the last
						// one that "wins"
						//
						//int tempCount = parts.Count;
						//ParseName( parts, input );
						//parts.relDirIndex = parts.Count - tempCount - 1;
						
						parts.relDirIndex = parts.Count;
						ParseName( parts, input );
						break;

					default:
						sb.Append( ch );
						break;
				}
			}

			// ******
			part.Name = sb.ToString();
			//
			// if we're NOT the last item in the list then we're
			// a directory entry - remember ParseName() is recursive
			// and this code will not be entered until the entire
			// path has been split, and it's the last part of the
			// path that contains the file patterns
			//
			part.IsDirectoryName = part != parts[ parts.Count - 1 ];

			if( ! part.IsDirectoryName ) {
				//
				// file pattern(s) at end of path
				//
				ParsePatterns( part.FilePatterns, sb.ToString() );
			}

			// ******
			return parts;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void GetFiles( DirectoryInfo di, PathPart part, FileGroup fileGroup )
		{
			// ******
			if( null == part.Next ) {
				//
				// look for files, not directories
				//
				if( null == di ) {
					throw new ArgumentNullException( "di" );
				}

				// ******
				//
				// each pattern
				//
				foreach( var pattern in part.FilePatterns ) {
					//
					// check the directory
					//
					foreach( var file in di.EnumerateFiles(pattern) ) {
						//
						// add matching files
						//
						fileGroup.Add( file.FullName );
					}
				}

				// ******
				return;
			}

			// ******
			//
			// directories
			//
			//
			
			if( 2 == part.Name.Length && "**" == part.Name ) {
				//
				// all subdirectories, note we're advancing 'part' until
				// we find the last instance which contains the file patterns
				// to look for - we do this because we are currently lazy and
				// don't check for "**" when we're parsing the path - which we
				// should do, and toss an error if "**\" is the not the last
				// directory part of the path
				//

				while( null != part.Next ) {
					part = part.Next;
				}
				
				//
				// "**" indicates recursive search INCLUDING the current directory (the
				// directory preceeding the "\**" in the path)
				//
				GetFiles( di, part, fileGroup );

				foreach( var dir in di.EnumerateDirectories("*", SearchOption.AllDirectories) ) {
					GetFiles( dir, part, fileGroup );
				}
			}
			
			else {
				//
				// only the specific directories matching the name or
				// pattern in part.Name - does not include files in the
				// current directory
				//
				foreach( var dir in di.EnumerateDirectories(part.Name) ) {
					GetFiles( dir, part.Next, fileGroup );
				}
			}

		}


		/////////////////////////////////////////////////////////////////////////////

		protected void GetFiles( PathPart part, FileGroup fileGroup )
		{
			var driveInfo = new DriveInfo( part.Name );
			GetFiles( driveInfo.RootDirectory, part.Next, fileGroup );
		}


		/////////////////////////////////////////////////////////////////////////////

		public FileGroup FindFiles( string filePattern, string rootDir, string altDir )
		{
			// ******
			string path = GetFullPath( filePattern, rootDir, altDir );

			// ******
			var nameParts = ParseName( new PathParts(), new StringIndexer(path) );
			var fileGroup = new FileGroup( nameParts.relDirIndex );

			//foreach( var part in nameParts ) {
			//	CSX.WriteLine( "  isDirectoryPart: {0}", part.IsDirectoryName );
			//	CSX.WriteLine( "  name:            {0}", part.Name );
			//	CSX.WriteLine( "" );
			//}
			//
			//CSX.WriteLine( "  relDirIndex: {0}", nameParts.relDirIndex );
			//CSX.WriteLine( "" );

			// ******
			GetFiles( nameParts[0], fileGroup );

			// ******
			return fileGroup;
		}


		/////////////////////////////////////////////////////////////////////////////

		public FilesFinder()
		{
		}
	}


}
