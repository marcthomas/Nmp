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

using NmpBase;
using Nmp;

using ReflectionHelpers;
using NmpExpressions;



#pragma warning disable 169

namespace ObjectTests {

#if thingsToDo

	implement static class for static tests

	test: methods, properties, primitives

	with classes and structs


#endif


	/////////////////////////////////////////////////////////////////////////////

	//[Serializable]
	public class AuxType<TKey, TValue> {
		TKey key;
		TValue value;
		public TKey Key { get { return key; } }
		public TValue Value { get { return value; } }
		public AuxType( TKey a, TValue b ) { key = a; value = b; }
	}

	public struct StructTest {

		public int Int32 { get { return 42; } }

	}


	/////////////////////////////////////////////////////////////////////////////

	public class ObjectTestClass {

		// ******
		public AuxType<string, object> AuxType
		{
			get { return new AuxType<string, object>( "auxtype key", "auxtype value" ); }
		}


		/////////////////////////////////////////////////////////////////////////////

		public KeyValuePair<string, object> KVP
		{
			get { return new KeyValuePair<string, object>( "key value pair key", "key value pair value" ); }
		}


		/////////////////////////////////////////////////////////////////////////////

		public StructTest AStruct 
		{
			get { return new StructTest { }; }
		}


		/////////////////////////////////////////////////////////////////////////////

		public string AString
		{
			get
			{
				return "string";
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public int Int32
		{
			get { return 42; }
		}


		/////////////////////////////////////////////////////////////////////////////

		public string MethodEchosArg( string value )
		{
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public double MethodEchosArg( double value )
		{
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string StaticMethodEchosArg( string value )
		{
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static double StaticMethodEchosArg( double value )
		{
			return value;
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	public static class TestClassExtensions {

		public static string ClassFullName( this ObjectTestClass tc )
		{
			return tc.GetType().FullName;
		}

		//public static double MethodEchosArg( this ObjectTestClass tc, double arg )
		//{
		//	return tc.MethodEchosArg( arg );
		//}

	}



}




