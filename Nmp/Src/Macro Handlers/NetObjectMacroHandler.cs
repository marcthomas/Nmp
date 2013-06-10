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
using NmpExpressions;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	class NetObjectMacroHandler : IMacroHandler {

		IMacroProcessor	mp;

		bool handlesBlocks = false;


		/////////////////////////////////////////////////////////////////////////////

		public string Name { get { return "net object macro handler"; } }


		/////////////////////////////////////////////////////////////////////////////

		public bool HandlesBlocks	{ get { return handlesBlocks; } }


		/////////////////////////////////////////////////////////////////////////////
		
		public virtual string ParseBlock( Expression exp, IInput input )
		{
			return string.Empty;
		}



		/////////////////////////////////////////////////////////////////////////////

		public IMacro Create( string name, IMacro macro, object instance, bool clone )
		{
			return mp.CreateObjectMacro( name, macro.MacroObject );
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public virtual object Evaluate( IMacro macro, IMacroArguments macroArgs )
		{
			// ******
			var oee = new ExpressionEvaluator( mp );
			return oee.Evaluate( macro, macroArgs.Expression );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NetObjectMacroHandler( IMacroProcessor mp )
		{
			this.mp = mp;
		}   

	}


}
