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
using Nmp.Expressions;


namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	abstract class BlockMacroHandlerBase : BuiltinMacroHandler {

		// ******
		protected abstract IEnumerable<Type> 		ExpectedArgTypes	{ get; }
		protected abstract IEnumerable<object>	DefaultArgs { get; }


		/////////////////////////////////////////////////////////////////////////////

		//protected MacroArgs GetMacroArgs( Expression exp, IEnumerable<Type> argTypes = null, IEnumerable<object> defArgs = null )
		//{
		//	// ******
		//	//
		//	// arguments from expression for current macro
		//	//
		//	ArgumentList argList = GetArguments( Name, exp );

		//	// ******
		//	//
		//	// intialize MacroArgs with types and defaults that were passed in to us,
		//	// or from "default" properties
		//	//
		//	return new MacroArgs( mp, argList, Name, null == argTypes ? ExpectedArgTypes : argTypes, null == defArgs ? DefaultArgs : defArgs );
		//}


		protected MacroArgs GetMacroArgs( Expression exp )
		{
			// ******
			//
			// arguments from expression for current macro
			//
			ArgumentList argList = GetArguments( Name, exp );

			// ******
			//
			// intialize MacroArgs with types and defaults that were passed in to us,
			// or from "default" properties
			//
			return new MacroArgs( mp, argList, Name, ExpectedArgTypes, DefaultArgs );
		}


		/////////////////////////////////////////////////////////////////////////////

		//protected object GetMacroArgsAsTuples( Expression exp, IEnumerable<Type> argTypes = null, IEnumerable<object> defArgs = null )
		//{
		//	// ******
		//	//
		//	// arguments from expression for current macro
		//	//
		//	ArgumentList argList = GetArguments( Name, exp );

		//	// ******
		//	//
		//	// intialize MacroArgs with types and defaults that were passed in to us,
		//	// or from "default" properties
		//	//
		//	var macroArgs = new MacroArgs( mp, Name, null == argTypes ? ExpectedArgTypes : argTypes, null == defArgs ? DefaultArgs : defArgs );
		//	return macroArgs.AsTuples( argList );
		//}


		protected object GetMacroArgsAsTuples( Expression exp )
		{
			// ******
			//
			// arguments from expression for current macro
			//
			ArgumentList argList = GetArguments( Name, exp );

			// ******
			//
			// intialize MacroArgs with types and defaults that were passed in to us,
			// or from "default" properties
			//
			var macroArgs = new MacroArgs( mp, Name, ExpectedArgTypes, DefaultArgs );
			return macroArgs.AsTuples( argList );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected BlockMacroHandlerBase( string macroName, IMacroProcessor mp )
			: base(mp)
		{
			Name = macroName;
		}

	}
}
