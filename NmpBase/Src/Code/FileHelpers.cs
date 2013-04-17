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

namespace NmpBase {


	///////////////////////////////////////////////////////////////////////////
	
	public static class FileHelpers {


		///////////////////////////////////////////////////////////////////////////

		public static bool HasValidFileNameChars( string fileName )
		{
			if( string.IsNullOrEmpty(fileName) ) {
				return true;
			}
			return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
		}


		///////////////////////////////////////////////////////////////////////////

		public static bool HasValidPathChars( string fileName )
		{
			if( string.IsNullOrEmpty(fileName) ) {
				return true;
			}
			return fileName.IndexOfAny(Path.GetInvalidPathChars()) < 0;
		}


		///////////////////////////////////////////////////////////////////////////

		public static bool IsValidFileName( string fileName )
		{
			// ******
			if( HasValidPathChars(Path.GetDirectoryName(fileName)) ) {
				return HasValidFileNameChars( Path.GetFileName(fileName) );
			}

			// ******
			return false;
		}



	}


}
