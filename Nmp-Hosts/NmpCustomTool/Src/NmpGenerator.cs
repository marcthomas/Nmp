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

using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;


using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;

using EnvDTE;
using EnvDTE80;
using EnvDTE100;
using VSLangProj;
using VSLangProj80;


using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using Microsoft.Win32;

//using Nmp;
using NmpCustomTool;


// http://code.msdn.microsoft.com/SingleFileGenerator/Project/ProjectRss.aspx

namespace NmpCustomTool {


	/////////////////////////////////////////////////////////////////////////////

	[ComVisible(true)]
	[Guid("D707539D-6D2A-4213-BEE2-EC0469B392DC")]
	
	//[CodeGeneratorRegistration(typeof(NetMacroProcessor), "CS File Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true )]
	//[CodeGeneratorRegistration(typeof(NetMacroProcessor), "VB File Generator", vsContextGuids.vsContextGuidVBProject, GeneratesDesignTimeSource = true )]
	////[CodeGeneratorRegistration(typeof(NetMacroProcessor), "J# File Generator", vsContextGuids.vsContextGuidVJSProject, GeneratesDesignTimeSource = true)]
	//[ProvideObject( typeof(NetMacroProcessor) )]
	
	public class NetMacroProcessor : BaseCodeGeneratorWithSite {

//		const int ErrorMessage = 0;
//		const int WarningMessage = 1;
//		const int Message = 2;
//		const int TraceMessage = 3;
//
//		ErrorListProvider errListProvider = null;
//		string defaultExtension = string.Empty;
//
//		protected	Solution solution;
//		protected	ProjectItem templateItem;
//		protected	ConfigurationManager templateConfigMgr;
//		protected	Project templateProject;
//
//		protected	Configuration projConfig;

		string defaultExtension = string.Empty;

		/////////////////////////////////////////////////////////////////////////////


		public override string GetDefaultExtension()
		{
			return defaultExtension;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected override byte[] GenerateCode( string inputFileName, string inputFileContent )
		{
			// ******
			string result = string.Empty;

			var runner = new NmpRunner( Dte, GlobalServiceProvider );
			result = runner.GenerateCode( inputFileName, inputFileContent, out defaultExtension );
					
			// ******
			//
			// causes a unicode 16 bit file to be created
			//
			//return System.Text.Encoding.Unicode.GetBytes( result );
			//
			// causes UTF8 file
			//
	return System.Text.Encoding.UTF8.GetBytes( result );
			//
			// causes ASCII file
			//
			//return System.Text.Encoding.ASCII.GetBytes( result );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public NetMacroProcessor()
		{
		}

	}

}
