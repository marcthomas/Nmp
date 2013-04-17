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
using System.Reflection;

using Microsoft.JScript;
using Microsoft.JScript.Vsa;

using System.Threading;

using NmpBase;

#pragma warning disable 618


#if (!MONO)

namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {


		/////////////////////////////////////////////////////////////////////////////

		private MethodInfo GetEvaluateMethod( Type type, string name, int nArgs )
		{
			// ******
			MethodInfo [] mia = type.GetMethods();
			foreach( var mi in mia ) {
				if( name == mi.Name && nArgs == mi.GetParameters().Length ) {
					return mi;
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		private Assembly jscriptDll;
		private object	vsa;
		private MethodInfo evalMethInfo;

		private object Evaluate( string text )
		{
			// ******
			if( null == vsa ) {
				try {
					jscriptDll = Assembly.Load( new AssemblyName("Microsoft.JScript, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a") );
					if( null == jscriptDll ) {
						throw new Exception("");
					}
				}
				catch {	//( Exception ex ) {
					ThreadContext.MacroWarning( "\"#eval\" will not be invoked, unable to load Microsoft.JScript.dll" );
					return string.Empty;
				}

				// ******
				try {
					//
					// vsa = VsaEngine.CreateEngine();
					//
					Type type = jscriptDll.GetType( "Microsoft.JScript.Vsa.VsaEngine", true, false );
					
					MethodInfo methInfo = type.GetMethod( "CreateEngine", new Type [0] );
					if( null == methInfo ) {
						throw new Exception("");
					}

					vsa = methInfo.Invoke( null, null ); 
					if( null == vsa ) {
						throw new Exception("");
					}
				}
				catch ( Exception ex ) {
					ThreadContext.MacroWarning( "\"#eval\" will not be invoked, unable to initialize VsaEngine instance: {0}", ex.Message );
					return string.Empty;
				}
			}
		
			// ******
			try {
				//
				// object result = Eval.JScriptEvaluate( text, "unsafe", vsa );
				//
				if( null == evalMethInfo ) {
					Type type = jscriptDll.GetType( "Microsoft.JScript.Eval", true, false );
					evalMethInfo = GetEvaluateMethod( type, "JScriptEvaluate", 3 );
					
					if( null == evalMethInfo ) {
						throw new Exception("");
					}
				}

				// ******
				object result = evalMethInfo.Invoke( null, new object [] {text, "unsafe", vsa} ); 
				return null == result ? string.Empty : result;
			}
			catch ( Exception ex ) {
				//
				// never returns
				//
				ThreadContext.WriteMessage( "#eval string: {0}", text );
				ThreadContext.MacroError( ExceptionHelpers.RecursiveMessage( ex, "error executing #eval" ) );
				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object eval( params string [] strs )
		{
			// ******
			if( 0 == strs.Length ) {
				ThreadContext.MacroWarning( "empty expression passed to \"$.eval\"" );
				return string.Empty;
			}

			// ******
			string text = strs[ 0 ];
			if( strs.Length > 1 ) {
				var sb = new StringBuilder();
				foreach( string s in strs ) {
					sb.Append( s );
				}
				text = sb.ToString();
			}

			
			// ******
			return Evaluate( text );

		}


	}


}
#endif