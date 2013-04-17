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
using System.Globalization;
using System.IO;

namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////	

	//public interface IHtmlString {
	//	// Methods
	//	string ToHtmlString();
	//}


	/////////////////////////////////////////////////////////////////////////////	

	public class HelperResult {	//: IHtmlString {

		private readonly Action<TextWriter> _action;


		///////////////////////////////////////////////////////////////////////////////	
		//
		//public void WriteTo(TextWriter writer)
		//{
		//	this._action(writer);
		//}
		//

		///////////////////////////////////////////////////////////////////////////////	
		//
		//public string ToHtmlString()
		//{
		//	return this.ToString();
		//}
		//

		/////////////////////////////////////////////////////////////////////////////	

		public override string ToString()
		{
			using( StringWriter writer = new StringWriter( CultureInfo.InvariantCulture ) ) {
				this._action( writer );
				return writer.ToString();
			}
		}


		/////////////////////////////////////////////////////////////////////////////	

		public HelperResult( Action<TextWriter> action )
		{
			if( action == null ) {
				throw new ArgumentNullException( "action" );
			}
			this._action = action;
		}


		/////////////////////////////////////////////////////////////////////////////	

		// is this doing the "right thing" vis-a-vi what WebPages does ? - aside from encoding ?


		public static void Write( TextWriter writer, object obj )
		{
			writer.Write( obj );
		}


		/////////////////////////////////////////////////////////////////////////////	

		public static void WriteLiteral( TextWriter writer, string y )
		{
			writer.Write( y );
		}


	}
}
