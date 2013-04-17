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

	//class JSONBail : Exception {
	//}


	/////////////////////////////////////////////////////////////////////////////

	public class NmpJSONException : Exception {

		JSONParser jp;

		/////////////////////////////////////////////////////////////////////////////

		public NmpJSONException( JSONParser parser, string message, params object [] args )
			:	base( Helpers.SafeStringFormat(message, args) )
		{
			this.jp = parser;
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	public interface JSONItemHandler {

		// ******
	 	//void Error( string msgFmt, params object [] args );
		void Warning( string msgFmt, params object [] args );

		// ******
		void EnterObject(JSONParser jp );
		void ExitObject(JSONParser jp );
		void EnterArray(JSONParser jp );
		void ExitArray(JSONParser jp );
		void Identifier( JSONParser jp, string identifier );
		void Value( JSONParser jp, object value );
	}




}
