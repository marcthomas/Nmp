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
using System.Linq;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NmpBase;
using Nmp;


#pragma warning disable 169

namespace NmpMSBuild {


	/////////////////////////////////////////////////////////////////////////////

	class NmpMSBuildTaskDieException : Exception {
		public NmpMSBuildTaskDieException( string msg )
			: base(msg)
		{
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	//public class NmpHostHelper : NmpEvaluator, INmpHost {

	public class NmpHostHelper : INmpHost {

		NmpTask task;

		StringBuilder warningsErrorsAndText = new StringBuilder();


		// ******
		public IRecognizer					Recognizer	{ get { return null; } }
		public IMacroTracer					Tracer			{ get { return null; } }
		public IMacroProcessorBase MacroHandler	{ get { return null; } }
		public string HostName { get { return "msbuild"; } }


		///////////////////////////////////////////////////////////////////////////////

		///public bool Errors	{ get; set; }


		///////////////////////////////////////////////////////////////////////////////

		public bool ErrorReturns
		{
			get {
				return false;
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Error( Notifier notifier, string msg, params object [] args )
		{
			// ******
			task.Errors = true;
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;
			task.Log.LogError( string.Empty, ei.DistressLevel.ToString(), string.Empty, ei.FileName, ei.Line, ei.Column, ei.Line, ei.Column, msg, args );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Warning( Notifier notifier, string msg, params object [] args )
		{
			// ******
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;
			task.Log.LogWarning( string.Empty, ei.DistressLevel.ToString(), string.Empty, ei.FileName, ei.Line, ei.Column, ei.Line, ei.Column, msg, args );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( ExecutionInfo ei, string msg, params object [] args )
		{
			task.Log.LogMessage( msg, args );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Trace( ExecutionInfo ei, string fmt, params object [] args )
		{
			System.Diagnostics.Trace.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public void Die( string fmt, params object [] args )
		{
			throw new NmpMSBuildTaskDieException( Helpers.SafeStringFormat(fmt, args) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpHostHelper( NmpTask task )
		{
			this.task = task;
			//base.Initialize( this );
		}


	}


//	/////////////////////////////////////////////////////////////////////////////
//
//	public class NmpInstance : NmpHostHelper {
//
//		StringBuilder warningsErrorsAndText = new StringBuilder();
//
//
//		/////////////////////////////////////////////////////////////////////////////
//	
//		public string InterpretFile( string filePath )
//		{
//			// ******
//			using ( var mp = new NmpEvaluator(this) ) {
//				warningsErrorsAndText.Length = 0;
//				return mp.Evaluate( new EvaluateFromFileContext(filePath), true );
//			}
//
//		}
//		
//		
//		/////////////////////////////////////////////////////////////////////////////
//	
//		public string InterpretString( string str )
//		{
//			// ******
//			using ( var mp = new NmpEvaluator(this) ) {
//				warningsErrorsAndText.Length = 0;
//				return mp.Evaluate( new EvaluateStringContext(str), true );
//			}
//
//		}
//		
//		
//		/////////////////////////////////////////////////////////////////////////////
//		
//		public NmpInstance()
//		{
//		}
//		
//	} 


}


