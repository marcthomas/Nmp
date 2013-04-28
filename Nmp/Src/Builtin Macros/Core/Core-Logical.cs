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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

using Microsoft.JScript;
using Microsoft.JScript.Vsa;


using NmpBase;
using Nmp;
using Global;


#pragma warning disable 618

namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Negates the result of the input value
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Negated result</returns>

		[Macro]
		public bool not( string value )
		{
			return ! Helpers.IsMacroTrue(value);
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates all arguments passed for truthfullness
		/// </summary>
		/// <param name="args">arguments</param>
		/// <returns>true if all arguments evaluate to true</returns>

		[Macro]
		public bool allTrue( params object [] args )
		{
			// ******
			if( 0 == args.Length ) {
				return false;
			}

			foreach( var arg in args ) {
				if( !Helpers.IsMacroTrue( arg ) ) {
					return false;
				}
			}

			return true;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates all arguments passed for truthfullness
		/// </summary>
		/// <param name="args">arguments</param>
		/// <returns>true if any argument evaluates to true</returns>

		[Macro]
		public bool anyTrue( params object [] args )
		{
			// ******
			if( 0 == args.Length ) {
				return false;
			}

			foreach( var arg in args ) {
				if( Helpers.IsMacroTrue( arg ) ) {
					return true;
				}
			}

			return false;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates all arguments passed for truthfullness
		/// </summary>
		/// <param name="args">arguments</param>
		/// <returns>true if all arguments evaluate to true</returns>

		[Macro]
		public bool allFalse( params object [] args )
		{
			// ******
			if( 0 == args.Length ) {
				return false;
			}

			foreach( var arg in args ) {
				if( Helpers.IsMacroTrue( arg ) ) {
					return false;
				}
			}

			return true;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates all arguments passed for truthfullness
		/// </summary>
		/// <param name="args">arguments</param>
		/// <returns>true if any argument evaluates to true</returns>

		[Macro]
		public bool anyFalse( params object [] args )
		{
			// ******
			if( 0 == args.Length ) {
				return false;
			}

			foreach( var arg in args ) {
				if( !Helpers.IsMacroTrue( arg ) ) {
					return true;
				}
			}

			return false;
		}


	
	
	
	}
}
