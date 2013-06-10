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
using NmpEvaluators;


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

		private object PopMacro( string macroName )
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

		private object UndefineMacro( string macroName )
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
		/// <summary>
		/// Outputs the contents of a macro into the ouput or error stream
		/// </summary>
		/// <param name="dumpToOutput">If true outputs to the error stream</param>
		/// <param name="names">Macro names to dump, '*' dumps all</param>
		/// <returns></returns>

		[Macro]
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
		/// <summary>
		/// Defines a mach
		/// </summary>
		/// <param name="macroName">Macho name where value should be placed</param>
		/// <param name="macroObject">Text or an object (`@someObject')</param>
		/// <param name="argNames">Options names for arguments passed to macro</param>
		/// <returns></returns>

		[Macro]
		public object define( string macroName, object macroObject, params string [] argNames )
		{
			return DefineMacro( mp.CurrentMacroArgs, macroName, macroObject, argNames, false );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Pushes a macro with the same name on a macro stack and defines a new macro
		/// with the same name
		/// </summary>
		/// <param name="macroName"></param>
		/// <param name="macroObject"></param>
		/// <param name="argNames"></param>
		/// <returns></returns>

		[Macro]
		public object push( string macroName, object macroObject, params string [] argNames )
		{
			return DefineMacro( mp.CurrentMacroArgs, macroName, macroObject, argNames, true );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Removes a macro from the macro stack for 'macroName'
		/// </summary>
		/// <param name="macroName"></param>
		/// <returns></returns>

		[Macro]
		public object pop( string macroName )
		{
			return PopMacro( macroName );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Same as 'pop'
		/// </summary>
		/// <param name="macroName"></param>
		/// <returns></returns>

		[Macro]
		public object popdef( string macroName )
		{
			return pop( macroName );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Undefines a macro
		/// If the macro has been pushed works the same as 'pop', otherwise the macro
		/// is removed
		/// </summary>
		/// <param name="macroName"></param>
		/// <returns></returns>

		[Macro]
		public object undef( string macroName )
		{
			return UndefineMacro( macroName );
		}


		/////////////////////////////////////////////////////////////////////////////

		//[Macro]
		//public object dumpdef( params string [] macroNames )
		//{
		//	bool toOutput = FirstElementMatches( "output", ref macroNames, true );
		//	return DumpMacro( toOutput, macroNames );
		//}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public object dumpdef( bool dumpToOutput, params string [] macroNames )
		//{
		//	return DumpMacro( dumpToOutput, macroNames );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Echos its arguments back
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		[Macro]
		public object echo( params string [] args )
		{
			return EchoArguments( args );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// If running under the VS debugger breaks before evaluating and processing
		/// the next macro parsed from the input
		/// </summary>

		[Macro]
		public void BreakNext()
		{
			gc.BreakNext = true;
		}

		/////////////////////////////////////////////////////////////////////////////

		public CoreMacros( IMacroProcessor mp )
			:	base(mp)
		{
			this.gc = Get<GrandCentral>();
		}

	}



}
