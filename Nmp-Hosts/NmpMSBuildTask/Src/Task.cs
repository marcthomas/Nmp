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
using Nmp;

#pragma warning disable 414

namespace NmpMSBuild {


	/////////////////////////////////////////////////////////////////////////////


	public sealed class NmpTask : Task {

		// ******
		private const string	RESPONSE_PATH			= "responsepath";

		// ******
		////NmpHostHelper		nmp;
		//NmpEvaluator		nmp;
		public bool			Errors = false;

		// ******
		//
		// sepparate multiple files with semi colons, files are loaded and processed
		// sequentialy
		//
		[Required]
		public string	SourceFiles	{ get; set; }

		// ******
		//
		// only a single file is allowed
		//
		[Required]
		public string	OutputFile		{ get; set; }
		
		// ******
		//
		// Defines="macro="text,more text";macro2;macro3="something"
		//
		public string	Defines			{ get; set; }
		

		/////////////////////////////////////////////////////////////////////////////
		
		private bool ProcessFiles( NmpEvaluator nmp )
		{
			// ******
			string [] fileList = SourceFiles.Split( ';' );
			if( 0 == fileList.Length ) {
				Log.LogError( "SourceFiles is empty", null );
				return false;
			}
			
			// ******
			int fileCount = 0;

			foreach( string fileName in fileList ) {
				if( ! File.Exists(fileName) ) {
					Log.LogError( "the source file \"{0}\" could not be located", fileName );
					return false;
				}
				
				// ******
				try {
					string result = nmp.Evaluate( nmp.GetFileContext( fileName ), true );

					// ******
					if( ! Errors && result.Length > 0 ) {
						if( 0 == fileCount++ ) {
							File.WriteAllText( OutputFile, result );
						}
						else {
							File.AppendAllText( OutputFile, result );
						}
					}
				}
				catch ( Exception ex ) {
	//
	// report error here
	//
	#if EXREPORTING
	// ref dll
	#endif
					Log.LogErrorFromException( ex );
					return false;
				}
			}

			// ******
			return true;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		private bool SetOutput()
		{
			// ******
			try {
				//StreamWriter	outFile = null;
				//outFile = File.CreateText( OutputFile );
				//
				//// ******
				//if( null == outFile ) {
				//	Log.LogError( "unable to create file \"{0}\"", OutputFile );
				//	return false;
				//}
				File.WriteAllText( OutputFile, string.Empty );
			}
			catch ( Exception ex ) {
				Log.LogErrorFromException( ex );
				return false;
			}
			
			// ******
			return true;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		private bool SetDefines( NmpEvaluator nmp )
		{
			// ******
			string startDir = Directory.GetCurrentDirectory();
			nmp.AddTextMacro( RESPONSE_PATH, startDir, null );


			// ******
			if( string.IsNullOrEmpty(Defines) ) {
				return true;
			}
		
			// ******
			NmpStringList defines = Utility.SplitDefines( Defines );
			
			foreach( string define in defines ) {
				string	key;
				string	value;
				
				if( Utility.GetValuePair(define, out key, out value) ) {
					nmp.AddTextMacro( key, value, null );
				}
				else {
					Log.LogError( "error in Defines argument: {0}", Defines );
					return false;
				}
			}
			
			// ******
			return true;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public override bool Execute()
		{
			// ******
			try {
				////nmp = new NmpHostHelper( this );
				//nmp = new NmpEvaluator( new NmpHostHelper(this) );

				// ******
				using( var nmp = new NmpEvaluator(new NmpHostHelper(this)) ) {
					if( !SetDefines(nmp) ) {
						return false;
					}

					// ******
					if( !SetOutput() ) {
						return false;
					}

					// ******
					return ProcessFiles( nmp );
				}
			}
			
			catch ( Exception ex ) {
				Log.LogErrorFromException( ex );
				return false;
			}
			
			finally {
				//if( null != outFile ) {
				//	outFile.Close();
				//}
			}
		}
	

		/////////////////////////////////////////////////////////////////////////////
		
		public NmpTask()
		{
		}
		
		
	}


}