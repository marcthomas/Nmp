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
using System.Linq;
using System.Text;

using System.Reflection;
using System.Runtime.InteropServices;

using System.Threading;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;

using EnvDTE;
using EnvDTE100;
//using VSLangProj;
using VSLangProj80;


using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using Microsoft.Win32;




namespace NmpCustomTool {

	/////////////////////////////////////////////////////////////////////////////

	//
	// need this so we can pass Error() and Warning() to MP.Lib
	//
	[Serializable()]
	class RunDomain : IDisposable {

		// ******	
		const string	RunDomainName		= "NmpRun";
		const string	NmpLibName			= "CustomToolShim.dll";

		// ******	
		public AppDomain	CurrentDomain		{ get; set; }
		public AppDomain	WorkingDomain			{ get; set; }

		// ******
		string thisAssemblyLocation = LibInfo.CodeBasePath;


		/////////////////////////////////////////////////////////////////////////////
		
		public CustomToolShim Initialize()
		{
			// ******
			//
			// we were having problems getting IMPEntry to cast and discovered the problem was
			// libraries (MPBase) being resolved in some way that the CLR didn't think that they
			// were the same; some help here: http://www.codeguru.com/forum/archive/index.php/t-398030.html
			//
			// problem solved
			//
			// note this hook is on the Visual Studio app domain, NOT the domain we're creating
			//
			CurrentDomain = System.Threading.Thread.GetDomain();	//AppDomain.CurrentDomain;
			CurrentDomain.AssemblyResolve += new ResolveEventHandler( ResolveEventHandler );
			
			// ******
			WorkingDomain = AppDomain.CreateDomain( RunDomainName, null, null );
			//
			// duh, serialize error because this will be fired in the NEW domain and we live in the OLD domain
			//
			//WorkingDomain.DomainUnload += new EventHandler( WorkDomainDomainGoAway );
			
			if( null != WorkingDomain ) {
				try {
					//
					// shim
					//
					//string dllPath = string.Format( "{0}\\{1}", Path.GetDirectoryName(LibInfo.Location), "MPLibShim.dll" );
					//object obj = domain.CreateInstanceFromAndUnwrap(dllPath, "MPLibShim.XEntry", false, 0, null, new object[] { null, null }, null, null, null);

					// ******
					//
					// MP.Lib
					//
					string dllPath = string.Format( "{0}\\{1}", thisAssemblyLocation, NmpLibName );
					//object obj = WorkingDomain.CreateInstanceFromAndUnwrap(	dllPath,
					//																												"NmpCustomTool.CustomToolEvaluator"
					//																											);//, 

					// ******
					object obj = WorkingDomain.CreateInstanceFromAndUnwrap(	dllPath, "NmpCustomTool.CustomToolShim" );
					return obj as CustomToolShim;
				}

				catch (Exception ex) {
					//ui.MsgBox( "Exception trying to create app domain: {0}", ex.Message );
					Trace.WriteLine( "WorkingDomain.Initialize: {0}", ex.Message );
					UnloadDomain();
					throw;
				}
			}

			// ******
			return null;
		}
					

		/////////////////////////////////////////////////////////////////////////////

		protected Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
		{
			// ******
			string asmName = args.Name.Substring( 0, args.Name.IndexOf(',') );
			string path = string.Format( @"{0}\{1}.dll", thisAssemblyLocation, asmName );

			return Assembly.LoadFrom( path );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void UnloadDomain()
		{
			// ******
			if( null != WorkingDomain ) {
				Trace.WriteLine( "WorkingDomain going away" );
				AppDomain.Unload( WorkingDomain );
				WorkingDomain = null;
			}
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler( ResolveEventHandler );
			UnloadDomain();
		}
	

		/////////////////////////////////////////////////////////////////////////////

		public RunDomain()
		{
			Trace.WriteLine( "WorkingDomain is constructing" );
		}

	}

}
