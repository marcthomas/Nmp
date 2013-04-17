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
using System.IO;
using System.Reflection;
using System.Text;

using NmpBase;
using Nmp;

namespace NmpCustomTool {


	/////////////////////////////////////////////////////////////////////////////

	class CustomToolEvaluatorDieException : Exception {
		public CustomToolEvaluatorDieException( string msg )
			: base(msg)
		{
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	//public enum HostMessageType { Error, Warning, Message, TraceMessage };

	public class NmpResult : MarshalByRefObject {

		public class HostMessage : MarshalByRefObject {
			public int		MessageType;
			public int		DistressLevel;
			public string	FullMessage = string.Empty;
			public string	Message = string.Empty;
			public int		Line;
			public int		Column;
		}

		// ******
		public string FileName = string.Empty;
		public string InputText = string.Empty;
		public string MacroResult = string.Empty;
		public string FileExt = string.Empty;
		public List<HostMessage> Messages = new List<HostMessage>();
		public bool		Errors;
	}


	/////////////////////////////////////////////////////////////////////////////

	public class CustomToolHost : INmpHost {

		const int ErrorMessage = 0;
		const int WarningMessage = 1;
		const int Message = 2;
		const int TraceMessage = 3;

		// ******
		public IRecognizer					Recognizer	{ get { return null; } }
		public IMacroTracer					Tracer			{ get { return null; } }
		public IMacroProcessorBase MacroHandler	{ get { return null; } }
		public string								HostName		{ get { return "visualstudio";}}

		// ******
		public List<NmpResult.HostMessage> Messages;



		/////////////////////////////////////////////////////////////////////////////

		public static string SafeStringFormat( string fmt, params object [] args )
		{
			// ******
			if( null == fmt ) {
				return string.Empty;
			}

			// ******
			if( args.Length > 0 ) {
				try {
					return string.Format( fmt, args );
				}
				catch {
					return string.Format( "{0}", fmt );
				}
			}

			// ******
			return fmt;
		}


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
			string message = SafeStringFormat( msg, args );
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;

			// ******
			//Errors.Add( new Tuple<bool, int, string, int, int>( true, ei.DistressLevel, message, ei.Line, ei.Column) );
			
			Messages.Add( new NmpResult.HostMessage() {	MessageType = ErrorMessage, 
																				DistressLevel = ei.DistressLevel, 
																				FullMessage = ei.FullMessage,
																				Message = message,
																				Line = ei.Line, 
																				Column = ei.Column
																			}
									);

			Die( msg, args );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Warning( Notifier notifier, string msg, params object [] args )
		{
			// ******
			string message = SafeStringFormat( msg, args );
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;

			// ******
			//Errors.Add( new Tuple<bool, int, string, int, int>( false, ei.DistressLevel, message, ei.Line, ei.Column) );

			Messages.Add( new NmpResult.HostMessage() {	MessageType = WarningMessage, 
																				DistressLevel = ei.DistressLevel, 
																				FullMessage = ei.FullMessage,
																				Message = message,
																				Line = ei.Line, 
																				Column = ei.Column
																			}
									);

		}


		///////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( ExecutionInfo ei, string msg, params object [] args )
		{
			// ******
			string message = SafeStringFormat( msg, args );
			if( null == ei ) {
				new ExecutionInfo();
			}

			Messages.Add( new NmpResult.HostMessage() {	MessageType = Message, 
																				DistressLevel = ei.DistressLevel, 
																				FullMessage = ei.FullMessage,
																				Message = message,
																				Line = ei.Line, 
																				Column = ei.Column
																			}
									);
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Trace( ExecutionInfo ei, string msg, params object [] args )
		{
			// ******
			string message = SafeStringFormat( msg, args );
			if( null == ei ) {
				new ExecutionInfo();
			}

			Messages.Add( new NmpResult.HostMessage() {	MessageType = TraceMessage, 
																				DistressLevel = ei.DistressLevel, 
																				FullMessage = ei.FullMessage,
																				Message = message,
																				Line = ei.Line, 
																				Column = ei.Column
																			}
									);

			System.Diagnostics.Trace.WriteLine( message );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public void Die( string fmt, params object [] args )
		{
			throw new CustomToolEvaluatorDieException( Helpers.SafeStringFormat(fmt, args) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public CustomToolHost( List<NmpResult.HostMessage> messages )
		{
			Messages = messages;
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	public class CustomToolShim : MarshalByRefObject {	//: ICustomToolShim {

		/////////////////////////////////////////////////////////////////////////////

		class TextMacroData {
			public string Name = string.Empty;
			public string Text = string.Empty;
			public List<string> ArgNames = null;

			public TextMacroData( string name, string text, List<string> argNames )
			{
				Name = name;
				Text = text;
				ArgNames = argNames;
			}
		}

		// ******
		const string VSBUILD_PATHS_MACRO_NAME = "#vsBuildPaths";
		const string VSPROJECT_PROPERTIES			= "#vsProjectProperties";

		// ******
		Dictionary<string, object> ObjectMacros = new Dictionary<string, object>();
		List<TextMacroData> TextMacros = new List<TextMacroData>();


		/////////////////////////////////////////////////////////////////////////////

		private void AddMacros( NmpEvaluator nmp, IDictionary vsBuildPaths )
		{
			// ******
			foreach( KeyValuePair<string, object> kvp in ObjectMacros ) {
				nmp.AddObjectMacro( kvp.Key, kvp.Value );
			}

			// ******
			foreach( var tmd in TextMacros ) {
				nmp.AddTextMacro( tmd.Name, tmd.Text, new NmpStringList(tmd.ArgNames) );
			}

			// ******
			//nmp.AddObjectMacro( VSBUILD_PATHS_MACRO_NAME, new NmpArray(vsBuildPaths) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AddObjectMacro( string macroName, object netObject )
		{
			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				throw new ArgumentNullException( "macroName" );
			}

			// ******
			if( null == netObject ) {
				throw new ArgumentNullException( "netObject" );
			}

			// ******
			ObjectMacros[ macroName ] = netObject;
		}
			

		/////////////////////////////////////////////////////////////////////////////

		public void AddTextMacro( string macroName, string text, List<string> argNames )
		{
			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				throw new ArgumentNullException( "macroName" );
			}

			// ******
			TextMacros.Add( new TextMacroData(macroName, text, argNames) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpResult Evaluate( IDictionary vsBuildPaths, IDictionary projProperties, string text, string inputFileName )
		{
			// ******
			string inputFullPath = Path.GetFullPath( inputFileName ).ToLower();

			// ******
			NmpResult evalResult = new NmpResult();

			evalResult.InputText = text;
			evalResult.FileName = inputFullPath;

			// ******
			var host = new CustomToolHost( evalResult.Messages );

			using( var nmp = new NmpEvaluator(host) ) {
				if( ! string.IsNullOrEmpty(inputFullPath) ) {
					nmp.ChangeRootDirectory( inputFileName );
				}

				// ******
				if( null != vsBuildPaths ) {
					AddObjectMacro( VSBUILD_PATHS_MACRO_NAME, new NmpArray(vsBuildPaths) );
				}

				if( null != projProperties ) {
					AddObjectMacro( VSPROJECT_PROPERTIES, new NmpArray(projProperties) );
			
					AddMacros( nmp, vsBuildPaths );
				}

				// ******
				var evx = nmp.GetStringContext( text );
				evx.SetFileInfo( inputFullPath );

				// ******
				bool errors = false;
				string result = string.Empty;
				try {
					evalResult.MacroResult = nmp.Evaluate( evx, true );
					evalResult.FileExt = nmp.FileExt;
				}
				catch ( CustomToolEvaluatorDieException ) {
					//
					// this is an exception we've created for as a generic terminal error
					// which simply causes us to return
					//
					errors = true;
				}
		
		//
		// catch error and report here
		//
	#if EXREPORTING
				// ref dll
	#endif
				//catch ( Exception ex ) {
				//	throw;
				//}

				// ******
				evalResult.Errors = errors;
				return evalResult;

			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public CustomToolShim()
		{
		}


	}



}
