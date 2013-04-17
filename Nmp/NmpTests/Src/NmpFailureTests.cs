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
using System.IO;
using System.Linq;
using System.Text;

using Xunit;

using NmpBase;
using Nmp;


#pragma warning disable 169

namespace NmpTests {


	/////////////////////////////////////////////////////////////////////////////

	public class NmpFailureTests1 {


		///////////////////////////////////////////////////////////////////////////////
		//
		//public void SetFixture( NmpInstance mp )
		//{
		//	this.nmpInstance = mp;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////////
		//// 1
		//
		//[Fact]
		//public void EmptyString()
		//{
		//	Assert.Equal( nmpInstance.InterpretString(string.Empty), string.Empty );
		//}
		//

		///////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void FileTest_Runaway()
		{
			//
			// debugger gets REALY slow if it's invoked after this fails
			//
			if( ! Debugger.IsAttached ) {
				Assert.Throws<RunawayMacroException> (
					delegate {
						Tuple<string, string> result = TestHelpers.RunMacro( "Runaway.nmp" );
						Assert.Equal(result.Item1, result.Item2 );
					}
				);
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void FileTest_NewObject()
		{
			// ******
			//
			// NewObject.nmp creates and throws an NmpBase.ExitException(), note that
			// we have to dig into the final exception to find it because it's actually
			// thrown inside of reflection invoke code
			//
			// #newobject
			// #newstatic
			//
			try {
				Tuple<string, string> result = TestHelpers.RunMacro( "NewObject.nmp" );
			}
			catch ( Exception ex ) {
				
				//
				// note: this is no longer required because we have to explicity
				// capture this exception in our .net method invoking code to allow
				// this test to "succeed" - in that code we cleanly rethrow the
				// ExitException by itself
				//

				//Exception root = ex;
				//while( null != ex.InnerException ) {
				//	ex = ex.InnerException;
				//}
				
				Assert.Equal( typeof(NmpBase.ExitException), ex.GetType() );
			}
		}


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
		
		public NmpFailureTests1()
		{
			// ******
			//Trace.Listeners.Add(new DefaultTraceListener());

		}


	}


}


