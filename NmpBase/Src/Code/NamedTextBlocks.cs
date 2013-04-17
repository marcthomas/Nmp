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


namespace NmpBase {



	/////////////////////////////////////////////////////////////////////////////

	public class NamedTextBlocks : Dictionary<string, string> {

		public const char	TextBlockStartChar = SC.EMBED_TEXT_BLOCK;
		public const int	NameLength					= 5;

		int	keyIndex = 1;


		/////////////////////////////////////////////////////////////////////////////

		public string AddTextBlock( string textBlock )
		{
			string key = string.Format( "{0}{1:x4}", TextBlockStartChar, keyIndex++ );
			Add( key, textBlock );
			return key;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string AddTextBlock( StringBuilder sb )
		{
			return AddTextBlock( sb.ToString() );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetTextBlock( string key )
		{
			string value;
			if( TryGetValue(key, out value) ) {
				return value;
			}

			// ******
			return null;
		}

	}


}
