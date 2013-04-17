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



using NmpBase;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	partial class GrandCentral {


		/////////////////////////////////////////////////////////////////////////////

		private static string Extract( string searchStr, Regex rx )
		{
			//
			// match the first occurance of a regular expression and return it,
			// or return the first subexpression (if there is one)
			//
			
			// ******
			try {
				Match match = rx.Match( searchStr );
				if( match.Success ) {
					/*
							group[0] represents the overall capture
							
							group[1] .. group[n]	represent sub expresion captures from the outermost to the inner
																		most "()" pair
					*/
				
					if( match.Groups.Count > 1 ) {
						//
						// if there are subexpressions then we return the first one
						//
						return match.Groups[1].Value;
					}
					else {
						return match.Value;
					}
				}
			}
			catch ( ArgumentException e ) {
				//parser.Error( "internal MatchEx: {0}", e.Message );
				//throw new HelperException( string.Format("internal MatchEx: {0}", e.Message) );
				throw new Exception( string.Format("internal MatchEx: {0}", e.Message) );
			}
			
			// ******
			return string.Empty;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public string FindFile( string name )
		{
			// ******
			if( ! FileHelpers.IsValidFileName(name) ) {
				return null;
			}

			// ******
			//if( Path.IsPathRooted(name) && File.Exists(name) ) {
			//	//
			//	// found using full path name
			//	//
			//	return name;
			//}
			
			if( Path.IsPathRooted(name) ) {
				if( File.Exists(name) ) {
					//
					// found using full path name
					//
					return name;
				}

				//
				// full path, NOT found
				//
				return null;
			}


			// ******
			//
			// first try the default path
			//   
			string defaultDirectory = GetDirectoryStack().Peek();
			string fileName = Path.Combine( defaultDirectory, name );

			if( File.Exists(fileName) ) {
				return fileName;
			}

			// ******
			//
			// then the SearchPath
			//
			NmpStringList searchPaths = GetSearchPaths();
			if( searchPaths.Count > 0 ) {
				foreach( string path in searchPaths ) {
					//
					// combine path with name an if the file exists we have acheived success
					//
					fileName = Path.Combine( path, name );
					if( File.Exists(fileName) ) {
						return fileName;
					}
				}
			}
			
			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ReadFile( string fileNameIn, out string filePathOut, Regex regex = null )
		{

			bool fixText = true;

			// ******
			if( string.IsNullOrEmpty(fileNameIn) ) {
				throw new ArgumentNullException( "fileName" );
			}
			
			// ******
			filePathOut = FindFile( fileNameIn );
			if( null == filePathOut ) {
				//
				// not found
				//
				return null;
			}

			// ******
			string fileContents = File.ReadAllText( filePathOut );
			if( null == regex ) {
				return fixText ? FixText(fileContents) : fileContents;
			}

			// ******
			string partialContents = Extract(fileContents, regex);
			return fixText ? FixText(partialContents) : partialContents;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ReadFile( string fileNameIn )
		{
			string filePathOut;
			return ReadFile( fileNameIn, out filePathOut, null );
		}



	}

}