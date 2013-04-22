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
		/// Nmp version
		/// </summary>

		[Macro]
		public object version
		{
			get {
				return Assembly.GetExecutingAssembly().GetName().Version;
			}
		}


	}


}
