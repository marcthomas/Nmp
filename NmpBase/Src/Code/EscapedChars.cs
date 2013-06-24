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



	///////////////////////////////////////////////////////////////////////////

	public class EscapedCharList {	// : List<char> {

		List<char> chars = new List<char> { };

		///////////////////////////////////////////////////////////////////////////

		public const char	FirstEmbedEscape	= SC.EMBED_FIRST_ESCAPE_CHAR;
		public const char	LastEmbedEscape		= SC.EMBED_LAST_ESCAPE_CHAR;


		///////////////////////////////////////////////////////////////////////////

		public void Clear()
		{
			chars.Clear();
		}


		///////////////////////////////////////////////////////////////////////////

		public char Get( char chSpecial )
		{
			// ******
			int index = chSpecial - FirstEmbedEscape;
			try {
				return index < 0 ? SC.NO_CHAR : chars [ index ];
			}
			catch( Exception ex ) {
				throw ex;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public new char Add( char ch )
		{
			// ******
			for( int i = 0; i < chars.Count; i++ ) {
				if( ch == chars[i] ) {
					return (char) ((int)FirstEmbedEscape + i);
				}
			}
			
			char chSpecial = (char) (FirstEmbedEscape + chars.Count);

			// ******
			chars.Add( ch );
			return chSpecial;
		}


		///////////////////////////////////////////////////////////////////////////

		public EscapedCharList()
		{
		}
	
	}
}
