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
using System.Linq;
using System.Text;

using NmpBase;
using NmpExpressions;


namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros : MacroContainer {

		// ******
		//static string [] macroNames = { "$", "$nmp" };

		// ******
		//protected IMacroProcessor mp;
		//protected Hub hub;
		
		GrandCentral gc;


		/////////////////////////////////////////////////////////////////////////////

		//public string [] ObjectMacroNames
		//{
		//	get {
		//		return macroNames;
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////

		public string DefineMacro( IMacroArguments macroArgs, string macroName, object macroObject, IList<string> argNames, bool isPushMacro )
		{
			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				ThreadContext.MacroError( "define macro expected the name of a macro" );
			}

			// ******
			IMacro newMacro = null;

			string macroText = macroObject as string;
			if( null != macroText ) {
				newMacro = mp.CreateTextMacro( macroName, macroText, argNames );
			}
			else {
				IMacro objMacro = macroObject as IMacro;

				if( null != objMacro ) {
					newMacro = objMacro.MacroHandler.Create( macroName, objMacro, null, false );
				}
				else {
					newMacro = mp.CreateObjectMacro( macroName, macroObject );
				}
			}

			// ******
			if( isPushMacro ) {
				IMacro existingMacro;
				if( mp.FindMacro(newMacro.Name, out existingMacro) ) {
					//
					// our reference will stay alive
					//
					mp.DeleteMacro( existingMacro );
					//
					// "push" it
					//
					newMacro.Pushed = existingMacro;
				}
			}

			// ******
			IMacro callingMacro = Get<InvocationStack>().GetCallingMacro();
			if( null != callingMacro && MacroType.Text == callingMacro.MacroType ) {
				newMacro.SourceFile = callingMacro.SourceFile;
				newMacro.MacroStartLine = callingMacro.MacroStartLine + macroArgs.Input.Line;
			}
			else {
				newMacro.SourceFile = macroArgs.Input.SourceName;
				newMacro.MacroStartLine = macroArgs.Input.Line;
			}

			if( macroArgs.Options.NoExpression ) {
				newMacro.Flags |= MacroFlags.NonExpressive;
			}

			// ******
			mp.AddMacro( newMacro );

			// ******
			return string.Empty;
		}

		/////////////////////////////////////////////////////////////////////////////

		public object PopMacro( string macroName )
		{
			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				ThreadContext.MacroError( "pop macro expected the name of a macro" );
			}

			// ******
			IMacro existingMacro;
			if( mp.FindMacro(macroName, out existingMacro) ) {
				//
				// get Pushed
				//
				IMacro pushedMacro = existingMacro.Pushed;

				if( null != pushedMacro ) {
					//
					// remove existingMacro and add the Pushed macro
					//
					mp.DeleteMacro( existingMacro );
					mp.AddMacro( pushedMacro );
				}
			}
			else {
				ThreadContext.MacroWarning( "attempt to undef a macro that does not exist in the macro dictionar: {0}", macroName );
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object UndefineMacro( string macroName )
		{
			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				ThreadContext.MacroError( "undefine macro expected the name of a macro" );
			}

			// ******
			mp.UndefMacro( macroName );

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		private void DumpDefFormat( StringBuilder sb, IMacro macro )
		{
			// ******
			string text = string.Empty;
			//StringBuilder sb = new StringBuilder();

			// ******
			if( MacroType.Text == macro.MacroType ) {
				text = macro.MacroText;
			}
			else if( MacroType.Builtin == macro.MacroType ) {
				text = string.Format( "Builting macro: {0}", macro.Name );
			}
			else {
				object obj = macro.MacroObject;
				if( null == obj ) {
					text = "null macro object";
				}
				else {
					text = obj.ToString();
				}
			}

			sb.AppendLine();
			sb.AppendLine( "*****************************************************************************" );
			sb.AppendFormat( "dumpdef \"{0}\", MacroType is {1}: (begins two lines down)\n\n", macro.Name, macro.MacroType );
//				sb.AppendLine( "*****************************************************************************" );
//				sb.AppendLine( "-----------------------------------------------------------------------------" );
			sb.AppendLine( text );
			sb.AppendLine( "*****************************************************************************" );

//			return sb;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object DumpMacro( bool dumpToOutput, IList<string> macroNames )
		{
			// ******
			if( 0 == macroNames.Count ) {
				ThreadContext.MacroError( "dump macro expteced one or more macro names, you can use '*' to dump all user macros" );
			}

			// ******
			string result = string.Empty;
			StringBuilder sb = new StringBuilder();

			if( "*" == macroNames[0] ) {
				List<IMacro> list = mp.GetMacros( true );
	
				foreach( IMacro m in list ) {
					DumpDefFormat( sb, m );
				}

			}
			else {
				IMacro existingMacro;
				foreach( string macroName in macroNames ) {
					if( mp.FindMacro(macroName.Trim(), out existingMacro) ) {
						DumpDefFormat( sb, existingMacro );
					}
					else {
						ThreadContext.MacroWarning( "attempt to dump a macro that does not exist in the macro dictionary: {0}", macroName );
					}
				}
			}

			// ******
			if( dumpToOutput) {
				ThreadContext.WriteMessage( sb.ToString() );
			}
			else {
				NamedTextBlocks blocks = gc.GetTextBlocks();
				result = blocks.AddTextBlock( sb );
			}

			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object dumpMacro( bool dumpToOutput, params string [] names )
		{
			return DumpMacro( dumpToOutput, names );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object EchoArguments( IEnumerable<string> args )
		{
			// ******
			StringBuilder sb = new StringBuilder();

			int count = 0;
			foreach( var arg in args ) {
				sb.AppendFormat( "{0}{1}", count++ > 0 ? ", " : string.Empty, arg );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public object define( string macroName, object macroObject, params string [] argNames )
		{
			return DefineMacro( mp.CurrentMacroArgs, macroName, macroObject, argNames, false );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object push( string macroName, object macroObject, params string [] argNames )
		{
			return DefineMacro( mp.CurrentMacroArgs, macroName, macroObject, argNames, true );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object pop( string macroName )
		{
			return PopMacro( macroName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object popdef( string macroName )
		{
			return pop( macroName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object undef( string macroName )
		{
			return UndefineMacro( macroName );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object dumpdef( params string [] macroNames )
		{
			bool toOutput = FirstElementMatches( "output", ref macroNames, true );
			return DumpMacro( toOutput, macroNames );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public object dumpdef( bool dumpToOutput, params string [] macroNames )
		//{
		//	return DumpMacro( dumpToOutput, macroNames );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public object echo( params string [] args )
		{
			return EchoArguments( args );
		}


		/////////////////////////////////////////////////////////////////////////////

		// [Macro( "#breakNext" )]
		public void BreakNext()
		{
			gc.BreakNext = true;
		}

		///////////////////////////////////////////////////////////////////////////////
		//
		//public object clearDivert( string divName )
		//{
		//	return diverter.clearDivert( divName );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object pushDivert( string divName )
		//{
		//	return diverter.pushDivert( divName );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object pushdivert( string divName )
		//{
		//	return diverter.pushDivert( divName );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object popDivert()
		//{
		//	return diverter.popDivert();
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object popdivert()
		//{
		//	return diverter.popDivert();
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object divert( string divName )
		//{
		//	return diverter.divert( divName );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object undivert( params string [] args )
		//{
		//	return diverter.undivert( args );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object fetchDivert( string divName, bool clear )
		//{
		//	return diverter.fetchDivert( divName, clear );
		//}
		//
		//
		//public object includeDivert( string divName, bool clear )
		//{
		//	return diverter.includeDivert( divName, clear );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object saveDivert( string fileName, string divName, bool clearDiv, bool append )
		//{
		//	return diverter.saveDivert( fileName, divName, clearDiv, append );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public object dumpDivert( string divName, bool toOutput )
		//{
		//	return diverter.dumpDivert( divName, toOutput );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public CoreMacros( IMacroProcessor mp )
			:	base(mp)
		{
			this.gc = Get<GrandCentral>();
		}

	}



}
