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

namespace NmpBase {

	/////////////////////////////////////////////////////////////////////////////

	public class TokenMap {

		// ******
		bool		IsRegExMatch = false;

		// ******
		public	string	Token;
		public	int			MatchLength;
		public	bool		IsAltTokenFormat;

		// ******
		public	NmpStringList	RegExCaptures = new NmpStringList();


		/////////////////////////////////////////////////////////////////////////////

		public TokenMap( bool isRegExMatch = false )
		{
			IsRegExMatch = isRegExMatch;
		}


	}
}
