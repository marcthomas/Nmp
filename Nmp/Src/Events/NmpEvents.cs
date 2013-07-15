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

	//public struct ConversionError {

	//	public Type FromType { get; set; }
	//	public Type ConvertToType { get; set; }

	//	/////////////////////////////////////////////////////////////////////////////

	//	public static void Write( Type from, Type to )
	//	{
	//		var evt = new ConversionError {
	//			FromType = from,
	//			ConvertToType = to
	//		};
	//		EventWriter.Error( evt );
	//	}
	//}


	/////////////////////////////////////////////////////////////////////////////

	class ParseArgumentsEvent {

		public enum State { None, Begin, End, Processing }

		public string Context { get; private set; }
		public State InState { get; private set; }
		public string Item { get; private set; }


		/////////////////////////////////////////////////////////////////////////////

		public string SetCurrentItem( string item )
		{
			Item = string.IsNullOrEmpty(item) ? string.Empty : item;
		
			InState = State.Processing;
			EventWriter.Information( this );

			return Item;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void BeginParseEvent()
		{
			InState = State.Begin;
			EventWriter.Information( this );
			InState = State.Processing;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void EndParseEvent()
		{
			InState = State.End;
			EventWriter.Information( this );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			// ******
			if( State.Processing == InState ) {
				return string.Format( "Argument \"{0}\"", Item );
			}
			else {
				return string.Format( "Parse Arguments Event for \"{0}\": {1}", Context, State.Begin == InState ? "start" : "end" );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public ParseArgumentsEvent( string context )
		{
			Context = context;
			Item = string.Empty;
		}

	}

	
	/////////////////////////////////////////////////////////////////////////////

	//class InvokeMacroEvent {

	//	public MIR MacroInvocationRecord { get; private set; }


	//	/////////////////////////////////////////////////////////////////////////////

	//	public string BasicInfo()
	//	{
	//		// ******
	//		var mir = MacroInvocationRecord;
	//		var sb = new StringBuilder { };

	//		// ******
	//		if( null == mir.Macro ) {
	//			sb.AppendFormat( "Root file: \"{0}\"\n", mir.SourceName );
	//		}
	//		else {
	//			string macroName = null != mir.Macro ? mir.Macro.Name : string.Empty;

	//			// ******
	//			string someMacroText = mir.Text.TrimStart();

	//			//char [] chars = new char [] { '\r', '\n' };

	//			//int index = 0;

	//			//while( 0 == (index = someMacroText.IndexOfAny( chars )) )
	//			//	;

	//			//if( index > 0 ) {
	//			//	someMacroText = string.Format( "{0}\n ...", someMacroText.Substring( 0, index ) );
	//			//}

	//			sb.AppendFormat( "\n {0}\n", someMacroText );
		
			
	//		}
			
	//		// ******
	//		return sb.ToString();
	//	}


	//	/////////////////////////////////////////////////////////////////////////////

	//	public override string ToString()
	//	{
	//		var macroArgs = MacroInvocationRecord.MacroArgs;

	//		// ******
	//		var sb = new StringBuilder { };
	//		{
	//			sb.Append( BasicInfo() );
	//		}
	//		return sb.ToString();
	//	}


	//	/////////////////////////////////////////////////////////////////////////////

	//	public static void Write( MIR mir )
	//	{
	//		var evt = new InvokeMacroEvent { MacroInvocationRecord = mir };
	//		EventWriter.Information( evt );
	//	}


	//}


}
