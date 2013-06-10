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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Reflection;


using NmpBase;
using NmpExpressions;

namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////

	class TextMacroHandler : BuiltinMacroHandler {	// IMacroHandler {

		//protected IMacroProcessor	mp;



		/////////////////////////////////////////////////////////////////////////////

		//public string	Name 						{ get { return "text macro handler"; } }
		//public bool		HandlesBlocks		{ get { return false; } }


		///////////////////////////////////////////////////////////////////////////////
		//
		//public virtual string ParseBlock( Expression exp, IInput input )
		//{
		//	return string.Empty;
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public IMacro Create( string name, IMacro macro, object instance, bool clone )
		//{
		//	return mp.CreateTextMacro( name, macro.MacroText, macro.ArgumentNames );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		//public object Evaluate( IMacro macro, IMacroArguments macroArgs )
		//{
		//	// ******
		//	string macroName = macro.Name;
		//
		//	// ******
		//	//
		//	// does not return unless validation succeeds
		//	//
		//	int skipCount;
		//	ArgumentList argList = MacroHandlerBase.ValidateExpression( macro, macroArgs, out skipCount );
		//	object [] objArgs = ArgumentsEvaluator.Evaluate( mp, argList );
		//
		//	// ******
		//	if( macroArgs.Options.Data ) {
		//		//
		//		// data - no substitution and no scanning
		//		//
		//		return MacroHandlerBase.EvaluateRemainingExpressions( mp, macro.MacroText, (MacroExpression) macroArgs.Expression, skipCount );
		//	}
		//	else {
		//		//
		//		// not data - do it the normal way
		//		//
		//		using( var macroRunner = new TextMacroRunner(mp, macro, macroArgs, objArgs) ) {
		//			string result = macroRunner.Run();
		//			return MacroHandlerBase.EvaluateRemainingExpressions( mp, result, (MacroExpression) macroArgs.Expression, skipCount );
		//		}
		//	}
		//}
		//

		public override object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			string macroName = macro.Name;

			// ******
			//
			// does not return unless validation succeeds
			//
			int skipCount;
			ArgumentList argList = GetArguments( macro.Name, macroArgs.Expression, out skipCount );
			object [] objArgs = ArgumentsEvaluator.Evaluate( mp, argList );

			// ******
			if( macroArgs.Options.Data ) {
				//
				// data - no substitution and no scanning
				//
				return EvaluateRemainingExpressions( mp, macro.MacroText, (MacroExpression) macroArgs.Expression, skipCount );
			}
			else {
				//
				// not data - do it the normal way
				//
				using( var macroRunner = new TextMacroRunner(mp, macro, macroArgs, objArgs) ) {
					string result = macroRunner.Run();
					return EvaluateRemainingExpressions( mp, result, (MacroExpression) macroArgs.Expression, skipCount );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public TextMacroHandler( IMacroProcessor mp )
			:	base(mp)
		{
			//this.mp = mp;
		}


	}


}
