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
		/// <summary>
		/// Clears the diversion to an empty string
		/// </summary>
		/// <param name="divName">Name of diversion</param>
		/// <returns>empty string</returns>

		[Macro]
		public object clearDivert( string divName )
		{
			GetOutput.ClearDivert( new List<string>() {divName} );
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Pushes the current diversion on a stack and sets a new diversion
		/// </summary>
		/// <param name="divName">Name of diversion to switch too</param>
		/// <returns>empty string</returns>

		[Macro]
		public object pushDivert( string divName )
		{
			GetOutput.PushDivert( divName );
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// All lower case name for pushDivert
		/// </summary>
		/// <param name="divName"></param>
		/// <returns></returns>

		[Macro]
		public object pushdivert( string divName )
		{
			return pushDivert( divName );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Pops the diversion stack
		/// </summary>
		/// <returns>empty string</returns>

		[Macro]
		public object popDivert()
		{
			GetOutput.PopDivert();
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// All lower case name for popDivert
		/// </summary>
		/// <returns></returns>

		[Macro]
		public object popdivert()
		{
			return popDivert();
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Changes to a new diversion
		/// </summary>
		/// <param name="divName">Name of diversion to switch too</param>
		/// <returns>empty string</returns>

		[Macro]
		public object divert( string divName )
		{
			// ******
			GetOutput.Divert( divName );
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Retreives the text from all diversions requested and clears them
		/// </summary>
		/// <param name="args">List of diversion names</param>
		/// <returns>Text of the diversions</returns>

		[Macro]
		public object undivert( params string [] args )
		{
			bool clear = CoreMacros.FirstElementMatches( "clear", ref args, true );
			return GetOutput.Undivert( args, clear );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Fetches the text of a single diversion and optionaly clears it
		/// </summary>
		/// <param name="divName"></param>
		/// <param name="clear"></param>
		/// <param name="count"></param>
		/// <param name="prependStr"></param>
		/// <param name="suppressWarnings"></param>
		/// <returns></returns>

		[Macro]
		public object fetchDivert( string divName, bool clear, int count, string prependStr, bool suppressWarnings )
		{
			return GetOutput.FetchDivert( divName, clear, count, prependStr, suppressWarnings );
		}


		[Macro]
		public object fetchDivert( string divName, bool clear, int count, string prependStr )
		{
			return GetOutput.FetchDivert( divName, clear, count, prependStr, false );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Fetches the text of a single diversion and optionaly clears it
		/// </summary>
		/// <param name="divName">Name of diversion</param>
		/// <param name="clear">Clear diversion if true</param>
		/// <returns>Text of the diversion</returns>

		[Macro]
		public object fetchDivert( string divName, bool clear )
		{
			return fetchDivert( divName, clear, 0, string.Empty );
		}


		[Macro]
		public object fetchDivert( string divName, bool clear, bool suppressWarnings )
		{
			return fetchDivert( divName, clear, 0, string.Empty, suppressWarnings );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Need to test this, no longer sure what it does!
		/// I think it appends the diversion to the input if an IInput is attached
		/// to the macro arguments associated with the current (includeDivert) macro
		/// </summary>
		/// <param name="divName"></param>
		/// <param name="clear"></param>
		/// <returns></returns>

		[Macro]
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
		/// <summary>
		/// Saves the diversion to a text file
		/// </summary>
		/// <param name="fileName">Output file name, relative to currently active input
		/// file or absolute (if full path provided)</param>
		/// <param name="divName">Diversion to save</param>
		/// <param name="clearDiv">If true clear diversion</param>
		/// <returns></returns>

		[Macro]
		public object saveDivert( string fileName, string divName, bool clearDiv )
		{
			return saveDivert( fileName, divName, clearDiv, false );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Saves the diversion to a text file, optionally appending rather than replacing the file contents
		/// </summary>
		/// <param name="fileName">Output file name, relative to currently active input
		/// file or absolute (if full path provided)</param>
		/// <param name="divName">Diversion to save</param>
		/// <param name="clearDiv">If true clear diversion</param>
		/// <param name="append">If true appends the text to file name</param>
		/// <returns></returns>
		/// 
		[Macro]
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
		/// <summary>
		/// Outputs the contents of the diversion to the output or error stream
		/// </summary>
		/// <param name="divName">Diversion name</param>
		/// <param name="toOutput">If true the text goest to the error stream, otherwise placed
		/// in the output of macro processor is a way that it will receive not further processing
		/// </param>
		/// <returns></returns>

		[Macro]
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
