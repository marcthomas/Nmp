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
//using System.Reflection;
using System.Text.RegularExpressions;

//using Microsoft.JScript;
//using Microsoft.JScript.Vsa;
//
// removed refs from project as well
//

using NmpBase;


#pragma warning disable 414
#pragma warning disable 618


namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////

	 partial class TextMacroRunner : IDisposable {

		/////////////////////////////////////////////////////////////////////////////

		//private VsaEngine	vsa;

		public string EvalResult( string result )
		{
			// ******
			if( string.IsNullOrEmpty(result) ) {
				return string.Empty;
			}

//#if (!MONO)
//			// ******
//			//if( null == vsa ) {
//				VsaEngine vsa = VsaEngine.CreateEngine();
//			//}
//
//			// ******
//			if( null == vsa ) {
//				ThreadContext.MacroWarning( "unable to initialize VsaEngine instance, can not evaluate macro result" );
//				return result;
//			}
//
//			// ******
//			try {
//				//object objResult = Eval.JScriptEvaluate( "(" + result + ")", "unsafe", vsa );
//
//				object objResult = Eval.JScriptEvaluate( result, "unsafe", vsa );
//				return null == objResult ? SC.MACRO_NULL : objResult.ToString();
//			}
//			catch ( Exception ex ) {
//				//
//				// never returns
//				//
//				ThreadContext.WriteMessage( "Evaluate macro result string: {0}", result );
//				ThreadContext.MacroError( Helpers.RecursiveMessage(ex, "error executing JScriptEvaluate on macro result") );
//				return null;
//			}
//#else
//	return result;
//#endif



			return mp.InvokeMacro( "#eval", new object [] {result}, false ).ToString();


		}


	}


}

