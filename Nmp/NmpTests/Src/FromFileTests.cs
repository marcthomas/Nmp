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

	public class FromFileTests {

		///////////////////////////////////////////////////////////////////////////////
		// 5

		[Fact]
		public void FileTest_Example()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Example.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 6

		[Fact]
		public void FileTest_Defmacro()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Defmacro.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 7

		[Fact]
		public void FileTest_Pushdef()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Pushdef.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 8

		[Fact]
		public void FileTest_If()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "If.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 9

		[Fact]
		public void FileTest_Tb()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Tb.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 10

		[Fact]
		public void FileTest_Defarray()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Defarray.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 11

		[Fact]
		public void FileTest_Deflist()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Deflist.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 12

		[Fact]
		public void FileTest_Foreach()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Foreach.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 13

		[Fact]
		public void FileTest_Forloop()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Forloop.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 14

		[Fact]
		public void FileTest_Expands()
		{
			Tuple<string, string> result = TestHelpers.RunMacro( "Expands.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 15

		[Fact]
		public void FileTest_IfElseAndOthers()
		{
			///Trace.WriteLine( "informational" );
			Tuple<string, string> result = TestHelpers.RunMacro( "IfElseAndOthers.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 16

		//
		// can't use this test as it depends upon the directory structure of the Nmp
		// solution being the same between initializing the tests reuslts and running
		// the test - it is common that they could be different - different machines, 
		// users or anything else
		//

		//[Fact]
		//public void FileTest_Directory()
		//{
		//	//
		//	// #directory
		//	// #defpath
		//	// #currentfile
		//	// #parentfile
		//	//
		//	Tuple<string, string> result = TestHelpers.RunMacro( "Directory.nmp" );
		//	Assert.Equal( result.Item1, result.Item2 );

		//	//Equal( result.Item1, result.Item2 );
		//}



		///////////////////////////////////////////////////////////////////////////////
		// 17

		[Fact]
		public void FileTest_SimpleRegex()
		{
			//
			// #addrecognizerregex( name, regexStr [, macroToCall])
			// #setregexrecognizer( true/on | false/off )
			//
			Tuple<string, string> result = TestHelpers.RunMacro( "Simple Regex.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		///////////////////////////////////////////////////////////////////////////////
		// 18

		[Fact]
		public void FileTest_DateTime()
		{
			//
			// #newDateTime
			// #dateTime				** not tested because will always fail
			// #dateTimeUtc			** not tested because will always fail
			//
			Tuple<string, string> result = TestHelpers.RunMacro( "DateTime.nmp" );
			Assert.Equal( result.Item1, result.Item2 );
		}


		/////////////////////////////////////////////////////////////////////////////////
		//// 19
		//
		//[Fact]
		//public void FileTest_CppMode()
		//{
		//	//
		//	// #cpp()
		//	//
		//	Tuple<string, string> result = TestHelpers.RunMacro( "Cpp.nmp" );
		//	Equal( result.Item1, result.Item2 );
		//}
		//

		///////////////////////////////////////////////////////////////////////////////

		public void Equal( string lhs, string rhs )
		{
			Assert.Equal( lhs, rhs );
		}




/*


#newlist
#newarray
#eval

#startEncodeHtml
#endEncodeHtml


#String
#Path
#DateTime
#Directory
#File

stringTest


#getmacros
#trace
#nofile

#setoutputextension

#loadmacros
#include
#readfile
#cleardivert
#pushdivert
#popdivert
#divert
#undivert
#fetchdivert
#includedivert
#savedivert
#dumpdivert
#forloop
#foreach
#forEachTest
#define
#push
#popdef
#dumpdef
#echo
#undef

#exit( value )




Current Object macros:

#DateTime
#Directory
#File
#Path
#String
stringTest


Current Builtin macros:

#&
#addRecognizerRegex
#clearDivert
#cpp
#currentFile
#dateTime
#dateTimeUtc
#define
#defpath
#directory
#divert
#dumpdef
#dumpDivert
#echo
#endEncodeHtml
#eval
#exit
#fetchDivert
#foreach
#forEachTest
#forloop
#getMacroNames
#getMacros
#getType
#ifDefined
#ifelse
#ifElse
#ifEmpty
#ifNotDefined
#ifNotEmpty
#include
#includeDivert
#isDefined
#isEmpty
#isEqual
#isFalse
#isNotDefined
#isNotEmpty
#isNotEqual
#isTrue
#loadMacros
#newArray
#newDateTime
#newList
#newObject
#newObjectMacro
#newStatic
#newStaticMacro
#nofile
#not
#parentFile
#popdef
#popdivert
#popDivert
#push
#pushdivert
#pushDivert
#readFile
#saveDivert
#setOutputExtension
#setRegexRecognizer
#startEncodeHtml
#trace
#typeof
#undef
#undivert
defarray
define
deflist
defmacro
foreach
forloop
if
include
pushdef
tb
undef
*/


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
		
		public FromFileTests()
		{
			// ******
			//Trace.Listeners.Add(new DefaultTraceListener());

		}


	}


}


