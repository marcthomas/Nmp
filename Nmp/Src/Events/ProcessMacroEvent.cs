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
using Nmp.Input;
using NmpEvaluators;
using NmpExpressions;
using Nmp.Output;



namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////

	class ProcessMacroEvent {

		public MIR MacroInvocationRecord { get; private set; }

		IMacro macro;
		IMacroArguments macroArgs;
		bool postProcess;


		/////////////////////////////////////////////////////////////////////////////

		public string BasicInfo()
		{
			// ******
			var mir = MacroInvocationRecord;
			var macro = mir.Macro;

			var sb = new StringBuilder { };

			// ******

			// called by

			//string calledBy = string.Empty;
			//if( mir.CalledFromFile ) {
			//	calledBy = string.Format( "macro file: {0}", string.IsNullOrEmpty( mir.SourceName ) ? "" : mir.SourceName );
			//}
			//else {
			//	calledBy = string.Format( "macro: {0}", null == macro
			//}

			if( null == mir.Macro ) {
				sb.AppendFormat( "Root file: \"{0}\"\n", mir.SourceName );
			}
			else {
				string macroName = null != mir.Macro ? mir.Macro.Name : string.Empty;

				// ******
				string someMacroText = mir.InitialTextSpan.TrimStart();

				//char [] chars = new char [] { '\r', '\n' };

				//int index = 0;

				//while( 0 == (index = someMacroText.IndexOfAny( chars )) )
				//	;

				//if( index > 0 ) {
				//	someMacroText = string.Format( "{0}\n ...", someMacroText.Substring( 0, index ) );
				//}

				sb.AppendFormat( "\n {0}\n", someMacroText );
		
			
			}
			
			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			var macroArgs = MacroInvocationRecord.MacroArgs;

			// ******
			var sb = new StringBuilder { };
			{
				sb.Append( BasicInfo() );
			}
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Write( MIR mir )
		{
			var evt = new ProcessMacroEvent { MacroInvocationRecord = mir };
			EventWriter.Information( evt );
		}


	}


}
