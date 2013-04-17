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
//using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;

using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;


using NmpBase;

#pragma warning disable 162 


	public enum TargetType {
		Unknown				= 0,
		ExeFile				= 1,
		WinExeFile		= 2,
		LibraryFile		= 3,
		ModuleFile		= 4,

		Default = LibraryFile
	}

	public enum PlatformType {
		Unknown				= 0,
		AnyCpu				= 1,
		x86						= 2,
		Itanium				= 3,
		x64						= 4,

		Default = AnyCpu
	}


namespace NmpBase.Compile {



	/////////////////////////////////////////////////////////////////////////////

	public class CodeCompileRunner : IDisposable {


		public const string ResponseFileName		= "response.rsp";

		// ******
		public string ErrorMessage		{ get; private set; }
		
		// ******
		//protected Dictionary<string, Assembly>	AssemblyCache = new Dictionary<string, Assembly>();

		// ******
		//string defaultErrorFile = string.Empty;

		string tempFilesDir = string.Empty;
		bool keepTempFiles = false;

		CodeDomProvider codeProvider = null;
		
		IEnumerable<string> compilerOptions = null;


		///////////////////////////////////////////////////////////////////////////////
		//
		//TempFileCollection	TempFiles;
		//
		//public TempFileCollection GetTempFiles
		//{
		//	get {
		//		var tf = TempFiles;
		//		TempFiles = new TempFileCollection();
		//		return tf;
		//	}
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public void SetError( string message )
		{
			ErrorMessage = message == null ? string.Empty : message;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SetError()
		{
			SetError( null );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public Assembly GetAssemblyFromId( string assemblyId )
		//{
		//	// ******
		//	Assembly asm = null;
		//	if( AssemblyCache.TryGetValue(assemblyId, out asm) ) {
		//		return asm;
		//	}
		//
		//	// ******
		//	return null;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public static string [] NewStringArray( IList<string> strs )
		{
			string [] text = new string [ strs.Count ];
			for( int i = 0; i < text.Length; i++ ) {
				text[ i ] = strs[ i ];
			}
			return text;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string CreateResponseFile( IEnumerable<string> compilerOptions, IList<string> refAssemblies, IList<string> sources )
		{
			// ******
			bool debug = false;

			string fmt = string.Empty;

			if( debug ) {
				fmt = "{0,-20}{1,20}\n";
			}
			else {
				fmt = "{1}\n";
			}

			// ******
			var sb = new StringBuilder();

			foreach( var option in compilerOptions ) {
				sb.AppendLine( option );
			}

			foreach( var assembly in refAssemblies ) {
				sb.AppendFormat( "/r:\"{0}\"\n", assembly );
			}

			if( null == sources ) {
				sb.AppendLine( "#" );
				sb.AppendLine( "# input source code is text strings" );
				sb.AppendLine( "#" );
			}
			else {
				sb.AppendLine( "#" );
				sb.AppendLine( "# These files are passed to the compiler when CodeDomProvider.CompileAssemblyFromFile() is called," );
				sb.AppendLine( "# they are listed here (as comments) for debugging purposes." );
				sb.AppendLine( "#" );
				foreach( var fileName in sources ) {
					sb.AppendFormat( "# {0}\n", fileName );
				}
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		protected CompilerParameters SetCompilerOptions(	CodeDomProvider provider, 
																											string [] assemblies, 
																											string assemblyName,
																											IList<string> sources,
																											out string rspFile
																										)
		{
			// ******
			//CompilerParameters cp = new CompilerParameters( assemblies );
			CompilerParameters cp = new CompilerParameters();

			// ******
			//
			// outputPath overrides /out
			//
			// ??? friend assemblies (see /out docs)
			//
			if( string.IsNullOrEmpty(assemblyName) ) {
				cp.GenerateInMemory = true;
			}
			else {
				cp.GenerateInMemory = false;
				cp.OutputAssembly = assemblyName;
			}

			// ******
			cp.TempFiles = new TempFileCollection( tempFilesDir, keepTempFiles );

			// ******
			//
			// all compiler options to temp file
			//
			rspFile = Path.Combine( tempFilesDir, ResponseFileName );
			File.WriteAllText( rspFile, CreateResponseFile(compilerOptions, assemblies, sources) );

			cp.CompilerOptions = string.Format( "@\"{0}\"", rspFile );

			//
			// add to temp file list
			//
			//cp.TempFiles.AddFile( rspFile, keepTempFiles );

			// ******

			return cp;
		}

		/////////////////////////////////////////////////////////////////////////////

		public CompilerResults Compile(	IList<string> refAssemblies,
																		string assemblyName,
																		IList<string> filesIn,
																		IList<string> sourcesIn
																	)
		{
			// ******
			if( null == filesIn && null == sourcesIn ) {
				throw new Exception( "both 'filesIn' and 'sourcesIn' are null" );
			}

			if( null != filesIn && null != sourcesIn ) {
				throw new Exception( "only one of the input parameters 'filesIn' or 'sourcesIn' can be non null" );
			}

			// ******
			bool compileFiles = null == sourcesIn;

			// ******
			if( compileFiles ) {
				if( 0 == filesIn.Count ) {
					throw new Exception( "'filesIn' is emtpy" );
				}
			}
			else if( 0 == sourcesIn.Count ) {
				throw new Exception( "'sourcesIn' is emtpy" );
			}
				
			// ******
			//string [] assemblies = NewStringArray( null == refAssemblies ? CoreAssemblies(null) : refAssemblies );

			string [] assemblies = NewStringArray( CoreAssemblies(refAssemblies) );
			string [] code = NewStringArray( compileFiles ? filesIn : sourcesIn );

			// ******
			string responseFilePath;
			CompilerParameters compilerParameters = SetCompilerOptions(	codeProvider,
																																	assemblies, 
																																	assemblyName, 
																																	filesIn,
																																	out responseFilePath
																																);
			// ******
			CompilerResults compilerResults = null;
			try {
				if( compileFiles ) {
					compilerResults = codeProvider.CompileAssemblyFromFile( compilerParameters, code );
				}
				else {
					compilerResults = codeProvider.CompileAssemblyFromSource( compilerParameters, code );
				}
			}
			catch ( Exception ex ) {
				this.SetError( string.Format("C# compiler threw an exception: {0}", ex.Message) );
				return null;
			}
			
			// ******
			if( compilerResults.Errors.Count > 0 ) {
				var compileErrors = new StringBuilder();

				foreach( System.CodeDom.Compiler.CompilerError compileError in compilerResults.Errors ) {
					string fileName = compileError.FileName;
					if( string.IsNullOrEmpty(fileName) ) {
						fileName = responseFilePath;
					}

					// ******
					string err = string.Format( "{0}({1},{2}): error: {3}", fileName, compileError.Line, compileError.Column, compileError.ErrorText );
					compileErrors.AppendLine( err );
				}

				// ******
				this.SetError( compileErrors.ToString() );

				// ******
				return null;
			}

			// ******
			return compilerResults;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			//if( null != TempFiles ) {
			//	TempFiles.KeepFiles = false;
			//	TempFiles.Delete();
			//}
		}


		/////////////////////////////////////////////////////////////////////////////

		public CodeCompileRunner(	string tempFilesDir, 
															bool keepTempFiles,
															
															CodeDomProvider codeProvider,
															IEnumerable<string> compilerOptions
														)
		{
			// ******
			ErrorMessage = string.Empty;
			//AssemblyCache = new Dictionary<string, Assembly>();
			
			// ******
			//this.defaultErrorFile = defaultErrorFile;

			// ******
			if( string.IsNullOrEmpty(tempFilesDir) ) {
				this.tempFilesDir = Path.GetTempPath();
			}
			else {
				this.tempFilesDir = tempFilesDir;
			}
			
			// ******
			this.keepTempFiles = keepTempFiles;
			
			// ******
			if( null == codeProvider ) {
				throw new ArgumentNullException( "codeProvider" );
			}
			this.codeProvider = codeProvider;

			// ******
			if( null == compilerOptions ) {
				compilerOptions = DefCompilerOptions( TargetType.Default );
			}
			this.compilerOptions = compilerOptions;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string TargetTypeExtension( TargetType type )
		{
			// ******
			switch( type ) {
				case TargetType.ExeFile:
					return "exe";

				case TargetType.WinExeFile:
					return "winexe";

				case TargetType.ModuleFile:
					return "module";

		case TargetType.Unknown:

				//case TargetType.Default:
				case TargetType.LibraryFile:
				default:
					return "library";

			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string PlatformName( PlatformType type )
		{
			// ******
			switch( type ) {
				case PlatformType.x86:
					return "x86";

				case PlatformType.Itanium:
					return "itanium";

				case PlatformType.x64:
					return "x64";

			case PlatformType.Unknown:

				//case PlatformType.Default:
				case PlatformType.AnyCpu:
				default:
					return "anycpu";
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static NmpStringList DefCompilerOptions( TargetType tt )
		{
			var options = new NmpStringList();

			// ******
			//options.Add( "/platform:" + PlatformName(pt) );
			options.Add( "/target:" + TargetTypeExtension(tt) );

			// ******
			return options;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static NmpStringList CoreAssemblies( IList<string> moreAssemblies )
		{
			// ******
			var assemblies = new NmpStringList( unique : true );
			
			assemblies.Add( "System.dll" );
			assemblies.Add( "System.Core.dll" );
			assemblies.Add( "Microsoft.CSharp.dll" );

	/*

		things to watch out for:

			o		the compile (and razor) code is currently in Csx.exe but it will be
					moved to NmpBase when it is all smoothed out and running

						so, at some point DefaultRazorTemplateBase will NOT be in the
						same assembly as this code


	*/

//			assemblies.Add( Assembly.GetExecutingAssembly().CodeBase.Substring( 8 ) );
//
//			//
//			// this get NmpBase.dll
//			//
//			assemblies.Add( typeof(NmpStringList).Assembly.CodeBase.Substring( 8 ) );

			if( null != moreAssemblies ) {
				assemblies.AddRange( moreAssemblies );
			}

			// ******
			assemblies.Sort();
			return assemblies;
		}



	}

}
