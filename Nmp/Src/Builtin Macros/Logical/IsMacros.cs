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

	class IsMacros : MacroContainer {

		//IMacroProcessor mp;


		/////////////////////////////////////////////////////////////////////////////
		//
		// #isempty( string )
		//
		/////////////////////////////////////////////////////////////////////////////

		string Combine( string value, string [] extra )
		{
			if( 0 == extra.Length ) {
				return value;
			}

			var sb = new StringBuilder( value );
			foreach( var item in extra ) {
				sb.Append( item );
			}

			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Checks if the combination of all the passed arguments are empty
		/// </summary>
		/// <param name="args"></param>
		/// <returns>True if empty string, otherwise false</returns>
		/// 
		[Macro]
		public object Empty( params string [] args )
		{
			var value = Combine( string.Empty, args );
			return string.IsNullOrEmpty( value );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Checks if the combination of all the passed arguments is not empty
		/// </summary>
		/// <param name="args"></param>
		/// <returns>Returns true if not empty string, otherwise false</returns>

		[Macro]
		public object NotEmpty( params string [] args )
		{
			var value = Combine( string.Empty, args );
			return !string.IsNullOrEmpty( value );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Compares to objects
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns>Returns true if the two objects are the same, otherwise false</returns>

		[Macro]
		public object Equal( object lhs, object rhs )
		{
			return lhs.Equals( rhs );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Compares two objects
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns>Returns true if the two objects are the same, otherwise false</returns>

		[Macro]
		public object NotEqual( object lhs, object rhs )
		{
			return ! lhs.Equals( rhs );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates the first (or emtpy) argument
		/// </summary>
		/// <param name="args"></param>
		/// <returns>True if true, false otherwise</returns>

		[Macro]
		public object True( params object [] args )
		{
			return 0 == args.Length ? false : Helpers.IsMacroTrue( args[0] );
		}

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates the first (or emtpy) argument
		/// </summary>
		/// <param name="args"></param>
		/// <returns>True if true, false otherwise</returns>

		[Macro]
		public object False( params object [] args )
		{
			return 0 == args.Length ? true :  !Helpers.IsMacroTrue( args[0] );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates the first (or emtpy) argument
		/// </summary>
		/// <param name="args"></param>
		/// <returns>True if defined, false otherwise</returns>

		[Macro]
		public object Defined( params string [] args )
		{
			return 0 == args.Length ? false : mp.IsMacroName( args[0] );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #isnotdefined
		//
		// return true or false
		//
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluates the first (or emtpy) argument
		/// </summary>
		/// <param name="args"></param>
		/// <returns>True if true not defined, false otherwise</returns>

		[Macro]
		public object NotDefined( params string [] args )
		{
			return 0 == args.Length ? true : !mp.IsMacroName( args [ 0 ] );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IsMacros( IMacroProcessor mp )
			:	base(mp)
		{
		}


	}


}
