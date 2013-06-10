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
using NmpEvaluators;
using NmpExpressions;

#pragma warning disable 169

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	class ForloopHandler {

		IMacroProcessor mp;

		
		/////////////////////////////////////////////////////////////////////////////

		protected static void AddExtraArgs( int startIndex, object [] args, object [] extraArgs )
		{
			for( int i = startIndex; i < args.Length; i++ ) {
				args[ i ] = extraArgs[ i - startIndex ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ProcessForLoop( IMacro target, int start, int end, int increment, object [] extraArgs )
		{
			// ******
			if( 0 == increment ) {
				ThreadContext.MacroError( "#forloop increment value ({0}) must not be zero.", increment );
			}
			else if( increment > 0 && end < start ) {
				ThreadContext.MacroWarning( "#forloop end value is less than start value (start: {0} end: {1} increment: {2}).", start, end, increment );
			}
			else if( increment < 0 && start < end ) {
				ThreadContext.MacroWarning( "#forloop start value is less than end value (start: {0} end: {1} increment: {2}).", start, end, increment );
			}

			// ******
			StringBuilder sb = new StringBuilder();

			// ******
			object [] argsToMacro = new object[3 + extraArgs.Length ];
			AddExtraArgs( 3, argsToMacro, extraArgs );

			// ******
			MacroExpression expression = ETB.CreateMacroCallExpression( target, argsToMacro );

			// ******
			argsToMacro[ 1 ] = end;
			argsToMacro[ 2 ] = increment;

			for( int counter = start; ; counter += increment ) {
				if( increment > 0 && counter > end ) {
					break;
				}
				else if( increment < 0 && counter < end ) {
					break;
				}
			
				// ******
				//
				//	`index', `lastIndex', `increment' [, extras ...]
				//
				argsToMacro[ 0 ] = counter;
				sb.Append( mp.InvokeMacro(target, expression, true) );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string Forloop_TextChunk( int start, int end, int increment, string macroText, object [] extraArgs )
		{
			// ******
			//
			//	`index', `lastIndex', `increment' [, extra0, extra1, ...]
			//
			var argNames = new NmpStringList( "index", "lastIndex", "increment" );
			//
			// extra0 ... extraN
			//
			for( int i = 0; i < extraArgs.Length; i++ ) {
				argNames.Add( string.Format( "extra{0}", i ) );
			}

			// ******
			IMacro target = mp.AddTextMacro( mp.GenerateMacroName("$.forloop"), macroText, argNames );
			string result = ProcessForLoop( target, start, end, increment, extraArgs );
			mp.DeleteMacro( target );

			// ******
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ForloopHandler( IMacroProcessor mp )
		{
			this.mp = mp;
		}

	}
}
