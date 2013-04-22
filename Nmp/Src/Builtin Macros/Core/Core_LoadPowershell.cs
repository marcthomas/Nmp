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
using Nmp;

#pragma warning disable 618

namespace Nmp.Builtin.Macros {


	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {

		const string PSInterfaceDllName = "PSInterface.dll";

		
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Do not use, has not been fully implemented in v3 of Nmp
		/// </summary>
		/// <param name="pathToDll"></param>
		/// <returns></returns>

		//[Macro]
		//public object loadPowershell( string pathToDll = null )
		//{
		//	// ******
		//	if( string.IsNullOrEmpty(pathToDll) ) {
		//		pathToDll = Path.Combine( LibInfo.CodeBasePath, PSInterfaceDllName );
		//	}

		//	// ******
		//	string message = string.Empty;

		//	try {
		//		//
		//		// note: powershell interface must register itself by calling mp.RegisterPowershell()
		//		//
		//		if( AutoRegisterMacros.RegisterMacroContainers(mp, pathToDll, false) ) {
		//			return string.Empty;
		//		}
		//	}
		//	catch ( Exception ex ) {
		//		message = ex.Message;
		//	}

		//	// ******
		//	ThreadContext.MacroError( "unable to load Powershell interface DLL \"{0}\"; {1}", pathToDll, message );
		//	return null;
		//}



	}
}
