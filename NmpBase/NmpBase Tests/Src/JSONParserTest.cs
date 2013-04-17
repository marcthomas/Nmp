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
using System.Diagnostics;
using System.Linq;
using System.Text;

//using NmpBase;

using NmpBase;

using Xunit;


namespace NmpBase_Tests {


	/////////////////////////////////////////////////////////////////////////////

	public class JSONTarget : JSONItemHandler {

		///////////////////////////////////////////////////////////////////////////

		public void Error( string msgFmt, params object [] args )
		{
			Trace.Write( "  error: " );
			try {
				Trace.WriteLine( string.Format( msgFmt, args ) );
			}
			catch ( Exception ex ) {
				Trace.Write( ex.Message );
				throw ex;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public void Warning( string msgFmt, params object [] args )
		{
			Trace.Write( "  warning: " );
			try {
				Trace.WriteLine( string.Format( msgFmt, args ) );
			}
			catch( Exception ex ) {
				Trace.Write( ex.Message );
				throw ex;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public void EnterObject( JSONParser jp  )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		public void ExitObject( JSONParser jp  )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		public void EnterArray( JSONParser jp  )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		public void ExitArray( JSONParser jp  )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		public void Identifier( JSONParser jp, string identifier )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		public void Value( JSONParser jp, object value )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		public bool Test( string jsonData )
		{
			var jp = new JSONParser( this, jsonData );
			try {
				return jp.Parse();
			}
			catch {
				return false;
			}
		}
	}




	/////////////////////////////////////////////////////////////////////////////

	public class JSONParserTest : IUseFixture<JSONTarget> {

		JSONTarget parser;


		/////////////////////////////////////////////////////////////////////////////

		public void SetFixture( JSONTarget target )
		{
			this.parser = target;
		}

/*
[
"some text",
"@1163531522089@",
"@-1163531522089@",
"\/Date(1198908717056)\/",
01,
-1e5,
0e5,
1,
1.2,
.3,
1e3,
1.2e-12,
30e6,
-1,
-1.2,
-.3,
-1e20,
-1.2e-12,
]
*/

		///////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TestParseInts()
		{
			// ******
			Trace.WriteLine( "TestParseInts() should succeed");

			// ******
			string array = "[ 1, 0, 14, 10000 ]";
			Assert.True( parser.Test(array) );
		}



		///////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TestParseFloats()
		{
			// ******
			Console.WriteLine( "TestParseFloats() should succeed");

			// ******
			string array = "[ 1.0, 0.123, -1.4, 42.0]";
			Assert.True( parser.Test(array) );
		}



		///////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TestParseScientific()
		{
			// ******
			Trace.WriteLine( "TestParseScientific() should succeed");

			// ******
			string array = "[ 0e8, 0.123e2, .002E16, 2e6 ]";
			Assert.True( parser.Test(array) );
		}


		///////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TestParseNumber()
		{
			// ******
			Trace.WriteLine( "TestParseNumber() should fail");

			// ******
			string array = "[ 0e8, 0.123e2, :.002E16, 2e6 ]";
			Assert.False( parser.Test(array) );
		}


//		///////////////////////////////////////////////////////////////////////////////
//
//		[Fact]
//		public void TestParseObject()
//		{
//			// ******
//			Trace.WriteLine( "TestParseObject() should succeed" );
//
//			// ******
//			string objs = "object1 : \"string\", object2 : 21.3-e4, object3 : [ 1, 2, 3 ]";
//			Assert.True( parser.Test(objs) );
//		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//[Fact]
		//public void MyTest()
		//{
		//	Assert.Equal(4, 2 + 2);
		//}
		//

		///////////////////////////////////////////////////////////////////////////////
		//
		//[Fact]
		//public void BadMath()
		//{
		//	Assert.Equal(5, 2 + 2);
		//}
		//

		///////////////////////////////////////////////////////////////////////////////
		//
		//[Fact]
		//public void BadMethod()
		//{
		//	double result = DivideNumbers(5, 0);
		//	Assert.Equal(double.PositiveInfinity, result);
		//}
		//

		///////////////////////////////////////////////////////////////////////////////
		//
		//public int DivideNumbers(int theTop, int theBottom)
		//{
		//	return theTop / theBottom;
		//}
		//

		///////////////////////////////////////////////////////////////////////////////
		//
		//[Fact]
		//public void DivideByZeroThrowsException()
		//{
		//	Assert.Throws<System.DivideByZeroException>(
		//		delegate
		//		{
		//				DivideNumbers(5, 0);
		//		});
		//}
		//

		///////////////////////////////////////////////////////////////////////////////
		//
		//[Fact(Skip="Can't figure out where this is going wrong...")]
		//public void SkipTest()
		//{
		//	Assert.Equal(5, 2 + 2);
		//}
		//


		/////////////////////////////////////////////////////////////////////////////
		
		public JSONParserTest()
		{
			Trace.Listeners.Add(new DefaultTraceListener());
		}


	}


}


