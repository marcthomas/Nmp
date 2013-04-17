#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
using System.Text;
using System.IO;
using System;
using System.Diagnostics;

namespace NmpBase.Razor {

	///////////////////////////////////////////////////////////////////////////////	

	public interface IRazorBase {

		void Initialize();
		void Execute();
		RazorResponse	Response	{ get; set; }

	}

}
