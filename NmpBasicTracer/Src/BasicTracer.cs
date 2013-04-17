#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// BasicTracer.cs
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NmpBase;
using Nmp;


namespace NmpBasicTracer {


	/////////////////////////////////////////////////////////////////////////////

	public class BasicTracer : IMacroTracer {

		const int INDENT_COUNT	= 2;

		// ******
		string sessionName = string.Empty;
		string rootDirectory = string.Empty; 

		// ******
		NmpStringList macrosCalled = new NmpStringList( unique : true );
		StringBuilder sb = new StringBuilder();

		// ******
		int callDepth = 0;


		/////////////////////////////////////////////////////////////////////////////
		
		protected void sbIndent()
		{
			sb.Append( SC.SPACE, callDepth * INDENT_COUNT );
		}

		
		/////////////////////////////////////////////////////////////////////////////

		int lastBeginId;
		int lastDoneId;

		public void ProcessMacroBegin( int id, IMacro macro, IMacroArguments macroArgs, bool postProcess )
		{
			if( lastDoneId != lastBeginId ) {
				//
				// macros are being called recursivly
				//
				sb.AppendLine();
			}


			sbIndent();
			sb.AppendFormat( "{0}\n", macro.Name );

			lastBeginId = id;
			++callDepth;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void ProcessMacroDone( int id, IMacro macro, bool postProcessed, bool diverted, object macroResult, object finalResult )
		{
			--callDepth;
			lastDoneId = id;

			// ******
			string result = string.Empty;

			if( null != finalResult ) {
				result = finalResult.ToString();
				if( result.Length > 64 ) {
					result = result.Substring( 0, 64 ) + " ...";
				}
				result = result.Replace( "\n", " " );
			}

			// ******
			sbIndent();
			sb.AppendFormat( "{0} result: {1}\n", macro.Name, result );
			sb.AppendLine();
		}


		/////////////////////////////////////////////////////////////////////////////

		public void FindMacroCall( string name, IMacro macro )
		{
			if( null != macro ) {
				macrosCalled.Add( name );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void BeginSession( string sessionName, string rootDirectory )
		{
			sb.Length = 0;
			this.sessionName = sessionName;
			this.rootDirectory = rootDirectory;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void EndSession()
		{
			
			File.WriteAllText( string.Format(@"{0}\{1}.trace", rootDirectory, sessionName), sb.ToString() );
		}


		/////////////////////////////////////////////////////////////////////////////

		public BasicTracer()
		{
		}


	}


}
