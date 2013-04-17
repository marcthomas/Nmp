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
		//
		// #forloop( `start', `end', `increment', `macroName' [, extraArgs ...] )
		//
		/////////////////////////////////////////////////////////////////////////////

		public object forloop( int start, int end, int increment, string macroName, params object [] extraArgs )
		{
			// ******
			if( string.IsNullOrEmpty(macroName) ) {
				ThreadContext.MacroError( "$.forloop requires the name of a macro to invoke" );
			}

			// ******
			var handler = new ForloopHandler( mp );

			// ******
			if( '!' == macroName[0] || '&' == macroName[0] ) {
				return handler.Forloop_TextChunk( start, end, increment, macroName.Substring(1), extraArgs );
			}

			// ******
			IMacro target;
			if( ! mp.FindMacro(macroName, out target) ) {
				ThreadContext.MacroError( "$.forloop could not locate the macro: \"{0}\"", macroName );
			}

			// ******
			return handler.ProcessForLoop( target, start, end, increment, extraArgs );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		//
		// #foreach( `object', `macroName' [, extraArgs ...] )
		//
		/////////////////////////////////////////////////////////////////////////////

		public object @foreach( object objToEnumerate, string macroToCall, params object [] extraArgs )
		{
			// ******
			if( null == objToEnumerate ) {
				ThreadContext.MacroError( "$.foreach requires an object to iterate over as its first argument" );
			}

			// ******
			if( string.IsNullOrEmpty(macroToCall) ) {
				ThreadContext.MacroError( "$.foreach requires the name of a macro to invoke" );
			}

			// ******
			var handler = new ForeachHandler( mp );

			if( '!' == macroToCall[0] || '&' == macroToCall[0] ) {
				return handler.Foreach_TextChunk( objToEnumerate, macroToCall.Substring(1), extraArgs );
			}

			// ******
			IMacro target;
			if( ! mp.FindMacro(macroToCall, out target) ) {
				ThreadContext.MacroError( "$.foreach could not locate the macro: \"{0}\"", macroToCall );
			}

			// ******
			if( MacroType.Text == target.MacroType ) {
				return handler.ForeachTextMacro( target, objToEnumerate, extraArgs );
			}
			else {
				return handler.ForeachObjectMacro( target, objToEnumerate, extraArgs );
			}
		}

	}
}
