#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
#if NET40
using System;
using System.Globalization;
using System.IO;
using System.Text;

using System.Linq;

//using System.Web.Razor.Generator;

using RazorHosting;
using NmpBase;

namespace Nmp {
 
	/// <summary>
	/// Base class used for Razor Page Templates - Razor generates
	/// a class from the parsed Razor markup and this class is the
	/// base class. Class must implement an Execute() method that 
	/// is overridden by the parser and contains the code that generates
	/// the markup.  Write() and WriteLiteral() must be implemented
	/// to handle output generation inside of the Execute() generated
	/// code.
	/// 
	/// This class can be subclassed to provide custom functionality.
	/// One common feature likely will be to provide Context style properties
	/// that are application specific (ie. HelpBuilderContext) and strongly
	/// typed and easily accesible in Razor markup code.   
	/// </summary>


	/*

http://www.west-wind.com/weblog/posts/2010/Dec/27/Hosting-the-Razor-Engine-for-Templating-in-NonWeb-Applications

http://www.google.com/search?hl=en&q=how+to+implement+@section+for+standalone+Razor&aq=f&aqi=&aql=&oq=

http://vibrantcode.com/blog/category/razor

	http://vibrantcode.com/blog/2010/11/16/hosting-razor-outside-of-aspnet-revised-for-mvc3-rc.html

	http://stackoverflow.com/questions/5937000/net-razor-engine-implementing-layouts


https://github.com/NancyFx/Nancy/tree/master/src/Nancy.ViewEngines.Razor

http://msdn.microsoft.com/en-us/library/system.web.razor.generator.generatedclasscontext(v=VS.99).aspx


WriteLiteralTo:

	http://blog.slaks.net/search/label/Razor

	http://msdn.microsoft.com/en-us/library/gg547967(v=VS.99).aspx


*/

	/////////////////////////////////////////////////////////////////////////////	

	public class NmpRazorTemplateBase : RazorTemplateBase {

		// ******
		public IMacroProcessor	mp;		
		
		// ******
		//
		// used by DefineSection() / RenderSection()
		//
		public object [] args = null;



		///////////////////////////////////////////////////////////////////////////////	
		//
		//public string Render( string name )
		//{
		//	return name;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////	

		public string Trim()
		{
			// ******
			var text = FileReader.FixText( Response.GetWriterText() );
			Response.SetWriterText( text.Trim() );

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////	

		public string RemoveEmptyLines()
		{
			// ******
			var endOfLineStr = "\n";
			var text = FileReader.FixText( Response.GetWriterText() );

			// ******
			string [] lines = text.ToString().Split( new string [] {endOfLineStr}, StringSplitOptions.RemoveEmptyEntries );
			
			var sb = new StringBuilder();
			foreach( var line in lines ) {
				sb.AppendLine( line );
			}
			text = sb.ToString();

			// ******
			Response.SetWriterText( text );

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////	

		public bool IsSectionDefined( string name )
		{
			IMacro macro;
			return mp.FindMacro( name, out macro );
		}


		/////////////////////////////////////////////////////////////////////////////	

		public object RenderSection( string name, bool required = true, params object [] args )
		{
			// ******
			IMacro macro;
			if( ! mp.FindMacro(name, out macro) ) {
				if( required ) {
					throw new Exception( string.Format("section not found: {0}", name) );
				}
				return string.Empty;
			}

			// ******
			return mp.InvokeMacro( macro, args, false );
		}


		/////////////////////////////////////////////////////////////////////////////	

		public object RenderMacro( string name, params object [] args )
		{
			// ******
			IMacro macro;
			if( ! mp.FindMacro(name, out macro) ) {
				ThreadContext.MacroWarning( "Razor RenderMacro() could not locate the macro: {0}", name );
				return string.Empty;
			}

			// ******
			return mp.InvokeMacro( macro, args, false );
		}


		/////////////////////////////////////////////////////////////////////////////	
		
		public void DefineSection( string name, Action action )
		{
			/*

			Anonymous Functions 

			Lambda Expression

			*/

			// ******
			//
			// 'args' can be passed in as the arguments passed to RenderSection() after
			// the the 'required' parameter
			//
			// or as the normal arguments if invoked as a macro
			//
			Func<object [], string> wrapper =
			
				(args) =>
				{
					this.args = args;

					var temp = Response;
					Response = new RazorResponse();

					action();

					string result = FileReader.FixText( Response.GetText );
					Response = temp;

					return result;
				};

			// ******
			mp.AddObjectMacro( name, wrapper );
		}
				
			
		///////////////////////////////////////////////////////////////////////////////	
		////
		////
		//// used by Template expansion - names set in RazorEngine.CreateHost()
		////
		////
		///////////////////////////////////////////////////////////////////////////////	
		////
		//// MOVED INTO HelperResult
		////
		//
		//// is this doing the "right thing" vis-a-vi what WebPages does ? - aside from encoding ?
		//
		//
		//public void Write( TextWriter writer, object obj )
		//{
		//	writer.Write( obj );
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////	
		//
		//public void WriteLiteral( TextWriter writer, string y )
		//{
		//	writer.Write( y );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////	

		public NmpRazorTemplateBase()
		{
			mp = null;
			args = null;
		}


	}

}
#endif