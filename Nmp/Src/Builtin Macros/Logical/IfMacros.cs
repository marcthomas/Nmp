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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


using NmpBase;


#pragma warning disable 618

namespace Nmp.Builtin.Macros {



	/////////////////////////////////////////////////////////////////////////////

	class IfMacros : MacroContainer {

		//IMacroProcessor mp;


		/////////////////////////////////////////////////////////////////////////////

		object Select( bool firstArg, object [] args )
		{
			return firstArg ? (args.Length < 2 ? string.Empty : args [ 1 ]) : (args.Length < 3 ? string.Empty : args [ 2 ]);
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a value depending upon the truthfulness of the first argument
		/// </summary>
		/// <param name="args">Two or three arguments; first is value to evaluate; second is result if macro evaluates to true; optional third argument is returned for false, an empty string is returned if there is no third argument</param>
		/// <returns>arg[1] for true, arg[2] for false; if arg [1] or [2] does not exist then the empty string</returns>

		[Macro]
		public object True( params object [] args )
		{
			// ******
			if( args.Length < 2 ) {
				ThreadContext.MacroError( "#if.True requires at least two arguments: #if.True(valueToCheck, trueResult [, falseResult])" );
			}

			// ******
			return Select( Helpers.IsMacroTrue(args[0]), args );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// See True()
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		[Macro]
		public object @true( params object [] args )
		{
			return True( args );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a value depending upon the truthfulness of the first argument
		/// </summary>
		/// <param name="args">Two or three arguments; first is value to evaluate; second is result if macro evaluates to true; optional third argument is returned for false, an empty string is returned if there is no third argument</param>
		/// <returns>arg[1] for true, arg[2] for false; if arg [1] or [2] does not exist then the empty string</returns>

		[Macro]
		public object False( params object [] args )
		{
			// ******
			if( args.Length < 2 ) {
				ThreadContext.MacroError( "#if.False requires at least two arguments: #if.False(valueToCheck, trueResult [, falseResult])" );
			}

			// ******
			return Select( ! Helpers.IsMacroTrue( args [ 0 ] ), args );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// See False()
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		[Macro]
		public object @false( params object [] args )
		{
			return False( args );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #ifelse( cmp1, cmp2, eq1, neq1 [cmp3, cmp4, eq2, neq2 ...
		//
		// if there are only 2 arguments at the top of the loop true
		// or false will be returned
		//
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Checks a cascading sequence of value for true or false and returns a value
		/// 
		/// Compares the next two values in the input object array where the first pair are argsIn[0] and argsIn[1]; if there are three values and the result is true then the third value is returned and if false an empty string is returned; when there are four values and the result is true then the third value is returned, if false the fourth value is returned. When there are six or more values and the result is false then the first three are discarded and processing begins over.
		/// </summary>
		/// <param name="argsIn">Values to compare and/or return</param>
		/// <returns></returns>

		[Macro]
		public object Else( params object [] argsIn )
		{
			// ******
			var args = new NmpObjectList( argsIn );

			// ******
			if( args.Count < 3 ) {
				ThreadContext.MacroError( "#if.Else requires at least three arguments: #if.Else(lhsValue, rhsValue, trueResult [, falseResult ...])" );
			}

			// ******			
			while( true ) {
				int numstrs = args.Count;
				if( numstrs < 3 ) {
					ThreadContext.MacroError( "in the current iteration of the #if.Else() there are fewer than 3 arguments, at least 3 are required:\n lhsValue, rhsValue, a result when comparison is true" );
				}

				// ******
				bool match = args [ 0 ].Equals( args [ 1 ] );
				if( match ) {
					return args [ 2 ];
				}
				else {
					if( numstrs >= 6 ) {
						args.RemoveRange( 0, 3 );
					}
					else {
						return numstrs >= 4 ? args [ 3 ] : string.Empty;
					}
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// See Else()
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		[Macro]
		public object @else( params object [] args )
		{
			return Else( args );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #ifempty( string, ifEmptyValue, ifNotEmptyValue )
		//
		// true or false are returned if there are not
		// enough return arguments
		//
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a value depending upon the truthfulness of the first argument
		/// </summary>
		/// <param name="args">Two or three arguments; first is value to evaluate; second is result if macro evaluates to true; optional third argument is returned for false, an empty string is returned if there is no third argument</param>
		/// <returns>arg[1] for true, arg[2] for false; if arg [1] or [2] does not exist then the empty string</returns>

		[Macro]
		public object Empty( params string [] args )
		{
			// ******
			if( args.Length < 2 ) {
				ThreadContext.MacroError( "#if.Empty requires at least two arguments: #if.Empty(valueToCheck, trueResult [, falseResult])" );
			}

			// ******
			return Select( string.IsNullOrEmpty(args[0]), args );
		}

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a value depending upon the truthfulness of the first argument
		/// </summary>
		/// <param name="args">Two or three arguments; first is value to evaluate; second is result if macro evaluates to true; optional third argument is returned for false, an empty string is returned if there is no third argument</param>
		/// <returns>arg[1] for true, arg[2] for false; if arg [1] or [2] does not exist then the empty string</returns>

		[Macro]
		public object NotEmpty( params string [] args )
		{
			// ******
			if( args.Length < 2 ) {
				ThreadContext.MacroError( "#if.NotEmpty requires at least two arguments: #if.NotEmpty(valueToCheck, trueResult [, falseResult])" );
			}

			// ******
			return Select( ! string.IsNullOrEmpty( args [ 0 ] ), args );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a value depending upon the truthfulness of the first argument
		/// </summary>
		/// <param name="args">Two or three arguments; first is value to evaluate; second is result if macro evaluates to true; optional third argument is returned for false, an empty string is returned if there is no third argument</param>
		/// <returns>arg[1] for true, arg[2] for false; if arg [1] or [2] does not exist then the empty string</returns>

		[Macro]
		public object Defined( params string [] args )
		{
			// ******
			if( args.Length < 2 ) {
				ThreadContext.MacroError( "#if.Defined requires at least two arguments: #if.Defined(valueToCheck, trueResult [, falseResult])" );
			}

			// ******
			return Select( mp.IsMacroName( args [ 0 ] ), args );
		}

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a value depending upon the truthfulness of the first argument
		/// </summary>
		/// <param name="args">Two or three arguments; first is value to evaluate; second is result if macro evaluates to true; optional third argument is returned for false, an empty string is returned if there is no third argument</param>
		/// <returns>arg[1] for true, arg[2] for false; if arg [1] or [2] does not exist then the empty string</returns>

		[Macro]
		public object NotDefined( params string [] args )
		{
			// ******
			if( args.Length < 2 ) {
				ThreadContext.MacroError( "#if.NotDefined requires at least two arguments: #if.NotDefined(valueToCheck, trueResult [, falseResult])" );
			}

			// ******
			return Select( !mp.IsMacroName( args [ 0 ] ), args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IfMacros( IMacroProcessor mp )
			: base( mp )
		{
			//this.mp = mp;
		}


	}


}
