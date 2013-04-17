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
using NmpExpressions;

namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////

	//public  class FileSysMacros {
	//
	//	IMacroProcessor mp;

	partial class CoreMacros {

		/////////////////////////////////////////////////////////////////////////////

		public object nmpAssemblyPath
		{
			get {
				if( gc.Security.RestrictRead ) {
					ThreadContext.MacroError( "attempt to retreive directory name when Security.RestrictRead is set" );
				}
				
				// ******
				return LibInfo.CodeBasePath;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object currentDirectory
		{
			get {
				if( gc.Security.RestrictRead ) {
					ThreadContext.MacroError( "attempt to retreive directory name when Security.RestrictRead is set" );
				}
				
				// ******
				string result = gc.GetDirectoryStack().Peek();
				return result.ToLower();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object defpath
		{
			get {
				return currentDirectory;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object currentFile
		{
			get {
				return Get<InvocationStack>().CurrentSourceFile();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object parentFile
		{
			get {
				return Get<InvocationStack>().ParentSourceFile();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object nofile()
		{
			GetNMP().NoOutputFile = true;
			return string.Empty;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public object setOutputExtension( string ext )
		{
			GetNMP().OutputFileExt = ext;
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets the text encoding for the output file
		/// 
		/// Currently only the command line host pays attention to this
		/// 
		/// </summary>
		/// <param name="encoding">
		/// 
		/// Values can be:
		/// 
		///		ascii
		///		utf7 
		///		utf8 
		///		utf32 
		///		unicode  (16 bit)
		///		
		///		</param>
		/// <returns></returns>

		public object setOutputEncoding( string encoding )
		{
			GetNMP().OutputEncoding = encoding;
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #loadmacros( `fileName' )
		//
		/////////////////////////////////////////////////////////////////////////////
		//
		//[Macro("#loadMacros", new Type[] {typeof(string)}, new object[] {true} )]
		//public object loadMacros( IMacro macro, ArgumentList argList, MacroArguments macroArgs )
		//{
		//	// ******
		//	if( gc.Security.RestrictRead ) {
		//		ThreadContext.MacroError( "macro #loadmacros: attempt to include file when Security.RestrictRead is set" );
		//	}
		//
		//	// ******
		//	//var args = new MacroArgs( mp ).Initialize( argList );
		//	//string fileName = (string) args[ 0 ];
		//
		//	// ******
		//	var args = new MacroArgs( macro.MacroProcessor ).AsTuples(argList) as NmpTuple<string, bool>;
		//
		//	// ******
		//	string path = FileReader.FindFile( args.Item1 );
		//	if( null == path ) {
		//		ThreadContext.MacroError( "macro #loadmacros: could not locate file: \"{0}\"", args.Item1 );
		//	}
		//
		//	// ******
		//	AutoRegisterMacros.RegisterMacros( mp, path, args.Item2 );
		//
		//	// ******
		//	return string.Empty;
		//}
		//


		//public object loadMacros( string dllPath )
		//{
		//	// ******
		//	if( gc.Security.RestrictRead ) {
		//		ThreadContext.MacroError( "macro #loadMacros: attempt to include file when Security.RestrictRead is set" );
		//	}

		//	// ******
		//	string path = gc.FindFile( dllPath );
		//	if( null == path ) {
		//		ThreadContext.MacroError( "macro #loadMacros: could not locate file: \"{0}\"", dllPath );
		//	}
			
		//	// ******
		//	string filePathOut;
		//	string text = Get<GrandCentral>().ReadFile( dllPath, out filePathOut  );
			
		//	//mp.AddTextMacro( macroName, text, null );

		//	// ******
		//	AutoRegisterMacros.RegisterMacros( mp, path,  );

		
		//	// ******
		//	return string.Empty;
		//}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public object include( string fileName )
		{
			// ******
			if( gc.Security.RestrictRead ) {
				ThreadContext.MacroError( "macro #include: attempt to include file when Security.RestrictRead is set" );
			}

			// ******
			//IInput input = mp.CurrentMacroArgs.Input;
			//if( null != input ) {
			//	input.IncludeFile( fileName );
			//}

			IInput input = mp.CurrentMacroArgs.Input;
			if( null != input ) {
				string fullFilePath;
				string text = gc.ReadFile( fileName, out fullFilePath );

				if( null == text ) {
					ThreadContext.MacroError( "include could not locate file: \"{0}\"", fileName );
				}
			
				input.IncludeText( text, fullFilePath );
			}






			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object readFile( string macroName, string fileName, string regExStr )
		{
			// ******
			if( gc.Security.RestrictRead ) {
				ThreadContext.MacroError( "attempt to read file when Security.RestrictRead is set" );
			}
			
			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				ThreadContext.MacroError( "read file: expected a macro name in which to place the file contents" );
			}

			//if( ! mp.ScanHelper.IsValidMacroIdentifier(macroName) ) {
			if( ! Get<IRecognizer>().IsValidMacroIdentifier(macroName) ) {
				ThreadContext.MacroError( "read file: \"{0}\" is not a valid macro name", macroName );
			}

			Regex regex = null;
			if( ! string.IsNullOrEmpty(regExStr) ) {
				try {
					regex = new Regex( regExStr );
				}
				catch ( Exception ex ) {
					ThreadContext.MacroError( "read file: error initializing RegEx with \"{0}\": {1}", regExStr, ex.Message );
				}
			}

			// ******
			string filePathOut;
			string text = gc.ReadFile( fileName, out filePathOut, regex );
			mp.AddTextMacro( macroName, text, null );

			// ******
			return string.Empty;
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public FileSysMacros( IMacroProcessor mp )
		//{
		//	this.mp = mp;
		//}
		//

	}


}
