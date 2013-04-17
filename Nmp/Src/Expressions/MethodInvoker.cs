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
using System.Reflection;
using System.Text;

using NmpBase;
using Nmp;


namespace Nmp.Expressions {


	/////////////////////////////////////////////////////////////////////////////

	class MethodInvoker : Invoker {

		static NullResult nullResult = new NullResult();

		ObjectInfo info;


		/////////////////////////////////////////////////////////////////////////////
		
		protected static object [] FindMethod( ObjectInfo objInfo, object [] argsIn, out MethodBase miOut )
		{
			// ******
			IList<MethodBase> methods = objInfo.MembersAs<MethodBase>();

			// ******
			MethodBase mi;
			object [] args = MatchArgs( methods, argsIn, out mi );
			if( null != args ) {
				miOut = mi;
				return args;
			}

			//
			// never returns
			//
			ThreadContext.MacroError( "unable to locate an implementation of \"{0}\" whose parameters match (or can be converted from) \"{1}\"\nTried matching:\n{2}", objInfo.MemberName, Arguments.ObjectsTypeNames(argsIn), Arguments.GetMethodSignatures(methods) );

			// ******
			miOut = null;
			return null;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( ObjectInfo objInfo, object [] argsIn )
		{
			// ******
			MethodBase mi;
			object [] args = FindMethod( objInfo, argsIn, out mi );

			// ******
			if( objInfo.IsStatic  && ! mi.IsStatic ) {
				ThreadContext.MacroError( "attempt to call non static method \"{0}\" using a static class reference to \"{1}\"", objInfo.MemberName, objInfo.ObjectType.FullName );
			}

			// ******
			//
			// 'obj' can be null if we're calling a static method
			//
			object obj = objInfo.Object;
			object result = null;

			try {
				result = mi.Invoke( obj, args );
			}
			catch ( ExitException ex ) {
				throw ex;
			}
			catch ( Exception ex ) {

				Exception exCheck = ex;
				while( null != exCheck ) {
					if( exCheck is ExitException ) {
						//
						// this is a test from NmpTests: NmpObject.nmp
						//
						// by capturing and rethrowing it here we:
						//
						//	1. don't call MacroError() so the test will succeed
						//	2. it cleans up capturing the error in the test
						//
						// this code was introducted once we wrapped a try/catch around invoking
						// methods - which, for some unexplicable reason, we hadn't been doing
						//
						// note: ExitException would never be thrown by .NET code unless we did
						// it (as we do) with a Lambda expression
						//
						throw exCheck;
					}
					exCheck = exCheck.InnerException;
				}

				//
				// never returns
				//
				//ThreadContext.MacroError( ExceptionHelpers.RecursiveMessage( ex, "error invoking method \"{0}\"", objInfo.MemberName ) );

				var errorAndMethod = ExceptionHelpers.RecursiveMessage( ex, "error invoking method \"{0}\"", objInfo.MemberName );
				
				var sb = new StringBuilder();
				sb.AppendFormat( "Arguments to method: {0}\n", objInfo.MemberName );
				foreach( var arg in argsIn ) {
					sb.AppendLine( null != arg ? arg.ToString() : "null" );
				}

				ThreadContext.MacroError( "{0}\n{1}", errorAndMethod, sb.ToString() );

				return null;
			}

			if( typeof(void) == (mi as MethodInfo).ReturnType ) {
				result = nullResult;
			}

			return result;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( info, argsIn );
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodInvoker( ObjectInfo objInfo )
		{
			info = objInfo;
		}


		/////////////////////////////////////////////////////////////////////////////

		public MethodInvoker( object obj, string name )
			: this( new ObjectInfo(obj, name) )
		{
		}


	}
}
