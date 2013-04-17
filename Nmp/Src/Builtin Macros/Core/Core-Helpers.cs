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
		
		private NMP GetNMP()
		{
			return Get<NMP>();
		}
		

		/////////////////////////////////////////////////////////////////////////////

		private static bool HasFlag( string specificFlag, string [] flags )
		{
			// ******
			if( null != flags && ! string.IsNullOrEmpty(specificFlag) ) {
				foreach( var flag in flags ) {
					if( 0 == string.Compare(specificFlag, flag.Trim(), true) ) {
						return true;
					}
				}
			}

			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static bool FirstElementMatches( string strToMatch, ref string [] items, bool remove )
		{
			// ******
			if( null == items || 0 == items.Length ) {
				return false;
			}

			if( 0 == string.Compare(strToMatch, items[0].Trim(), true) ) {
				if( remove ) {
					//
					// we remove the first item
					//
					int newLength = items.Length - 1;
					var newItems = new string [ newLength ];

					if( newItems.Length > 0 ) {
						Array.Copy( items, 1, newItems, 0, newLength );
					}
					
					// ******
					items = newItems;
				}

				// ******
				return true;
			}

			// ******
			return false;
		}


	}


}
