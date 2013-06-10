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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Text;


//#pragma warning disable 414


using NmpBase;
using Nmp;



namespace ReflectionHelpers {
	//
	// caching, delegates
	//
	// http://www.bing.com/search?q=.net+dynamically+create+delegate&qs=n&form=QBLH&pq=.net+dynamically+create+delegate&sc=1-32&sp=-1&sk=

	//
	// lambda expressions
	//
	// http://blogs.msdn.com/b/csharpfaq/archive/2009/09/14/generating-dynamic-methods-with-expression-trees-in-visual-studio-2010.aspx

	public delegate object InvokeHandler( object target, object [] paramters );

	/////////////////////////////////////////////////////////////////////////////

	static class InvokerGenerator {

		/////////////////////////////////////////////////////////////////////////////

		static void EmitCastToReference( ILGenerator il, System.Type type )
		{
			if( type.IsValueType ) {
				il.Emit( OpCodes.Unbox_Any, type );
			}
			else {
				il.Emit( OpCodes.Castclass, type );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		static void EmitBoxIfNeeded( ILGenerator il, System.Type type )
		{
			if( type.IsValueType ) {
				il.Emit( OpCodes.Box, type );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		static void EmitFastInt( ILGenerator il, int value )
		{
			switch( value ) {
				case -1:
					il.Emit( OpCodes.Ldc_I4_M1 );
					return;
				case 0:
					il.Emit( OpCodes.Ldc_I4_0 );
					return;
				case 1:
					il.Emit( OpCodes.Ldc_I4_1 );
					return;
				case 2:
					il.Emit( OpCodes.Ldc_I4_2 );
					return;
				case 3:
					il.Emit( OpCodes.Ldc_I4_3 );
					return;
				case 4:
					il.Emit( OpCodes.Ldc_I4_4 );
					return;
				case 5:
					il.Emit( OpCodes.Ldc_I4_5 );
					return;
				case 6:
					il.Emit( OpCodes.Ldc_I4_6 );
					return;
				case 7:
					il.Emit( OpCodes.Ldc_I4_7 );
					return;
				case 8:
					il.Emit( OpCodes.Ldc_I4_8 );
					return;
			}

			if( value > -129 && value < 128 ) {
				il.Emit( OpCodes.Ldc_I4_S, (SByte) value );
			}
			else {
				il.Emit( OpCodes.Ldc_I4, value );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static InvokeHandler Generate( MethodInfo methodInfo, object instanceObject )
		{
			// ******
			bool isStruct = false;
			Type instanceType = null;
			if( null != instanceObject ) {
				instanceType = instanceObject.GetType();
				isStruct = instanceType.IsValueType && !instanceType.IsEnum && !instanceType.IsPrimitive;
			}

			// ******
			DynamicMethod dynamicMethod = new DynamicMethod(
					string.Empty,																					// name of dynamic method
					typeof( object ),																			// return type
					new Type [] { typeof( object ), typeof( object [] ) },// parameter types to dynamic method
					methodInfo.DeclaringType.Module												// module to associate dynamic method with
				);

			ILGenerator il = dynamicMethod.GetILGenerator();

			// ******
			ParameterInfo[] ps = methodInfo.GetParameters();

			// ******
			Type[] paramTypes = new Type [ ps.Length ];
			for( int i = 0; i < paramTypes.Length; i++ ) {
				if( ps [ i ].ParameterType.IsByRef )
					paramTypes [ i ] = ps [ i ].ParameterType.GetElementType();
				else
					paramTypes [ i ] = ps [ i ].ParameterType;
			}

			// ******
			LocalBuilder[] locals = new LocalBuilder [ paramTypes.Length ];
			for( int i = 0; i < paramTypes.Length; i++ ) {
				locals [ i ] = il.DeclareLocal( paramTypes [ i ], true );
			}

			// ******
			for( int i = 0; i < paramTypes.Length; i++ ) {
				il.Emit( OpCodes.Ldarg_1 );
				EmitFastInt( il, i );
				il.Emit( OpCodes.Ldelem_Ref );
				EmitCastToReference( il, paramTypes [ i ] );
				il.Emit( OpCodes.Stloc, locals [ i ] );
			}

			// ******
			if( !methodInfo.IsStatic ) {
				il.Emit( OpCodes.Ldarg_0 );
				if( isStruct ) {
					il.Emit( OpCodes.Unbox, instanceType );
				}
			}

			// ******
			for( int i = 0; i < paramTypes.Length; i++ ) {
				if( ps [ i ].ParameterType.IsByRef )
					il.Emit( OpCodes.Ldloca_S, locals [ i ] );
				else
					il.Emit( OpCodes.Ldloc, locals [ i ] );
			}

			// ******
			if( methodInfo.IsStatic || isStruct )
				il.EmitCall( OpCodes.Call, methodInfo, null );
			else {
				il.EmitCall( OpCodes.Callvirt, methodInfo, null );
			}
			
			// ******
			if( methodInfo.ReturnType == typeof( void ) )
				il.Emit( OpCodes.Ldnull );
			else
				EmitBoxIfNeeded( il, methodInfo.ReturnType );

			// ******
			for( int i = 0; i < paramTypes.Length; i++ ) {
				if( ps [ i ].ParameterType.IsByRef ) {
					il.Emit( OpCodes.Ldarg_1 );
					EmitFastInt( il, i );
					il.Emit( OpCodes.Ldloc, locals [ i ] );
					if( locals [ i ].LocalType.IsValueType )
						il.Emit( OpCodes.Box, locals [ i ].LocalType );
					il.Emit( OpCodes.Stelem_Ref );
				}
			}

			// ******
			il.Emit( OpCodes.Ret );

			if( dynamicMethod.ContainsGenericParameters ) {
				Debugger.Break();
			}

			// ******
			InvokeHandler invoker = (InvokeHandler) dynamicMethod.CreateDelegate( typeof( InvokeHandler ) );
			return invoker;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static InvokeHandler __Generate( MethodInfo methodInfo, Type [] paramTypes )
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();
			//Type[] paramTypes = new Type[parameters.Length];

			// ******
			//
			// Generate the IL for the method
			//
			DynamicMethod dynamicMethod = new DynamicMethod( string.Empty, typeof( object ), new Type [] { typeof( object ), typeof( object [] ) }, methodInfo.DeclaringType.Module );
			ILGenerator il = dynamicMethod.GetILGenerator();

			// ******
			//
			// generates a local variable for each parameter
			//
			LocalBuilder[] locals = new LocalBuilder [ paramTypes.Length ];
			for( int i = 0; i < paramTypes.Length; i++ ) {
				locals [ i ] = il.DeclareLocal( paramTypes [ i ], true );
			}

			// ******
			//
			// creates code to copy the parameters to the local variables
			//
			for( int i = 0; i < paramTypes.Length; i++ ) {
				il.Emit( OpCodes.Ldarg_1 );
				EmitFastInt( il, i );
				il.Emit( OpCodes.Ldelem_Ref );
				EmitCastToReference( il, paramTypes [ i ] );
				il.Emit( OpCodes.Stloc, locals [ i ] );
			}

			// ******
			if( !methodInfo.IsStatic ) {
				//
				// loads the object into the stack
				//
				il.Emit( OpCodes.Ldarg_0 );
			}

			// ******
			//
			// loads the parameters copied to the local variables into the stack
			//
			for( int i = 0; i < paramTypes.Length; i++ ) {
				if( parameters [ i ].ParameterType.IsByRef )
					il.Emit( OpCodes.Ldloca_S, locals [ i ] );
				else
					il.Emit( OpCodes.Ldloc, locals [ i ] );
			}

			// ******
			//
			// calls the method
			//
			if( !methodInfo.IsStatic ) {
				il.EmitCall( OpCodes.Callvirt, methodInfo, null );
			}
			else {
				il.EmitCall( OpCodes.Call, methodInfo, null );
			}

			// ******
			//
			// creates code for handling the return value
			//
			if( methodInfo.ReturnType == typeof( void ) ) {
				il.Emit( OpCodes.Ldnull );
			}
			else {
				EmitBoxIfNeeded( il, methodInfo.ReturnType );
			}

			// ******
			//
			// iterates through the parameters updating the parameters passed by ref
			//
			for( int i = 0; i < paramTypes.Length; i++ ) {
				if( parameters [ i ].ParameterType.IsByRef ) {
					il.Emit( OpCodes.Ldarg_1 );
					EmitFastInt( il, i );
					il.Emit( OpCodes.Ldloc, locals [ i ] );
					if( locals [ i ].LocalType.IsValueType )
						il.Emit( OpCodes.Box, locals [ i ].LocalType );
					il.Emit( OpCodes.Stelem_Ref );
				}
			}

			// ******
			//
			// returns the value to the caller
			//
			il.Emit( OpCodes.Ret );

			// ******
			//
			// converts the DynamicMethod to a FastInvokeHandler delegate to call to the method
			//
			InvokeHandler invoker = (InvokeHandler) dynamicMethod.CreateDelegate( typeof( InvokeHandler ) );
			return invoker;
		}

	}

}

