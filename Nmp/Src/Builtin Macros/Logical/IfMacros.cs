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
		//
		// #ifTrue( object, trueResult, falseResult )
		//
		/////////////////////////////////////////////////////////////////////////////

		public object True( object obj, object isTrueResult, params object [] others )
		{
			bool isTrue = null == obj ? false : Helpers.IsMacroTrue( obj );
			return isTrue ? isTrueResult : (0 == others.Length ? string.Empty : others[0]);
		}


		/////////////////////////////////////////////////////////////////////////////

		public object @true( object obj, object isTrueResult, params object [] others )
		{
			return True( obj, isTrueResult, others );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #ifFalse( bool, falseResult, trueResult )
		//
		/////////////////////////////////////////////////////////////////////////////

		public object False( object obj, object isFalseResult, params object [] others )
		{
			bool isFalse = null == obj ? true : ! Helpers.IsMacroTrue( obj );
			return isFalse ? isFalseResult : (0 == others.Length ? string.Empty : others[0]);
		}


		/////////////////////////////////////////////////////////////////////////////

		public object @false( object obj, object isFalseResult, params object [] others )
		{
			return False( obj, isFalseResult, others );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #ifelse( cmp1, cmp2, eq1, neq1 [cmp3, cmp4, eq2, neq2 ...
		//
		// if there are only 2 arguments at the top of the loop true
		// or false will be returned
		//
		/////////////////////////////////////////////////////////////////////////////

		public object Else( params object [] argsIn )
		{
			// ******
			var args = new NmpObjectList( argsIn );
				
			// ******			
			while( true ) {
				int numstrs = args.Count;
				
				// ******
				if( numstrs < 2 ) {
					ThreadContext.MacroError( "in the current iteration of the #ifelse() macro there are only 0 or 1 arguments, at least 2 are required" );
				}

				// ******
				bool match = args[0].Equals( args[1] );

				if( 2 == numstrs ) {
					return match;
				}
			
				if( match ) {
					return args[2];
				}
				else {
					if( numstrs >= 6 ) {
						args.RemoveRange( 0, 3 );
					}
					else {
						return numstrs >= 4 ? args[3] : string.Empty;
					}
				}
			}


		}


		/////////////////////////////////////////////////////////////////////////////

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

		public object Empty( params string [] argsIn )
		{
			// ******
			var args = new NmpStringList( argsIn );

			// ******
			if( 0 == args.Count ) {
				return false;
			}
			
			// ******
			if( string.IsNullOrEmpty(args.NextArg()) ) {
				//
				// if the next argument exist then return it, else return true
				//
				if( 0 == args.Count ) {
					return true;
				}
				else {
					return args.NextArg();
				}
			}
			else {
				//
				// if there are two strings then return the second (false)
				//
				if( args.Count > 1 ) {
					return args[ 1 ];
				}
				else {
					return false;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #ifnotempty( string, ifNotValue, ifNotValue )
		//
		// true or false are returned if there are not
		// enough return arguments
		//
		/////////////////////////////////////////////////////////////////////////////

		public object NotEmpty( params string [] argsIn )
		{
			// ******
			var args = new NmpStringList( argsIn );

			// ******
			if( 0 == args.Count ) {
				return false;
			}
			
			// ******
			if( ! string.IsNullOrEmpty(args.NextArg()) ) {
				//
				// if the next argument exist then return it, else return true
				//
				if( 0 == args.Count ) {
					return true;
				}
				else {
					return args.NextArg();
				}
			}
			else {
				//
				// if there are two strings then return the second (false)
				//
				if( args.Count > 1 ) {
					return args[ 1 ];
				}
				else {
					return false;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #ifDefined
		//
		// true or false are returned if there are not
		// enough return arguments
		//
		/////////////////////////////////////////////////////////////////////////////

		public object Defined( params string [] argsIn )
		{
			// ******
			var args = new NmpStringList( argsIn );

			// ******
			if( 0 == args.Count ) {
				return false;
			}
			
			// ******
			if( mp.IsMacroName(args[0]) ) {
				if( args.Count > 1 ) {
					return args[ 1 ];
				}
				else {
					return true;
				}
			}
			else {
				if( args.Count > 2 ) {
					return args[ 2 ];
				}
				else {
					return false;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #ifndef
		//
		// return true or false
		//
		/////////////////////////////////////////////////////////////////////////////

		public object NotDefined( params string [] args )
		{
			// ******
			if( 0 == args.Length ) {
				return false;
			}
			
			// ******
			if( ! mp.IsMacroName(args[0]) ) {
				if( args.Length > 1 ) {
					return args[ 1 ];
				}
				else {
					return true;
				}
			}
			else {
				if( args.Length > 2 ) {
					return args[ 2 ];
				}
				else {
					return false;
				}
			}

		}


		/////////////////////////////////////////////////////////////////////////////

		public IfMacros( IMacroProcessor mp )
			:	base(mp)
		{
			//this.mp = mp;
		}


	}


}
