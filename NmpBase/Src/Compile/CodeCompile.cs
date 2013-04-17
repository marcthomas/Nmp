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
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;


using NmpBase;

#pragma warning disable 162 


namespace NmpBase.Compile {


		/////////////////////////////////////////////////////////////////////////////

	public class CodeCompile {

		// ******
		public string ErrorMessage	{ get; private set; }

		bool		forceDebug;

		// ******
		public	string						TempFilesDir = string.Empty;
		public	bool							KeepTempFiles = true;


		/////////////////////////////////////////////////////////////////////////////

		private IEnumerable<string> ForceDebug( IEnumerable<string> options )
		{
			if( null == options ) {
				options = CodeCompileRunner.DefCompilerOptions( TargetType.Default );
			}

			// ******
			List<string> newOptions = new List<string>();

			foreach( var option in options ) {
				if( option.StartsWith("/debug", StringComparison.OrdinalIgnoreCase) ) {
					//
					// removes it
					//
				}
				else {
					newOptions.Add( option );
				}
			}

			newOptions.Add( "/debug:full" );
			newOptions.Add( "/optimize-" );

			// ******
			return newOptions;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected CompilerResults Compile(	CodeDomProvider codeProvider,
																				IEnumerable<string> compilerOptions,
																				IList<string> refAssemblies,
																				string assemblyName,
																				IList<string> filesIn,
																				IList<string> sourcesIn
																			)
		{
			// ******
			if( forceDebug ) {
				compilerOptions = ForceDebug( compilerOptions );
			}

	//
	// ??if want debug info then have to make sure code is passed in as a
	// file and library is on disk - currently no matter what we pass in
	// for debug we get "/debug-" in the xxx.cmdline file passed to the
	// compiler
	// 


			// ******
			using( var cb = new CodeCompileRunner(TempFilesDir, KeepTempFiles, codeProvider, compilerOptions) ) {
				var result = cb.Compile( refAssemblies, assemblyName, filesIn, sourcesIn );
				ErrorMessage = cb.ErrorMessage;
				return result;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public CompilerResults CompileFiles(	IList<string> refAssemblies,
																					IList<string> files,
																					string assemblyName,
																					IEnumerable<string> compilerOptions
																				)
		{
			//return Compile( new CSharpCodeProvider(), compilerOptions, refAssemblies, assemblyName, files, null );

			using( var provider = new CSharpCodeProvider() ) {
				return Compile( provider, compilerOptions, refAssemblies, assemblyName, files, null );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public CompilerResults CompileSource(	IList<string> refAssemblies,
																					IList<string> sources,
																					string assemblyName,
																					IEnumerable<string> compilerOptions
																				)
		{
			//return Compile( new CSharpCodeProvider(), compilerOptions, refAssemblies, assemblyName, null, sources );

			using( var provider = new CSharpCodeProvider() ) {
				return Compile( provider, compilerOptions, refAssemblies, assemblyName, null, sources );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public CompilerResults CompileCcu(	IList<string> refAssemblies,
																				CodeCompileUnit ccu,
																				string assemblyName,
																				IEnumerable<string> compilerOptions
																			)
		{
	//
	// why do we do this, can't we just use the CCU ??
	//
			string generatedCode = CSharpSourceFromCcu( ccu, true );
			
			//return Compile( new CSharpCodeProvider(), compilerOptions, refAssemblies, assemblyName, null, new List<string>() { generatedCode } );
			
			using( var provider = new CSharpCodeProvider() ) {
				return Compile( provider, compilerOptions, refAssemblies, assemblyName, null, new List<string>() { generatedCode } );
			}

		}


		/////////////////////////////////////////////////////////////////////////////

		public CodeCompile( bool forceDebug = false, string TempFilesDir = null, bool KeepTempFiles = false )
		{
			// ******
			ErrorMessage = string.Empty;
			this.forceDebug = forceDebug;

			// ******
			if( string.IsNullOrEmpty(TempFilesDir) ) {
				this.TempFilesDir = Path.GetTempPath();
			}
			else {
				this.TempFilesDir = TempFilesDir;
			}
			
			// ******
			this.KeepTempFiles = KeepTempFiles;
		}



		/////////////////////////////////////////////////////////////////////////////

		private static string RemoveLineHidden( string source )
		{
			return source.Replace( "#line hidden", string.Empty );
		}


		/////////////////////////////////////////////////////////////////////////////

		private static string SourceFromCcu( CodeDomProvider codeProvider, CodeCompileUnit ccu, bool removeLineHidden )
		{
			// ******
			CodeGeneratorOptions options = new CodeGeneratorOptions();

			// ******
			using( var writer = new StringWriter() ) {
				codeProvider.GenerateCodeFromCompileUnit( ccu, writer, options );
				string source = writer.ToString();
				return removeLineHidden ? RemoveLineHidden(source) : source;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string CSharpSourceFromCcu( CodeCompileUnit ccu, bool removeLineHidden )
		{
			//return SourceFromCcu(  new CSharpCodeProvider(), ccu, removeLineHidden );

			using( var provider = new CSharpCodeProvider() ) {
				return SourceFromCcu(  provider, ccu, removeLineHidden );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string VbSourceFromCcu( CodeCompileUnit ccu )
		{
			//return SourceFromCcu( new VBCodeProvider(), ccu, false );

			using( var provider = new VBCodeProvider() ) {
				return SourceFromCcu( provider, ccu, false );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static List<string> CoreAssemblies( IList<string> moreAssemblies )
		{
			return CodeCompileRunner.CoreAssemblies( moreAssemblies );
		}

	}
}
