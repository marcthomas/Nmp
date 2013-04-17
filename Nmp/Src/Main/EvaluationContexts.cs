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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using NmpBase;


namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	public class EvaluationContext : IEvaluationContext {

		// ******
		public bool		FromFile	{ get; set; }
		public string	FileName	{ get; set; }
		public string FileNamePart { get; set; }
		public string FilePathPart { get; set; }

		// ******
		public string	Text			{ get; set; }


		/////////////////////////////////////////////////////////////////////////////

		public void SetFileInfo( string fullFilePath )
		{
			FileName = fullFilePath;
			FileNamePart = Path.GetFileName( fullFilePath );
			FilePathPart = Path.GetDirectoryName( fullFilePath );
		}


		/////////////////////////////////////////////////////////////////////////////

		public EvaluationContext()
		{
			FromFile = false;
			FileName = string.Empty;
			FileNamePart = string.Empty;
			FilePathPart = string.Empty;
			Text = string.Empty;
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	class EvaluateFromFileContext : EvaluationContext {

		/////////////////////////////////////////////////////////////////////////////

		public EvaluateFromFileContext( NMP nmp, string fullFilePath )
		{
			// ******
			if( ! File.Exists(fullFilePath) ) {
				throw new FileNotFoundException( fullFilePath );
			}

			// ******
			FromFile = true;
			SetFileInfo( fullFilePath );
			
			// ******
			//Text = File.ReadAllText( fullFilePath );
			Text = nmp.GrandCentral.ReadFile( fullFilePath );
		}


	}


	/////////////////////////////////////////////////////////////////////////////

	class EvaluateStringContext : EvaluationContext {

		/////////////////////////////////////////////////////////////////////////////

		public EvaluateStringContext( NMP nmp, string text )
		{
			Text = nmp.GrandCentral.FixText( text );
		}


	}


}
