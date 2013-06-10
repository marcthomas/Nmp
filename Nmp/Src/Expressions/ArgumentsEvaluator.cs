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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


//#pragma warning disable 414


using NmpBase;
using Nmp;


namespace NmpExpressions {



	/////////////////////////////////////////////////////////////////////////////

	class ArgumentsEvaluator {

		IMacroProcessor macroProcessor;


		/////////////////////////////////////////////////////////////////////////////

		protected object ExecuteMacro( string name, MacroExpression exp )
		{
			IMacro macro;
			if( ! macroProcessor.FindMacro(name, out macro) ) {
				ThreadContext.MacroError( "\"{0}\" is not the name of a macro", name );
			}

			// ******
			//using( var input = macroProcessor.ScanHelper.NewEmptyIInput ) {
			//	return macroProcessor.ProcessMacro( macro, new MacroArguments(macro, input, exp), false );
			//}
			return macroProcessor.InvokeMacro( macro, exp, false );
		}


		/////////////////////////////////////////////////////////////////////////////

		private object EvalArgumentConstant( ArgumentExpression arg )
		{
			return arg.ConstantValue;
		}


		/////////////////////////////////////////////////////////////////////////////

		private object EvalArgumentExpression( ArgumentExpression arg )
		{
			return ExecuteMacro( arg.Name, (MacroExpression) arg.Expression );
		}


		/////////////////////////////////////////////////////////////////////////////

		private object EvalArgumentConvert( ArgumentExpression arg )
		{
			// ******
			Type castToType = arg.ExpressionCastType;
			object value = ExecuteMacro( arg.Name, (MacroExpression) arg.Expression );
		
			// ******

			//
			// HAVE TO CAST value
			//

/*
	implicit operator / explicit operator on a class

		IConvertible

		Convert class

			- use in arguments instead of our code

*/

			try {
				value = Arguments.ChangeType( value, castToType );
			}
			catch ( Exception ex ) {
				ThreadContext.MacroError( ex.Message );
			}

			// ******
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		private object [] Evaluate( ArgumentList args )
		{
			// ******
			object [] list = new object [ args.Count ];

			// ******
			for( int iArg = 0; iArg < args.Count; iArg++ ) {
				ArgumentExpression arg = args[ iArg ];

				// ******
				object objResult = null;

				switch( arg.ArgumentType ) {
					case ArgumentType.Constant:
						objResult = EvalArgumentConstant( arg );
						break;

					case ArgumentType.Expression:
						objResult = EvalArgumentExpression( arg );
						break;

					case ArgumentType.Convert:
						objResult = EvalArgumentConvert( arg );
						break;
				}

				// ******
				list[ iArg ] = objResult;
			}

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected ArgumentsEvaluator( IMacroProcessor mp )
		{
			macroProcessor = mp;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object [] Evaluate( IMacroProcessor mp, ArgumentList args )
		{
			// ******
			var preProcessedArgs = args.PreProcessedArgs;
			if( null != preProcessedArgs ) {
				return preProcessedArgs;
			}

			// ******
			ArgumentsEvaluator ae = new ArgumentsEvaluator( mp );
			return ae.Evaluate( args );
		}


	}



}
