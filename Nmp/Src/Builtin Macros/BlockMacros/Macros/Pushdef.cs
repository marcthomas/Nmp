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

namespace Nmp.Builtin.Macros {

	/////////////////////////////////////////////////////////////////////////////


	class Pushdef : Defmacro {

		//
		// no hash in name because its alt format only
		//
		public const string	PUSHDEF		= "pushdef";


		/////////////////////////////////////////////////////////////////////////////

		public Pushdef( IMacroProcessor mp, CoreMacros builtins )
			: base( PUSHDEF, mp, builtins )
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public static new IMacro Create( IMacroProcessor mp, CoreMacros builtins )
		{
			// ******
			var o = new Pushdef( mp, builtins );
			IMacro macro = mp.CreateBuiltinMacro( o.Name, o );
			macro.Flags |= MacroFlags.AltTokenFmtOnly | MacroFlags.RequiresArgs;

			return macro;
		}

	}
}
