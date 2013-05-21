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

//using System.Runtime.InteropServices;

//using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.Shell;


//using Microsoft.VisualStudio.Shell.Interop;
////using Microsoft.VisualStudio.OLE.Interop;
//using Microsoft.VisualStudio.Designer.Interfaces;

////using Microsoft.VisualStudio.TextManager.Interop;

//using EnvDTE;
//using EnvDTE80;
//using EnvDTE100;
//using VSLangProj;
//using VSLangProj80;

using EnvDTE;
using EnvDTE100;
//using VSLangProj;

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using Microsoft.Win32;


// http://code.msdn.microsoft.com/SingleFileGenerator/Project/ProjectRss.aspx

namespace NmpCustomTool {


	/////////////////////////////////////////////////////////////////////////////

	public class NmpRunner {

		const int ErrorMessage = 0;
		const int WarningMessage = 1;
		const int Message = 2;
		const int TraceMessage = 3;

		DTE Dte;
		ServiceProvider GlobalServiceProvider;

		static ErrorListProvider errListProvider = null;
		
		//string defaultExtension = string.Empty;

		protected	Solution solution;
		protected	ProjectItem templateItem;
		protected	ConfigurationManager templateConfigMgr;
		protected	Project templateProject;

		protected	Configuration projConfig;


		/////////////////////////////////////////////////////////////////////////////

