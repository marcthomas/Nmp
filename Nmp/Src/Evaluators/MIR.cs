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

	 class MIR : IMacroInvocationRecord {

		// ******
		int _line;
		int _column;

		// ******
		public bool	CalledFromMacro		{ get; private set; }
		public bool	CalledFromFile		{ get; private set; }

		// ******
		public	MacroProcessingState	State				{ get; set; }
		public	IMacroArguments				MacroArgs		{ get; set; }
		public	NmpStringList					SpecialArgs	{ get; set; }

		// ******
		public	IMacro	Macro							{ get; private set; }
		public	IReader	Source						{ get; private set; }

		public	bool		PushbackCalled		{ get; private set; }

		public	string	Context						{ get; private set; }

		public	string	SourceName				{ get { return Source.SourceName; } }

		public	int			SourceStartIndex 	{ get; private set; }
		public	int			SourceEndIndex		{ get; private set; }
		public	bool		AltToken					{ get; private set; }

		public	string	Text							{ get { return Source.GetText(SourceStartIndex, SourceEndIndex); } }

		// ******
		//public	NmpStringList	Instructions	{ get; set; }


		/////////////////////////////////////////////////////////////////////////////

		public	int	Line							{ get { return null == Macro ? Source.Line : _line; } }
		public	int	Column						{ get { return null == Macro ? Source.Column : _column; } }


		/////////////////////////////////////////////////////////////////////////////

		public void SetSourceEndIndex( int pos )
		{
			SourceEndIndex = pos;
		}


//		/////////////////////////////////////////////////////////////////////////////
//
//		public void SetInstructions( NmpStringList list )
//		{
//			Instructions = list;
//		}
//

		/////////////////////////////////////////////////////////////////////////////

		public string NameAndLocationString()
		{
			return string.Format( "[{0} {1}:{2}, {3}]", Macro.Name, SourceName, Line, Column );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MIR( IMacro macro, bool isAltTokenFormat, NmpStringList specialArgs, IInput inputSource, string context, int pos, int line, int column )
		{
			// ******
			CalledFromMacro = string.IsNullOrEmpty( inputSource.SourceName );
			CalledFromFile = ! CalledFromMacro;

			// ******
			State = MacroProcessingState.None;
			MacroArgs = null;
			SpecialArgs = specialArgs;

			// ******
			Macro = macro;
			Source = inputSource.Current;
			PushbackCalled = inputSource.PushbackCalled;

			// ******
			Context = context;
			SourceStartIndex = pos;
			SourceEndIndex = 0;
			_line = line;
			_column = column;
			AltToken = isAltTokenFormat;

		}


		/////////////////////////////////////////////////////////////////////////////

		public MIR( IMacro macro, IInput inputSource, string context )
		{
			// ******
			CalledFromMacro = string.IsNullOrEmpty( inputSource.SourceName );
			CalledFromFile = ! CalledFromMacro;

			// ******
			State = MacroProcessingState.None;
			MacroArgs = null;
			SpecialArgs = null;

			// ******
			//
			// sometimes called with 'macro' null
			//
			Macro = macro;
			Source = inputSource.Current;
			PushbackCalled = inputSource.PushbackCalled;

			// ******
			Context = context;
			SourceStartIndex = 0;
			SourceEndIndex = 0;
			_line = 0;
			_column = 0;
			AltToken = false;
		}


	}


}