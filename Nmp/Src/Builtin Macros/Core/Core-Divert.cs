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
using System.Text.RegularExpressions;


using NmpBase;
using Nmp;
using Nmp.Output;


namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////

	//public class Divert {
	//
	//	IMacroProcessor mp;
	//	Builtin builtins;

	partial class CoreMacros {

		/////////////////////////////////////////////////////////////////////////////

		MasterOutput GetOutput
		{
			[DebuggerStepThrough]
			get
			{
				MasterOutput output = mp.OutputInstance as MasterOutput;
				if( null == output ) {
					throw new Exception( "OutputMacros ... unable to obtain valid MasterOutput instance!" );
				}
				return output;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object clearDivert( string divName )
		{
			GetOutput.ClearDivert( new List<string>() {divName} );
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object pushDivert( string divName )
		{
			GetOutput.PushDivert( divName );
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object pushdivert( string divName )
		{
			return pushDivert( divName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object popDivert()
		{
			GetOutput.PopDivert();
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object popdivert()
		{
			return popDivert();
		}


		/////////////////////////////////////////////////////////////////////////////

		public object divert( string divName )
		{
			// ******
			GetOutput.Divert( divName );
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object undivert( params string [] args )
		{
			bool clear = CoreMacros.FirstElementMatches( "clear", ref args, true );
			return GetOutput.Undivert( args, clear );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object fetchDivert( string divName, bool clear )
		{
			return GetOutput.FetchDivert( divName, clear );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object includeDivert( string divName, bool clear )
		{
			// ******
			string text = GetOutput.FetchDivert( divName, clear );

			// ******
			IInput input = mp.CurrentMacroArgs.Input;
			if( null != input ) {
				input.IncludeText( text, divName );
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object saveDivert( string fileName, string divName, bool clearDiv )
		{
			return saveDivert( fileName, divName, clearDiv, false );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object saveDivert( string fileName, string divName, bool clearDiv, bool append )
		{
			// ******
			if( string.IsNullOrEmpty(fileName) ) {
				ThreadContext.MacroError( "save divert expected a file name" );
			}

			if( ! Path.IsPathRooted(fileName) ) {
				fileName = string.Format( @"{0}\{1}", gc.GetDirectoryStack().Peek(), fileName );
			}

			if( ! FileHelpers.IsValidFileName(fileName) ) {
				ThreadContext.MacroError( "save divert found an invalid path characters in: \"{0}\"", fileName );
			}

			// ******
			string text = GetOutput.FetchDivert( divName, clearDiv );

			MasterOutput output = mp.OutputInstance as MasterOutput;
			StringBuilder sb = output.FinalProcessText( new StringBuilder(text), false );
			text = sb.ToString();

			try {
				if( append ) {
					File.AppendAllText( fileName, text );
				}
				else {
					File.WriteAllText( fileName, text );
				}
			}
			catch ( IOException ex ) {
				ThreadContext.MacroError( "save divert was unable to write to file \"{0}\": {1}", fileName, ex.Message );
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object dumpDivert( string divName, bool toOutput )
		{
			// ******
			string text = GetOutput.FetchDivert( divName, false );

			// ******
			string result = string.Empty;

			if( toOutput) {
				ThreadContext.WriteMessage( text );
			}
			else {
				NamedTextBlocks blocks = gc.GetTextBlocks();
				result = blocks.AddTextBlock( text );
			}

			// ******
			return result;
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public Divert( IMacroProcessor mp, Builtin builtins )
		//{
		//	this.mp = mp;
		//	this.builtins = builtins;
		//}
		//

	}

}
