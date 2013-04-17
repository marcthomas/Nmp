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

	public class EscapedCharList : List<char> {

		///////////////////////////////////////////////////////////////////////////

		public const char	FirstEmbedEscape	= SC.EMBED_FIRST_ESCAPE_CHAR;
		public const char	LastEmbedEscape		= SC.EMBED_LAST_ESCAPE_CHAR;
		
		
		///////////////////////////////////////////////////////////////////////////
		
		//public char First	{ get { return (char) (FirstEmbedEscape + this.Count); } }
		//public char Last	{ get { return 0 == this.Count ? SC.NO_CHAR : (char) (FirstEmbedEscape + this.Count); } }
		
		
		///////////////////////////////////////////////////////////////////////////

		public char Get( char chSpecial )
		{
			// ******
			int index = chSpecial - FirstEmbedEscape;
			return index < 0 ? SC.NO_CHAR : this[ index ];
		}


		///////////////////////////////////////////////////////////////////////////

		public new char Add( char ch )
		{
			// ******
			for( int i = 0; i < this.Count; i++ ) {
				if( ch == this[i] ) {
					return (char) ((int)FirstEmbedEscape + i);
				}
			}
			
			char chSpecial = (char) (FirstEmbedEscape + this.Count);

			// ******
			base.Add( ch );
			return chSpecial;
		}
	
	}
}
