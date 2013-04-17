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

using NmpBase;

namespace Nmp {



	/////////////////////////////////////////////////////////////////////////////
	
	class DelegateContainer {

		public string InstanceStringGuy()
		{
			return "instance string guy";
		}
		

		public static string StaticStringGuy()
		{
			return "static string guy";
		}
		
	}


	/////////////////////////////////////////////////////////////////////////////

	class StringTest {

		public string StringField		= "string field";

		
		DelegateContainer dc = new DelegateContainer();

		public Func<string> InstanceStringGuyDelegate = null;
		
		public Func<string> StaticStringGuyDelegate = DelegateContainer.StaticStringGuy;

		public int IntValue { get { return 5; } }

		
		/////////////////////////////////////////////////////////////////////////////
		
		public Func<string> DelegateProp
		{
			get {
				return InstanceStringGuyDelegate;
			}
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public string StringProperty
		{
			get {
				return "string property";
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public char this [ int index ]
		{
			get {
				return typeof(StringTest).Name[ index ];
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string this [ int start, int length ]
		{
			get {
				string str = typeof(StringTest).Name;

				if( start < 0 || start >= str.Length ) {
					start = 0;
				}

				if( length > str.Length - start ) {
					length = str.Length - start;
				}
				return str.Substring( start, length );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string StringMethod()
		{
			return "string method result";
		}


		/////////////////////////////////////////////////////////////////////////////

		public string StringMethod( string str )
		{
			return string.Format( "string method with 1 argument: \"{0}\"", str );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string StringMethod( string str, string str2 )
		{
			return string.Format( "string method with 2 arguments: \"{0}\", \"{1}\"", str, str2 );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string StringMethodOptionalArgs( params string [] args )
		{
			if( 0 == args.Length ) {
				return "StringMethodOptionalArgs was passed no arguments";
			}
			else {
				StringBuilder sb = new StringBuilder( "StringMethodOptionalArgs was called with the following args: ");
				for( int i = 0; i < args.Length; i++ ) {
					string arg = args[ i ];

					if( i > 0 ) {
						sb.Append( ", " );
					}

					sb.Append( arg );
				}
				return sb.ToString();
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public Func<bool> BoolFunc()
		//{
		//	return () => true;
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public Func<Func<bool>> DoubleUp()
		//{
		//	return BoolFunc;
		//}
		//
		//
		///////////////////////////////////////////////////////////////////////////////
		//
		//public void FuncTest()
		//{
		//	DoubleUp()();
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public StringTest()
		{
			InstanceStringGuyDelegate = dc.InstanceStringGuy;
		}


	}


	/////////////////////////////////////////////////////////////////////////////

	public class ObjectTest {

		public Func<object, object> QuoteArg;


		/////////////////////////////////////////////////////////////////////////////

		public object QuoteString( object obj )
		{
			return string.Format( "\"{0}\"", obj.ToString() );
		}


		/////////////////////////////////////////////////////////////////////////////

		public ObjectTest()
		{
			QuoteArg = QuoteString;
		}
	}
		




	/////////////////////////////////////////////////////////////////////////////

	partial class DefaultMacroProcessor : IMacroProcessor {


		/////////////////////////////////////////////////////////////////////////////

		public void AddTestObjects()
		{




			// ******
			IMacroHandler objHandler = registeredObjectMacroHandler;

		
			AddMacro( Macro.NewObjectMacro( this, "stringTest", objHandler, new StringTest()) );
		
			AddMacro( Macro.NewObjectMacro( this, "objTest", objHandler, new ObjectTest()) );


			//AddMacro( Macro.NewObjectMacro( this, "string", objHandler, "I am a string") );
			//AddMacro( Macro.NewObjectMacro( this, "dateTime", objHandler, new DateTime()) );
		}


	}

}
