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

	public class NmpTests {	//: IUseFixture<NmpInstance> {

	//	NmpInstance nmpInstance;



//		/////////////////////////////////////////////////////////////////////////////
//
//		public void SetFixture( NmpInstance mp )
//		{
//			this.nmpInstance = mp;
//		}


		///////////////////////////////////////////////////////////////////////////////
		// 1

		[Fact]
		public void EmptyString()
		{
			Assert.Equal( TestHelpers.InterpretString(string.Empty), string.Empty );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 2

		[Fact]
		public void NonMacroString()
		{
			string str = "Hello World!";
			Assert.Equal( TestHelpers.InterpretString(str), str );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 3

		[Fact]
		public void SimpleMacro()
		{
			// ******
			string macroName = "test";
			string macroText = "one two three";

			//
			// text is interpreted by Nmp
			//
			string text = macroName;

			// ******
			using ( var mp = TestHelpers.NewNmpEvaluator() ) {
				mp.AddTextMacro( macroName, macroText, null );
				Assert.Equal( mp.Evaluate(mp.GetStringContext(text), true), macroText );
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		// 4

		[Fact]
		public void SimpleInvokeMacro()
		{
			// ******
			string macroName = "test";
			string macroText = "one two three";

			//
			// macro is invoked by name
			//

			// ******
			using ( var mp = TestHelpers.NewNmpEvaluator() ) {
				mp.AddTextMacro( macroName, macroText, null );
				Assert.Equal( mp.InvokeMacro(macroName, null, true), macroText );
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		// 5

		//[Fact]
		//public void SimpleInvokeMacro()
		//{
		//	// ******
		//	string macroName = "test";
		//	string macroText = "one two three";

		//	//
		//	// macro is invoked by name
		//	//

		//	// ******
		//	using( var mp = TestHelpers.NewNmpEvaluator() ) {
		//		mp.AddTextMacro( macroName, macroText, null );
		//		Assert.Equal( mp.InvokeMacro( macroName, null, true ), macroText );
		//	}
		//}





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
		
		public NmpTests()
		{
		}


	}


}


