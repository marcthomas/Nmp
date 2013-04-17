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

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////
	
	public class ExecutionSecurity {
	
		const bool		DEF_LOCKDOWN = false;
		const bool		DEF_RESTRICT_CODE = false;
		const bool		DEF_RESTRICT_READ = false;
		const bool		DEF_RESTRICT_NETOBJECTS = false;
		const bool		DEF_RESTRICT_SETPARAMS = false;
		const int			DEF_MAX_PARSE_ITEMS = 1024;
		
		const int			MAX_MACRO_SIZE	= 50000;
		
		// ******
		//
		// security
		//
		public	bool	LockDown						{ get; set; }
		public	bool	RestrictCode				{ get; set; }
		public	bool	RestrictRead				{ get; set; }
		public	bool	RestrictNetObjects	{ get; set; }
		public	bool	RestrictSetParam		{ get; set; }
		
		//
		// max number of strings that can be returned by split, extract, etc.
		//
		public	int		MaxParseItems				{ get; set; }
		public	int		MaxRecursionDepth		{ get; set; }
		public	int		MaxExecTime					{ get; set; }
	
	
	}

}
