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
using System.Reflection;
using System.Text;

using Xunit;
using NmpTests;

using NmpBase;
using Nmp;

using ReflectionHelpers;
using NmpExpressions;



#pragma warning disable 169

namespace ObjectTests {


	// #object.newStaticMacro( `#testStatic', `SSTestClass', `AssociatedClass1', `AssociatedClass2' ... )


	/////////////////////////////////////////////////////////////////////////////

	public class MacroObjectTests : IUseFixture<Evaluator<NmpHostHelper>> {

		NmpEvaluator nmp;
		ObjectTestClass testClass;

		/////////////////////////////////////////////////////////////////////////////

		public void SetFixture( Evaluator<NmpHostHelper> data )
		{
			// ******
			nmp = data;
			testClass = new ObjectTestClass { };

			// ******
			var script = @"
				#object.newObjectMacro( `#testInstance', `ObjectTestClass')
				#object.newStaticMacro( `#testStatic', `ObjectTestClass')

				#object.addExtensionClasses( `TestClassExtensions', `ObjectTestClass' )

				#.define( `macro', `world')
			";
			nmp.Evaluate( nmp.GetStringContext( script ), true );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void MacroObjectTests_TestNmpExtensionMethod()
		{
			var result = nmp.Evaluate( nmp.GetStringContext( "#testInstance.ClassFullName()" ), false );
			Assert.Equal( typeof( ObjectTestClass ).FullName, result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void MacroObjectTests_InstancePropertyGetInt32()
		{
			// ******
			//
			// verify instance object can access property of it's class
			//
			var result = nmp.Evaluate( nmp.GetStringContext( "#testInstance.Int32" ), false );
			Assert.Equal( "42", result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void MacroObjectTests_InstancePropertyGetAString()
		{
			// ******
			//
			// verify instance object can access property of it's class
			//
			var result = nmp.Evaluate( nmp.GetStringContext( "#testInstance.AString" ), false );
			Assert.Equal( "string", result );
		}



		/////////////////////////////////////////////////////////////////////////////
#if somethingelse

		[Fact]
		public void MacroObjectTests_StaticGetStatic()
		{
			// ******
			//
			// verify instance object can access static members of it's class
			//
			using( var mp = GetTestClass() ) {
				var result = mp.Evaluate( mp.GetStringContext( "#testStatic.PublicStaticString()" ), false );
				Assert.Equal( "pss", result );
			}
		}

		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void MacroObjectTests_InstanceGetStatic()
		{
			// ******
			//
			// verify instance object can access an instance members of it's class
			//
			using( var mp = GetTestClass() ) {
				var result = mp.Evaluate( mp.GetStringContext( "#testInstance.PublicStaticString()" ), false );
				Assert.Equal( "pss", result );
			}
		}
#endif


		/////////////////////////////////////////////////////////////////////////////

		//public static string DisasmAuxType( AuxType<string, object> aux )
		//{
		//	return aux.Key;
		//}

		//public static string DisasmKVP( object kvp )	//KeyValuePair<string, object> kvp )
		//{
		//	return ((KeyValuePair<string, object>) kvp).Key;
		//}


		/////////////////////////////////////////////////////////////////////////////

		public MacroObjectTests()
		{
		}



	}


}


