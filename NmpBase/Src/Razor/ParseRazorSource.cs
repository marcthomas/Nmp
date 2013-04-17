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

//using System.Globalization;
using System.IO;
//using System.Reflection;

using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser.SyntaxTree;


namespace NmpBase.Razor {


	/////////////////////////////////////////////////////////////////////////////

	public static class ParseRazorSource {

		///////////////////////////////////////////////////////////////////////////////
		//
		//private static string GetBaseClassName()
		//{
		//	return typeof( DefaultRazorTemplateBase ).Name;	//FullName;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		private static string GetSafeCodeNamespace()
		{
			return "__RasorHost";
		}


		/////////////////////////////////////////////////////////////////////////////

		private static string GetSafeClassName()
		{
			return "_" + Guid.NewGuid().ToString().Replace( "-", "_" );
		}


		/////////////////////////////////////////////////////////////////////////////

		private static void CreateHostHandler( RazorEngineHost host )
		{
			host.GeneratedClassContext = new GeneratedClassContext(

					"Execute",										// executeMethodName

					"Write",											// writeMethoName

					"WriteLiteral",								// writeLiteralMethodName

					"HelperResult.Write",					// string writeToMethodName,

					"HelperResult.WriteLiteral",	// string writeLiteralToMethodName,

					"HelperResult",								// string templateTypeName;

					"DefineSection"								// string defineSectionMethodName
				);
		}


		/////////////////////////////////////////////////////////////////////////////

		private static RazorTemplateEngine CreateHost(	RazorCodeLanguage codeLanguage, 
																										string defBaseClassName,
																										string defNamespace, 
																										string defClassName,
																										IList<string> refNamespaces,
																										Action<RazorEngineHost,Action<RazorEngineHost>> createHostHandler
																									)
		{
			// ******
			RazorEngineHost host = new RazorEngineHost( codeLanguage );

			// ******
			if( string.IsNullOrEmpty(defBaseClassName) ) {
				//defBaseClassName = GetBaseClassName();
				throw new ArgumentNullException( "defBaseClassName" );
			}

			if( string.IsNullOrEmpty(defNamespace) ) {
				defNamespace = GetSafeCodeNamespace();
			}

			if( string.IsNullOrEmpty(defClassName) ) {
				defClassName = GetSafeClassName();
			}

			// ******
			host.DefaultBaseClass = defBaseClassName;
			host.DefaultClassName = defClassName;
			host.DefaultNamespace = defNamespace;

			// ******
			host.NamespaceImports.Add( "NmpBase.Razor" );

			if( null != refNamespaces ) {
				foreach( var ns in refNamespaces ) {
					host.NamespaceImports.Add( ns );
				}
			}

			// ******
			if( null != createHostHandler ) {
				//
				// presumably user has replaced the base class,
				// we are passing the host and the default CreateHostHandler()
				//
				createHostHandler( host, CreateHostHandler );
			}
			else {
				CreateHostHandler( host );
			}

			// ******
			return new RazorTemplateEngine( host );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static GeneratorResults Generate(	RazorCodeLanguage codeLanguage,
																							TextReader source,
																							IList<string> refNamespaces,
																							string baseClassName,
																							string genCodeNamespace,
																							string genCodeClassName
																							//,out IList<string> refAssemblies
																						)
		{
			// ******
			RazorTemplateEngine engine = CreateHost(	codeLanguage, 
																								baseClassName, 
																								genCodeNamespace, 
																								genCodeClassName, 
																								refNamespaces, 
																								null
																							);

			//// ******
			////
			//// pre parse the file to get our options at the top
			////
			//refAssemblies = PreParseRazorSource.GetAssemblyRefs( ref source );

			// ******
			//
			// Generate the template class as CodeDom	
			//
			GeneratorResults razorResults = engine.GenerateCode( source );
			return razorResults;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static GeneratorResults CSharp(	TextReader source,
																						IList<string> refNamespaces,
																						string baseClassName,
																						string genCodeNamespace,
																						string genCodeClassName
																					)
		{
			return Generate(	new CSharpRazorCodeLanguage(),
												source,
												refNamespaces,
												baseClassName,
												genCodeNamespace,
												genCodeClassName
												//,out refAssemblies
											);
		}


		/////////////////////////////////////////////////////////////////////////////

		public static GeneratorResults Vb(	TextReader source,
																				IList<string> refNamespaces,
																				string baseClassName,
																				string genCodeNamespace,
																				string genCodeClassName
																			)
		{
			return Generate(	new VBRazorCodeLanguage(),
												source,
												refNamespaces,
												baseClassName,
												genCodeNamespace,
												genCodeClassName
												//,out refAssemblies
											);
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string GetErrorList( string fileName, IList<RazorError> errors )
		{
			// ******
			var sb = new StringBuilder();

			// ******
			foreach( var err in errors ) {
				string errStr = string.Format( "{0}({1},{2}): error: {3}", fileName, err.Location.LineIndex, err.Location.CharacterIndex, err.Message );
				sb.AppendLine( errStr );
			}

			// ******
			return sb.ToString();
		}


	}


}
