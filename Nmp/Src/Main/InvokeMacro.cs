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
using NmpExpressions;
using Nmp.Expressions;
using Nmp.Output;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	public class MacroNotFoundException : Exception {

		public string MacroName	{ get; private set; }


		/////////////////////////////////////////////////////////////////////////////

		public MacroNotFoundException( string macroName )
			:	base(macroName)
		{
			this.MacroName = macroName;
		}
	}
	
	
	/////////////////////////////////////////////////////////////////////////////

//	[Serializable()]
	partial class NMP {


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Invokes a macro with the 'args' - currently not implemented
		/// </summary>
		/// <param name="macroName"></param>
		/// <param name="args"></param>
		/// <param name="newOutput"></param>
		/// <returns></returns>
		
		public string InvokeMacro( string macroName, object [] args, bool newOutput )
		{
			// ******
			IMacro macro;
			if( ! MacroProcessor.FindMacro(macroName, out macro) ) {
				throw new MacroNotFoundException(macroName);
			}
			
			// ******
			MacroExpression expression = ETB.CreateMacroCallExpression( macro, args );
			var input = gc.GetParseReader( macroName );
			var mir = new MIR( macro, false, null, input, "InvokeMacro direct", 0, 1, 1 );

			// ******
			//
			// EvalLock() will throw an exception if this instance of NMP is already
			// evaluating something, it also calls Restore()/Save() for our thread
			// data
			//
			using( new EvalLock(this) ) {

				// ******
				MasterOutput output = new MasterOutput( gc );

				using( new UsingHelper(() => SetMacroProcessorOutputInstance(output), () => SetMacroProcessorOutputInstance(null)) ) {
					object result = MacroProcessor.InvokeMacro( input, mir, expression, true );

					// ******
					//
					// if we were calling Scanner() then we'd just call 'output.AllText' but since we're
					// calling InvokeMacro() the text we're interested in is what is returned by the macro
					// NOT what's in the output buffer
					//
					// so, we have to call FinalProcessText() ourself with the result of the macro invocation
					//
					StringBuilder sb = output.FinalProcessText( new StringBuilder(result.ToString()), true );
					string strResult = sb.ToString();
					return strResult;
				}
			}
		}
		

	}


}
