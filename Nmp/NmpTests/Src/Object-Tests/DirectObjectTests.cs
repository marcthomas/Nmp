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


	/////////////////////////////////////////////////////////////////////////////

	public class DirectObjectTests : IUseFixture<ObjectTestClass> {

		ObjectTestClass testClass;
		Type testClassType;
		MethodCache methodCache;
		ExtensionTypeDictionary methodExtensions;

		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_TestInt32()
		{
			//
			// tests getting int property on the test class
			//
			var result = GetProperty( testClass, "Int32" );
			result = GetProperty( testClass, "Int32" );
			Assert.Equal( 42, result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_TestAStructInt32()
		{
			//
			// tests getting property from struct on the test class; exercises code
			// generation for calling methods on structs which require a different
			// set of operations - unpacking boxed object
			//
			var outerProp = GetProperty( testClass, "AStruct" );
			var result = GetProperty( outerProp, "Int32" );
			Assert.Equal( 42, result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_TestKVPKey()
		{
			//
			// tests getting property from a generic struct on the test class; exercises code
			// generation for calling methods on structs which require a different
			// set of operations - unpacking boxed object
			//
			var outerProp = GetProperty( testClass, "KVP" );
			var result = GetProperty( outerProp, "Key" );
			Assert.Equal( "key value pair key", result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_InstanceMethodEchosArg()
		{
			// ******
			const string ConstStr = "argument";
			var strResult = CallInstanceMethod( testClass, "MethodEchosArg", ConstStr );
			Assert.Equal( ConstStr, strResult );

			// ******
			const double ConstDouble = 3.145;
			var dblResult = CallInstanceMethod( testClass, "MethodEchosArg", ConstDouble );
			Assert.Equal( ConstDouble, dblResult );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_StaticMethodEchosArg()
		{
			// ******
			const string ConstStr = "argument";
			var strResult = CallStaticMethod( testClassType, "StaticMethodEchosArg", ConstStr );
			Assert.Equal( ConstStr, strResult );

			// ******
			const double ConstDouble = 3.145;
			var dblResult = CallStaticMethod( testClassType, "StaticMethodEchosArg", ConstDouble );
			Assert.Equal( ConstDouble, dblResult );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_ExtensionMethodSelectType()
		{
			// ******
			const string ConstMethodName = "Modifiers";
			Type firstParamType;

			var ctors = testClassType.GetConstructors();
			var methodInfo = SelectExtensionMethod( ctors [ 0 ], ConstMethodName, out firstParamType );

			Assert.Equal( ConstMethodName, methodInfo.Name );
			Assert.Equal( typeof( ConstructorInfo ), firstParamType );

			// ******
			methodInfo = SelectExtensionMethod( ctors [ 0 ], ConstMethodName, out firstParamType, 42 );

			Assert.Equal( ConstMethodName, methodInfo.Name );
			Assert.Equal( typeof( MemberInfo ), firstParamType );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_ExtensionMethodSelectType2()
		{
			// ******
			const string ConstMethodName = "Modifiers";
			Type firstParamType;

			var fieldInfo = testClassType.GetField( "intField" );
			var methodInfo = SelectExtensionMethod( fieldInfo, ConstMethodName, out firstParamType );

			Assert.Equal( ConstMethodName, methodInfo.Name );
			Assert.Equal( typeof( FieldInfo ), firstParamType );

			// ******
			methodInfo = SelectExtensionMethod( fieldInfo, ConstMethodName, out firstParamType, 42 );

			Assert.Equal( ConstMethodName, methodInfo.Name );
			Assert.Equal( typeof( MemberInfo ), firstParamType );

			var strResult = CallExtensionMethod( typeof(FieldInfo), "Modifiers", fieldInfo, 42 );
			Assert.Equal( "MemberInfo Modifiers value is 42", strResult );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_ExtensionMethodEchosArg()
		{
			// ******
			var newArgs = new object [] { testClass };
			var strResult = CallExtensionMethod( testClassType, "ClassFullName", newArgs );
			Assert.Equal( typeof( ObjectTestClass ).FullName, strResult );
		}


		/////////////////////////////////////////////////////////////////////////////
#if include

//		NmpEvaluator GetTestClass()
//		{
//			var script = @"
//#object.newStaticMacro( `#testStatic', `SSTestClass')
//#object.newObjectMacro( `#testInstance', `SSTestClass')

//#object.addExtensionClasses( `string', `SSTestClass' )

//#.define( `macro', `world')
//";
//			var mp = TestHelpers.NewNmpEvaluator();
//			mp.Evaluate( mp.GetStringContext( script ), true );
//			return mp;
//		}


		/////////////////////////////////////////////////////////////////////////////
		string testScript = @"
(#defmacro `_Test')
	
	(#defarray `array')
    ""debug"" : ""on""
	(#endarray)

	(#foreach `@array')
		.{$$value.Key}.
		
		
		(#if #is.Equal(#object.typeof(`@$$value.Value'),System.String) )
			.{ = $$value.Value }.
		(#else)
			(#foreach `@$$value.Value')
				.{.nl.  $$$value.Key = $$$value.Value}.
			(#endforeach)
		(#endif)
		
		.{.nl.}.
		.{.nl.}.
	(#endforeach)


(#endmacro)
";

		/////////////////////////////////////////////////////////////////////////////
		[Fact]
		public void DirectObjectTests_TestNmpExtensionMethod()
		{
			// ******
			//
			// verify instance object can access property of it's class
			//
			using( var mp = GetTestClass() ) {
				//
				// 
				//
				//var result = mp.Evaluate( mp.GetStringContext( "#testInstance.MethodName()" ), false );
				//Assert.Equal( "pps", result );

				//
				// another test but this time the extension class is not the same as the class
				// of the instance that is used to call the method
				//
				var result = mp.Evaluate( mp.GetStringContext( "macro.StringExtensionMethod()" ), false );
				Assert.Equal( "hello world", result );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_InstanceGetStringProperty()
		{
			// ******
			//
			// verify instance object can access property of it's class
			//
			using( var mp = GetTestClass() ) {
				var result = mp.Evaluate( mp.GetStringContext( "#testInstance.PublicPropertyString" ), false );
				Assert.Equal( "pps", result );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void DirectObjectTests_StaticGetStatic()
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
		public void DirectObjectTests_InstanceGetStatic()
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

		public void SetFixture( ObjectTestClass data )
		{
			testClass = data;
			testClassType = testClass.GetType();
			NewMethodCache();
			NewMethodExtensions();
		}


		/////////////////////////////////////////////////////////////////////////////

		void NewMethodCache()
		{
			methodCache = new MethodCache { };
		}


		/////////////////////////////////////////////////////////////////////////////

		void NewMethodExtensions()
		{
			methodExtensions = new ExtensionTypeDictionary { };
			methodExtensions.AddMethodExtensions(
				typeof( TestClassExtensions ),

				typeof( ObjectTestClass ),
				typeof( EventInfo ),
				typeof( FieldInfo ),
				typeof( MethodBase ),
				typeof( ConstructorInfo ),
				typeof( MemberInfo ),

				typeof( object )
			);
		}


		/////////////////////////////////////////////////////////////////////////////

		MethodInfo GetGetPropertyMethodInfo( Type type, string name )
		{
			var pi = type.GetProperty( name );
			return pi.GetGetMethod();
		}


		/////////////////////////////////////////////////////////////////////////////

		MethodInfo GetMethodMethodInfo( Type type, string name )
		{
			return type.GetMethod( name );
		}

		/////////////////////////////////////////////////////////////////////////////

		object GetProperty( object instance, string propertyName )
		{
			var instanceType = instance.GetType();
			var methodInfo = GetGetPropertyMethodInfo( instanceType, propertyName );
			var cacheItem = methodCache.GetHandler( instance, instanceType, propertyName, methodInfo, null );
			var result = cacheItem.Handler( instance, null );
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		object CallInstanceMethod( object instance, string methodName, params object [] args )
		{
			//
			// this is not the Nmp, the args MUST be correct
			//
			var instanceType = instance.GetType();
			var methodInfo = instanceType.GetMethod( methodName, Type.GetTypeArray( args ) );
			var cacheItem = methodCache.GetHandler( instance, instanceType, methodName, methodInfo, args );
			var result = cacheItem.Handler( instance, args );
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		object CallStaticMethod( Type type, string methodName, params object [] args )
		{
			//
			// this is not the Nmp, the args MUST be correct
			//
			//var methodInfo = type.GetMethod( methodName, BindingFlags.Public | BindingFlags.Static, null, Type.GetTypeArray( args ), null );
			var methodInfo = type.GetMethod( methodName, Type.GetTypeArray( args ) );
			var cacheItem = methodCache.GetHandler( null, type, methodName, methodInfo, args );
			var result = cacheItem.Handler( null, args );
			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		object CallExtensionMethod( Type type, string methodName, params object [] args )
		{
			//
			// this is not the Nmp, the args MUST be correct
			//
			Tuple<MethodBase, object [], List<MethodBase>> findResult = methodExtensions.FindExtensionMethod( type, methodName, args );	//, ArgumentMatcher.MatchArgs2 );
			var cacheItem = methodCache.GetHandler( null, testClassType, methodName, findResult.Item1 as MethodInfo, args );
			return cacheItem.Handler( null, findResult.Item2 );
		}


		/////////////////////////////////////////////////////////////////////////////

		Type GetFirstParameterType( MethodBase mi )
		{
			if( null == mi ) {
				throw new ArgumentNullException( "mi" );
			}
			var parameters = mi.GetParameters();
			return 0 == parameters.Length ? null : parameters [ 0 ].ParameterType;
		}


		/////////////////////////////////////////////////////////////////////////////

		MethodBase SelectExtensionMethod( object theObject, string methodName, out Type firstParamType, params object [] args )
		{
			var objType = theObject.GetType();
			var newArgs = Arguments.PrependArgs( args, theObject );
			Tuple<MethodBase, object [], List<MethodBase>> findResult = methodExtensions.FindExtensionMethod( objType, methodName, newArgs );
			firstParamType = GetFirstParameterType( findResult.Item1 );
			return findResult.Item1;
		}


		/////////////////////////////////////////////////////////////////////////////

		public DirectObjectTests()
		{
		}



	}


}


