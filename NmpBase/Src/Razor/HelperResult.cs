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

namespace NmpBase.Razor {

//
// some info on templates can be found here:
//
//    http://vibrantcode.com/blog/category/razor
//

/*

	HOW THIS WORKS
	--------------

	Razor tears appart the inline template "expression" and writes calls to WriteLiteral()
	for the raw text pieces - it generates calls to Write() for the '@item' object

	@item IS ALWAYS treated as an object, it's value is item.ToString()


	Note the lambda function being created and passed to the new instance of
	HelperResult

	It is that code that is executed when the '_action' member of HelperResult
	is called.

	Write(	MyTest( item => new HelperResult( __razor_template_writer => {
																							HelperResult.WriteLiteral(@__razor_template_writer, " ");
																							HelperResult.Write(@__razor_template_writer, item);
																						}
																					)
								)
			);

	void Write( TextWriter writer, object obj )

		is what's called when the template wants to output '@item'


*/


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
				
				//
				// calling ToString() invokes the '_action' which causes those
				// HelperResult.Write() and HelperResult.WriteLiteral() calls
				// to happen
				//

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
