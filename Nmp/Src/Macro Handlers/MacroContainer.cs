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

using NmpBase;


namespace Nmp {

	class MacroContainer : IMacroContainer, IHub {

		protected IMacroProcessor mp;

		/////////////////////////////////////////////////////////////////////////////

		public virtual string [] ObjectMacroNames
		{
			//
			// classes that inherit from MacroContainer will generaly not implement
			// this
			// 
			get
			{
				return new string [0];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Initialize( IMacroProcessor mp )
		{
			this.mp = mp;
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Get<T>() where T : class
		{
			return mp.Get<T>();
		}


		/////////////////////////////////////////////////////////////////////////////

		public MacroContainer( IMacroProcessor mp )
		{
			Initialize( mp );
		}
	}
}
