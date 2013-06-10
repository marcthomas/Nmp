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
using NmpEvaluators;
using Nmp.Output;

//#pragma warning disable 414

namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////

	[Serializable()]
	partial class NMP : NmpScanner, IDisposable {

		// ******
		GrandCentral gc;
		object threadContext;

		// ******
		//
		// trace/debug macro flags
		//
		public bool	TraceMacroFound = false;

		// ******
		string _outputFileExt = string.Empty;
		bool _noOutputFile = false;
		string _outputEncoding = string.Empty;


		/////////////////////////////////////////////////////////////////////////////

		public string OutputFileExt
		{
			get {
				return _outputFileExt;
			}
			set {
				_outputFileExt = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool NoOutputFile
		{
			get {
				return _noOutputFile;
			}
			set {
				_noOutputFile = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string OutputEncoding
		{
			get
			{
				return _outputEncoding;
			}
			set
			{
				_outputEncoding = value;
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public DefaultMacroProcessor MacroProcessor
		{
			get
			{
				return gc.Get<DefaultMacroProcessor>();
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public object ThreadContextState
		{
			get {
				return threadContext;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		MasterOutput _masterOutput = null;

		public MasterOutput MasterOutput
		{
			get
			{
				return _masterOutput;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		MasterOutput GetMasterOutput( bool newOutput )
		{
			// ******
			if( null == _masterOutput || newOutput ) {
				_masterOutput = new MasterOutput( gc );
			}

			// ******
			return _masterOutput;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void ClearMasterOutput()
		{
			_masterOutput = null;
		}


		///////////////////////////////////////////////////////////////////////////////

		public void SetMacroProcessorOutputInstance( MasterOutput output )
		{
			MacroProcessor.OutputInstance = output;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
		{

		const string DOT_RESOURCES = ".resources";

			// ******
			string asmName = args.Name.Substring( 0, args.Name.IndexOf(',') );
			//Console.WriteLine( "resolving assembly: {0}", asmName );
			
			if( asmName.StartsWith("RazorHosting") ) {
				asmName = "RazorHosting.dll";
				//asmName = "System.Web.Razor.dll";
			}
			
			else if( asmName.EndsWith(DOT_RESOURCES) ) {
				asmName = asmName.Substring( 0, asmName.Length - DOT_RESOURCES.Length );
				var asm = Assembly.Load( asmName + ".dll" );
				return asm;
			}

			// ******
			//
			// all Nmp dll's should be located with Nmp.lib
			//
			string path = string.Format( @"{0}\{1}.dll", LibInfo.CodeBasePath, asmName );
			
			//
			// see Nmp V1 - ReferencedAssemblies has assembly names added to it when
			// we ran the compiler to load source (NmpCodeCompiler) - when we compile
			// to memory .NET sometimes can't find/load a user dll - even if it's
			// already loaded!
			//
			//if( ! File.Exists(path) ) {
			//	MPList refList = SharedContext.Shared.ReferencedAssemblies;
			//	string dllName = asmName + ".dll";
			//
			//	foreach( string refPath in refList ) {
			//		if( refPath.EndsWith(dllName, StringComparison.OrdinalIgnoreCase) ) {
			//			path = refPath;
			//			break;
			//		}
			//	}	
			//}

			// ******
			return Assembly.LoadFrom( path );
		}


		/////////////////////////////////////////////////////////////////////////////

		private void SetDefaults()
		{
			// ******
			gc.AltTokenFmtOnly			= false;
			gc.MacroTraceOn				= false;
			
			gc.MacroTraceLevel			= 1;

			gc.PushbackResult = false;
			
			gc.ExpandAndScan = true;

			gc.SetQuotes( SC.DEF_OPEN_QUOTE_STR, SC.DEF_CLOSE_QUOTE_STR );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler( ResolveEventHandler );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NMP( INmpHost host )
		{
			// ******
			if( null == host ) {
				throw new ArgumentNullException( "host" );
			}

			// ******
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler( ResolveEventHandler );
		
			// ******
			//
			// need to make current BEFORE INotification has been created; need to
			// set it up so we can use INotification at this point - maybe new it and
			// set bind it later, or - do we need to bind it ??
			//
			threadContext = ThreadContext.CreateThreadStorage();

			//
			// make sure we're the current context
			//
			using( new NmpMakeCurrent(this) ) {
				gc = new GrandCentral( this, host );
				SetDefaults();
				NmpScannerInitialize( gc.Get<IMacroProcessorBase>(), null );
			}

			// ******
			//
			// see ThreadContext.CreateThreadStorage() above
			//
			ThreadContext.Notifications = Get<INotifications>();
		}


	}


}
