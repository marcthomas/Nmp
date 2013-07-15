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

//using NmpBase;


//#pragma warning disable 414

using NmpBase;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	public class InputSpan {

		public IInput Input { get; set; }
		public IReader Source { get { return Input.Current; } }

		/// <summary>
		/// current line in Source when InputSpan initialized
		/// </summary>
		public int StartLine { get; set; }

		/// <summary>
		/// current column in Source when InputSpan initialized
		/// </summary>
		public int StartColumn { get; set; }

		/// <summary>
		/// current line now
		/// </summary>
		public int CurrentLine { get { return Source.Line; } }

		/// <summary>
		/// current column now
		/// </summary>
		public int CurrentColumn { get { return Source.Column; } }

		/// <summary>
		/// start of span in Source
		/// </summary>
		public int SpanStart { get; private set; }

		/// <summary>
		/// end of span in Source - should be end of expression
		/// </summary>
		public int SpanEnd { get; private set; }

		/// <summary>
		/// end of span when macro is block macro
		/// </summary>
		public int ExpandedSpanEnd { get; private set; }


		/////////////////////////////////////////////////////////////////////////////

		public void SetSpanEnd( int index )
		{
			SpanEnd = index;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SetExpandedSpanEnd( int index )
		{
			ExpandedSpanEnd = index;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string InitialTextSpan()
		{
			return SpanStart < 0 || SpanEnd <= SpanStart ? string.Empty : Source.GetText( SpanStart, SpanEnd );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string TextSpan()
		{
			var endSpan = ExpandedSpanEnd > 0 ? ExpandedSpanEnd : SpanEnd;
			return SpanStart < 0 || endSpan <= SpanStart ? string.Empty : Source.GetText( SpanStart, endSpan );
		}


		/////////////////////////////////////////////////////////////////////////////

		public InputSpan( IInput input, int spanEnd = -1, int expandedSpanEnd = -1 )
		{
			Input = input;
			StartLine = input.Line;
			StartColumn = input.Column;
			SpanStart = input.Index;
			SpanEnd = spanEnd;
			ExpandedSpanEnd = expandedSpanEnd;
		}


	}


	/////////////////////////////////////////////////////////////////////////////

	class MIR : IMacroInvocationRecord {

		// ******
		//
		// IMacroInvocationRecord
		//
		public IMacro Macro { get; private set; }
		public string Context { get; private set; }
		public bool AltToken { get; private set; }

		public IMacroArguments MacroArgs { get; private set; }
		public NmpStringList SpecialArgs { get; private set; }

		// ******
		public bool PushbackCalled { get { return Input.PushbackCalled; } }
		public string SourceName { get { return Source.SourceName; } }

		public int SourceStartIndex { get { return inputSpan.SpanStart; } }
		public int SourceEndIndex { get { return inputSpan.SpanEnd; } }
		public int SourceExpandedEndIndex { get { return inputSpan.ExpandedSpanEnd; } }

		public string InitialTextSpan { get { return inputSpan.InitialTextSpan(); } }
		public string TextSpan { get { return inputSpan.TextSpan(); } }

		//
		// Macro CAN be null for the root (first) instance of MIR on the InvocationStack,
		// that instance of MIR is set by Evaluate() and MultipleEvaluate() in Evaluate.cs 
		// which setup the processing of files, not straight invoking of macros
		//
		// above required MIR to be pushed on InvocationStack by the evaluate methods, this
		// is no longer done

		public int Line { get { return null == Macro ? 0 : Source.Line; } }
		public int Column { get { return null == Macro ? 0 : Source.Column; } }

		// ******
		public bool CalledFromMacro { get; private set; }
		public bool CalledFromFile { get; private set; }

		// ******
		//public MacroProcessingState State { get; set; }

		// ******
		//
		// locals
		//
		InputSpan inputSpan;

		public IInput Input { get { return inputSpan.Input; } }
		public IReader Source { get { return inputSpan.Source; } }


		/////////////////////////////////////////////////////////////////////////////

		//public void SetSourceEndIndex( int pos )
		//{
		//	inputSpan.SetSpanEnd( pos );
		//}


		/////////////////////////////////////////////////////////////////////////////

		public string NameAndLocationString()
		{
			return string.Format( "[{0} {1}:{2}, {3}]", Macro.Name, SourceName, Line, Column );
		}


		/////////////////////////////////////////////////////////////////////////////


		public MIR( IMacro macro, TokenMap tm, InputSpan ispan, int spanEnd, int extendedSpanEnd, IMacroArguments macroArgs, string context )
		{
			// ******
			if( null == macro ) {
				throw new ArgumentNullException( "macro" );
			}

			if( null == tm ) {
				throw new ArgumentNullException( "tm" );
			}

			if( null == ispan ) {
				throw new ArgumentNullException( "ispan" );
			}

			if( string.IsNullOrEmpty(context) ) {
				throw new ArgumentNullException( "context" );
			}

			// ******
			inputSpan = ispan;
			inputSpan.SetSpanEnd( spanEnd );
			inputSpan.SetExpandedSpanEnd( extendedSpanEnd );

			Macro = macro;
			Context = context;
			AltToken = tm.IsAltTokenFormat;

			MacroArgs = macroArgs;
			SpecialArgs = tm.RegExCaptures;

			// ******
			CalledFromMacro = string.IsNullOrEmpty( inputSpan.Source.SourceName );
			CalledFromFile = !CalledFromMacro;
		}


		/////////////////////////////////////////////////////////////////////////////

		public MIR( IMacro macro, IInput input, MacroArguments macroArgs, string context )
		{
			// ******
			if( null == macro ) {
				throw new ArgumentNullException( "macro" );
			}

			if( null == input ) {
				throw new ArgumentNullException( "input" );
			}

			if( string.IsNullOrEmpty( context ) ) {
				throw new ArgumentNullException( "context" );
			}
		
			// ******
			inputSpan = new InputSpan( input );

			Macro = macro;
			Context = context;
			AltToken = false;

			MacroArgs = macroArgs;
			SpecialArgs = null;

			// ******
			CalledFromMacro = string.IsNullOrEmpty( inputSpan.Source.SourceName );
			CalledFromFile = !CalledFromMacro;
		}
			
	}


}