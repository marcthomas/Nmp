#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// ErrorsWarningsTrace.cs
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;

using NmpBase;
using NmpExpressions;


namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////

	class Notifications : INotifications {

		// ******
		static int _nmpInstance;

		// ******
		int nmpInstance;
		string idNmpInstance;

		// ******
		INmpHost host;
		InvocationStack invocationStack;

		public StringBuilder preErrorWarningText = new StringBuilder();

		/////////////////////////////////////////////////////////////////////////////

		private NmpTuple<string, int, int> CurrentLocation( IMacroInvocationRecord item )
		{
			// ******
			//IMacroInvocationRecord item = stack.Peek();
			
			// ******
			//if( null == item.Macro ) {
			//	//
			//	// should not happen
			//	//
			//	throw new ShouldNeverHappenException( "null == item.Macro" );
			//}

			// ******
			string fileName = item.SourceName;
			int line = item.Line;
			int column = item.Column;

			// ******
			if( item.CalledFromMacro ) {
				//
				// file/line info from the macro that called the macro
				// represented by the current 'item' - the next lower item
				// on the stack
				//
				IMacro macro = invocationStack.GetCallingMacro( item );
				if( null != macro ) {
					fileName = macro.MacroType == MacroType.Text ? macro.SourceFile : macro.MacroType.ToString() + " Macro";
					line += macro.MacroStartLine;
				}
			}

			// ******
			return new NmpTuple<string, int, int>( fileName, line, column );
		}


		/////////////////////////////////////////////////////////////////////////////

		private NmpTuple<string, int, int> GenerateInvocationStackDump( StringBuilder sb )
		{
			// ******
			bool somePushBack = false;

			string fileName = string.Empty;
			int line = 0;
			int column = 0;

			// ******
			sb.AppendLine();
			sb.AppendLine( "Macro Stack" );
			sb.AppendLine( "===========" );
			sb.AppendLine();

			foreach( IMacroInvocationRecord item in invocationStack.ReverseEnumerator() ) {
				if( null ==  item.Macro ) {
					sb.AppendFormat( "Root file: \"{0}\"\n", item.SourceName );
				}
				else {
					//line = item.Line;
					//column = item.Column;
					//fileName = item.SourceName;

					// ******
					string macroName = string.Empty;
					if( null != item.Macro ) {
						macroName = item.Macro.Name;
					}

					//// ******
					//if( item.CalledFromMacro ) {
					//	//
					//	// file/line info from the macro that called the macro
					//	// represented by the current 'item' - the next lower item
					//	// on the stack
					//	//
					//	IMacro macro = stack.GetCallingMacro( item );
					//	fileName = macro.MacroType == MacroType.Text ? macro.SourceFile : macro.MacroType.ToString() + " Macro";
					//	line += macro.MacroStartLine;
					//}

					NmpTuple<string, int, int> currentLocation = CurrentLocation( item );
					fileName = currentLocation.Item1;
					line = currentLocation.Item2;
					column = currentLocation.Item3;

					// ******
					sb.AppendFormat( "Call Macro: \"{0}\", from: Line {1}: Column: {2} File: \"{3}\"\n", macroName, line, column, fileName );

					// ******
					string someMacroText = item.Text;

					char [] chars = new char[] { '\r', '\n' };

					int index = 0;
					
					while( 0 == (index = someMacroText.IndexOfAny(chars)) )
						;
					
					if( index > 0 ) {
						someMacroText = string.Format( "{0}\n ...", someMacroText.Substring(0, index) );
					}

					sb.AppendFormat( "\n {0}\n", someMacroText );
				}

				if( item.PushbackCalled ) {
					somePushBack = true;
				}

				sb.AppendLine();
				sb.AppendLine( "---------------------------------------------------------" );
				sb.AppendLine();
			}

			// ******
			if( somePushBack ) {
				sb.AppendLine( "\nNote: text being pushed back onto the input will cause some line numbers to become invalid" );
				sb.AppendLine();
			}

			// ******
			//
			// returning: fileName, line and column of the last item we processed which is the
			// item (mir) at the top of the stack - this is the information our caller needs to
			// identifiy the location of the error
			//
			return new NmpTuple<string, int, int>( fileName, line, column );
		}


		/////////////////////////////////////////////////////////////////////////////

		private ExecutionInfo GetExecutionInfo( StringBuilder stackDumpResult, string msg )
		{								
			// ******
			StringBuilder sb = new StringBuilder();
			NmpTuple<string, int, int> currentLocation = GenerateInvocationStackDump( sb );

			// ******
			if( null != stackDumpResult ) {
				stackDumpResult = sb;
			}

			// ******
			return new ExecutionInfo( 0, currentLocation.Item1, currentLocation.Item2, currentLocation.Item3, msg );
		}


		/////////////////////////////////////////////////////////////////////////////

		private void PreErrorWarningText( string fmt, params object [] args )
		{
			if( 0 == args.Length ) {
				preErrorWarningText.AppendFormat( fmt );
			}
			else {
				preErrorWarningText.AppendFormat( fmt, args );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// 
		//
		///////////////////////////////////////////////////////////////////////////

		public void MacroError( string fmt, params object [] args )
		{
			MacroError( new UnknownNotifier(), fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void MacroError( Notifier notifier, string fmt, params object [] args )
		{								
			// ******
			//InvocationStack iStack = InvocationStack;

			// ******
			if( null == notifier ) {
				notifier = new UnknownNotifier();
			}

			// ******
			StringBuilder sb = new StringBuilder();
			NmpTuple<string, int, int> currentLocation = GenerateInvocationStackDump( sb );

			// ******
			sb.Append( preErrorWarningText );
			sb.AppendLine();
			preErrorWarningText.Length = 0;


			string safeText = Helpers.SafeStringFormat( fmt, args );

			if( string.IsNullOrEmpty(fmt) && null != notifier.ExceptionToThrow ) {
				sb.Append( notifier.ExceptionToThrow.Message );
			}
			else {
				//sb.Append( Helpers.SafeStringFormat(fmt, args) );
				sb.Append( safeText );
			}

			// ******
			notifier.ExecutionInfo = new ExecutionInfo(	0,
																									currentLocation.Item1,
																									currentLocation.Item2,
																									currentLocation.Item3,
																									sb.ToString()
																								);

			// ******
			if( null == notifier.ExceptionToThrow ) {
				notifier.ExceptionToThrow = new MacroErrorException( fmt, args );
			}

			// ******
			//
			// never returns;
			//
			host.Error( notifier, idNmpInstance + safeText );
		}
		

		///////////////////////////////////////////////////////////////////////////////

		public void MacroWarning( string fmt, params object [] args )
		{
			MacroWarning( new UnknownNotifier(), fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void MacroWarning( Notifier notifier, string fmt, params object [] args )
		{								
			// ******
			if( null == notifier ) {
				notifier = new UnknownNotifier();
			}

			// ******
			StringBuilder sb = new StringBuilder();

			sb.Append( preErrorWarningText );
			preErrorWarningText.Length = 0;

			string safeText = Helpers.SafeStringFormat( fmt, args );

			sb.AppendFormat( safeText );

			// ******
			NmpTuple<string, int, int> currentLocation = CurrentLocation( invocationStack.Peek() );
			notifier.ExecutionInfo = new ExecutionInfo(	0,
																									currentLocation.Item1,
																									currentLocation.Item2,
																									currentLocation.Item3,
																									sb.ToString()
																								);

			// ******
			host.Warning( notifier, idNmpInstance + safeText );
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( string fmt, params object [] args )
		{					
			// ******			
			NmpTuple<string, int, int> currentLocation = CurrentLocation( invocationStack.Peek() );
			string message = Helpers.SafeStringFormat( fmt, args );
			var ei = new ExecutionInfo(	0,
																	currentLocation.Item1,
																	currentLocation.Item2,
																	currentLocation.Item3,
																	message
																);

			// ******
			host.WriteMessage( ei, idNmpInstance + message );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Trace( string fmt, params object [] args )
		{	
			// ******			
			NmpTuple<string, int, int> currentLocation = CurrentLocation( invocationStack.Peek() );
			string message = Helpers.SafeStringFormat( fmt, args );
			var ei = new ExecutionInfo(	0,
																	currentLocation.Item1,
																	currentLocation.Item2,
																	currentLocation.Item3,
																	string.Empty
																);

			// ******
			host.WriteMessage( ei, idNmpInstance + message );
			host.Trace( ei,  idNmpInstance + message );
		}


		/////////////////////////////////////////////////////////////////////////////
 
		public Notifications( INmpHost host, InvocationStack stack )
		{
			// ******
			nmpInstance = ++_nmpInstance;
			//idNmpInstance = string.Format( "[{0}] ", nmpInstance );
			idNmpInstance = string.Format( "[Nmp instance:{0}] ", nmpInstance );

			// ******
			this.host = host;
			this.invocationStack = stack;
		}
 
	}


}