		public string _getPropertyAsString( Properties props, string propName )
		{
			// ******
			if( null != props && ! string.IsNullOrEmpty(propName) ) {
				try {
					EnvDTE.Property  property = props.Item( propName );
					if( null != property ) {
						return property.Value.ToString();
					}
				}
				catch {
				}
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Dictionary<string, string> AllProjectProperties( Project proj )
		{
			// ******
			var dict = new Dictionary<string, string>();

			var props = proj.Properties;
			if( null != props ) {
				foreach( EnvDTE.Property prop in props ) {
					if( null != prop ) {
						string name = string.Empty;
						string value = string.Empty;
						try {
							name = prop.Name;
							value = prop.Value.ToString();
						}
						catch {
							//value = string.Empty;
						}
						dict[ name ] = value;
					}
				}
			}

			// ******
			return dict;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ProjectPropertyAsString( string propName )
		{
			return _getPropertyAsString( null == templateProject ? null : templateProject.Properties, propName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ProjectConfigPropertyAsString( string propName )
		{
			return _getPropertyAsString( null == projConfig ? null : projConfig.Properties, propName );
		}


		/////////////////////////////////////////////////////////////////////////////

		private void AddNoNulls( Dictionary<string, string> dict, string key, string value )
		{
			if( string.IsNullOrEmpty(key) ) {
				return;
			}

			dict[ key ] = null == value ? string.Empty : value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public IDictionary GetProjectBuildPaths()
		{
			// ******
			//StringStringDict dict = new StringStringDict();
			Dictionary<string, string> dict = new Dictionary<string, string>();

			// ******
//			if( ! IsConnected ) {
//				return dict;
//			}

			//StringObjectDict sp = SolutionProperties();
			//StringObjectDict pp = ProjectProperties();
			//StringObjectDict pcp = ProjectConfigProperties();

			// ******
			Properties props = projConfig.Properties;

			string fullSolutionPath = solution.FullName;
			string fullProjectPath = templateProject.FullName;
			string targetName = ProjectPropertyAsString("OutputFileName");

			// ******
			//
			//OutDir
			//
			AddNoNulls( dict, "outdir", ProjectConfigPropertyAsString("OutputPath") );

			// ******
			//
			// ConfigurationName
			//
			AddNoNulls( dict, "configurationname", projConfig.ConfigurationName );

			// ******
			//
			// ProjectName
			//
			AddNoNulls( dict, "projectname", templateProject.Name );

			// ******
			//
			// TargetName
			//
			AddNoNulls( dict, "targetname", Path.GetFileNameWithoutExtension(targetName) );

			// ******
			//
			// TargetPath
			//
			AddNoNulls( dict, "targetpath", string.Format(@"{0}\{1}\{2}", Path.GetDirectoryName(fullProjectPath), dict["outdir"], targetName ) );

			// ******
			//
			// ProjectPath
			//
			AddNoNulls( dict, "projectpath", fullProjectPath );

			// ******
			//
			// ProjectFileName
			//
			AddNoNulls( dict, "projectfilename", Path.GetFileName( fullProjectPath ) );

			// ******
			//
			// TargetExt
			//
			AddNoNulls( dict, "targetext", Path.GetExtension(targetName) );

			// ******
			//
			// TargetFileName
			//
			AddNoNulls( dict, "targetfilename", targetName );

			// ******
			//
			// TargetDir
			//
			AddNoNulls( dict, "targetdir", string.Format(@"{0}\{1}", Path.GetDirectoryName(fullProjectPath), dict["outdir"] ) );

			// ******
			//
			// ProjectDir
			//
			AddNoNulls( dict, "projectdir", Path.GetDirectoryName(fullProjectPath) + "\\" );


			// ******
			if( string.IsNullOrEmpty(fullSolutionPath) ) {
				AddNoNulls( dict, "solutionfilename", string.Empty );
				AddNoNulls( dict, "solutionpath", string.Empty );
				AddNoNulls( dict, "solutiondir", string.Empty );
				AddNoNulls( dict, "solutionname", string.Empty );
				AddNoNulls( dict, "solutionext", string.Empty );
			}
			else {
				//
				// SolutionFileName
				//
				AddNoNulls( dict, "solutionfilename", Path.GetFileName(fullSolutionPath) );

				// ******
				//
				// SolutionPath
				//
				AddNoNulls( dict, "solutionpath", fullSolutionPath );

				// ******
				//
				// SolutionDir
				//
				AddNoNulls( dict, "solutiondir", Path.GetDirectoryName(fullSolutionPath) + "\\" );

				// ******
				//
				// SolutionName
				//
				AddNoNulls( dict, "solutionname", Path.GetFileNameWithoutExtension(fullSolutionPath) );

				// ******
				//
				// SolutionExt
				//
				AddNoNulls( dict, "solutionext", Path.GetExtension(fullSolutionPath) );
			}

			// ******
			//
			// PlatformName
			//
			AddNoNulls( dict, "platformname", projConfig.PlatformName );

			// ******
			//
			// ProjectExt
			//
			AddNoNulls( dict, "projectext", Path.GetExtension(fullProjectPath) );

			// ******
			return dict;
		}


		/////////////////////////////////////////////////////////////////////////////

//
// this uses the "legacy" api, need use new api
//
// the following does not compile
//
//
//// http://social.msdn.microsoft.com/Forums/en/vsx/thread/81c2959c-a21a-4baa-88b2-757ce0769532
//
//		private void NavigateHandler(object sender, EventArgs arguments)	
//		{	
//			Microsoft.VisualStudio.Shell.Task task = sender as Microsoft.VisualStudio.Shell.Task;	
//
//			if (task == null)	
//			{	
//				throw new ArgumentException("sender parm cannot be null");	
//			}	
//
//			if (String.IsNullOrEmpty(task.Document))	
//			{	
//				return;	
//			}	
//
////			IVsUIShellOpenDocument openDoc = GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;	
//
//			IVsUIShellOpenDocument openDoc = GlobalServiceProvider.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;	
//
//			if (openDoc == null)	
//			{	
//				return;	
//			}	
//
//			IVsWindowFrame frame;	
//			Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider;	
//			IVsUIHierarchy hierarchy;	
//			uint itemId;	
//			Guid logicalView = VSConstants.LOGVIEWID_Code;	
//
//			if (ErrorHandler.Failed(openDoc.OpenDocumentViaProject(	
//				task.Document, ref logicalView, out serviceProvider, out hierarchy, out itemId, out frame))	
//				|| frame == null)	
//			{	
//				return;	
//			}	
//
//			object docData;	
//			frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData);	
//
//			VsTextBuffer buffer = docData as VsTextBuffer;	
//			if (buffer == null)	
//			{	
//				IVsTextBufferProvider bufferProvider = docData as IVsTextBufferProvider;	
//				if (bufferProvider != null)	
//				{	
//					IVsTextLines lines;	
//					ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));	
//					buffer = lines as VsTextBuffer;	
//
//					if (buffer == null)	
//					{	
//						return;	
//					}	
//				}	
//			}	
//
//			IVsTextManager mgr = GetService(typeof(VsTextManagerClass)) as IVsTextManager;	
//			if (mgr == null)	
//			{	
//				return;	
//			}	
//
//			mgr.NavigateToLineAndColumn(buffer, ref logicalView, task.Line, task.Column, task.Line, task.Column);	
//		}	
//

		/////////////////////////////////////////////////////////////////////////////

		private void ErrList( ErrorListProvider errListProvider, int msgType, string fileName, string message, int line, int column )
		{
			// ******
			//ErrorListProvider ep = new ErrorListProvider( GlobalServiceProvider );
			if( null == errListProvider ) {
				return;
			}
			
			// ******
			TaskErrorCategory category;

			switch( msgType ) {
				case ErrorMessage:
					category = TaskErrorCategory.Error;
					break;

				case WarningMessage:
					category = TaskErrorCategory.Warning;
					break;

				default:
					category = TaskErrorCategory.Message;
					break;
			}

			// ******
			//
			// http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.errortask.aspx
			//
			ErrorTask et = new ErrorTask();

			et.CanDelete = true;
			et.ImageIndex = 0;
			et.ErrorCategory = category;
			et.Priority = TaskPriority.Normal;	//High;
			et.Document = string.IsNullOrEmpty(fileName) ? string.Empty : fileName;
			et.Text = string.IsNullOrEmpty( message ) ? string.Empty : message;
			et.Line = (int) line;
			et.Column = (int) column;

			// ******
			try {
				errListProvider.Tasks.Add( et );
				errListProvider.Show();
			}
			catch ( Exception ex ) {
				string str = ex.Message;
			}
		}
		

		/////////////////////////////////////////////////////////////////////////////

		protected void WriteOutput( string fmt, params object [] args )
		{
			// http://stackoverflow.com/questions/1094366/how-do-i-write-to-the-visual-studio-output-window-in-my-custom-tool

			//
			// more info, not necessarily used:
			// http://msdn.microsoft.com/en-us/library/bb166236.aspx
			//

			// ******
			//IVsOutputWindow outWindow = Package.GetGlobalService( typeof( SVsOutputWindow ) ) as IVsOutputWindow;
			IVsOutputWindow outWindow = GlobalServiceProvider.GetService( typeof( SVsOutputWindow ) ) as IVsOutputWindow;

			// ******
			//
			// these do not work; best guess: they changed how this works in VS 2010 but it didn't make
			// it into the documentation
			//
			// so, we'll create our own window - we know that works (in VS 2010 at least)
			//			
			//Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
			Guid nmpWindowGuid = new Guid( "{09A9CC84-B600-4FAF-8177-82949048A3A2}" );

			// ******
			IVsOutputWindowPane generalPane;
			outWindow.GetPane( ref nmpWindowGuid, out generalPane );
			if( null == generalPane ) {
				if( 0 == outWindow.CreatePane( ref nmpWindowGuid, "Net Macro Processor", 1, 1 ) ) {
					outWindow.GetPane( ref nmpWindowGuid, out generalPane );
				}
			}
			
			// ******
			if( null != generalPane ) {
				generalPane.OutputString( CustomToolUtilities.SafeStringFormat(fmt, args) );
				generalPane.Activate(); // Brings this pane into view
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		private void HandleMessages( NmpResult nmpResult )
		{
			// ******
			//
			// clear the error list
			//
			TaskProvider.TaskCollection tc = errListProvider.Tasks;
			tc.Clear();

			// ******
			foreach( var msg in nmpResult.Messages ) {
				WriteOutput( msg.FullMessage );

				if( TraceMessage != msg.MessageType ) {
					//GeneratorErrorCallback( HostMessageType.Warning == msg.MessageType, msg.DistressLevel, msg.Message, msg.Line, msg.Column );
					//
					// this allows us to add messages as well
					//
					ErrList( errListProvider, msg.MessageType, nmpResult.FileName, msg.Message, msg.Line - 1, msg.Column - 1 );
				}
			}

			// ******
			errListProvider.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void Initialize( string inputFilePath )
		{
			// ******
			if( null == errListProvider ) {
				//
				// custom tool FAILS if we try to do this in the ctor
				//
				errListProvider = new ErrorListProvider( GlobalServiceProvider );
			}
	
			// ******
			solution = Dte.Solution;
			if( null == solution ) {
				return;
			}

			// ******
			templateItem = solution.FindProjectItem( inputFilePath );
			templateConfigMgr = templateItem.ConfigurationManager;
			templateProject = templateItem.ContainingProject;

			//IEnumerable<Project> projects = GetAllProjects( solution );

			// ******
			//
			// is always null - this is references elsewhere (null is ok)
			//
			projConfig = templateProject.ConfigurationManager.ActiveConfiguration;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GenerateCode( string inputFileName, string inputFileContent, out string defaultExtension )
		{
			// ******
			Initialize( inputFileName );
			defaultExtension = string.Empty;

			// ******
			string result = string.Empty;
			using( var runDomain = new RunDomain() ) {
				CustomToolShim nmpEval = runDomain.Initialize();

				if( null == nmpEval ) {
					WriteOutput( "unable to create instance of macro processor shim (CustomToolShim)\n" );
				}

				else {
					NmpResult nmpResult = nmpEval.Evaluate( GetProjectBuildPaths(), AllProjectProperties(templateProject), inputFileContent, inputFileName );
					HandleMessages( nmpResult );
					
					// ******
					result = nmpResult.MacroResult;
					//defaultExtension = nmpResult.FileExt;
					var ext = nmpResult.FileExt;
					defaultExtension = string.IsNullOrEmpty( ext ) ? null : ext;
				}
			}
				
			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpRunner( DTE Dte, ServiceProvider GlobalServiceProvider )
		{
			this.Dte = Dte;
			this.GlobalServiceProvider = GlobalServiceProvider;
		}

	}

}
